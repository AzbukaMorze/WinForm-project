using System;
using System.Buffers;
using System.Threading.Tasks;

namespace ImageContrastApp;

internal enum LocalFragmentProcessorKind
{
    Method1,
    Method2,
    Method3
}

internal sealed class LocalFragmentSettings
{
    internal const float AdaptiveQReferenceDeviation = 80f;
    internal const float AdaptiveSigmaReferenceDeviation = 80f;
    internal const float AdaptiveSigmaBlend = 0.5f;

    internal int FragmentWidth { get; init; } = 9;

    internal int FragmentHeight { get; init; } = 9;

    internal bool UseMultithreading { get; init; }

    internal int MaxDegreeOfParallelism { get; init; } = Environment.ProcessorCount;

    internal LocalFragmentProcessorKind ProcessorKind { get; init; } = LocalFragmentProcessorKind.Method1;
}

internal readonly record struct FragmentBounds(int X, int Y, int Width, int Height);

internal static class LocalFragmentEngine
{
    internal static System.Drawing.Bitmap Process(System.Drawing.Bitmap image, LocalFragmentSettings settings)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(settings);

        if (settings.FragmentWidth < 1 || settings.FragmentHeight < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(settings), "Fragment dimensions must be positive.");
        }

        GrayImageBuffer source = GrayImageBuffer.FromBitmap(image);
        byte[] result = new byte[source.Width * source.Height];
        bool[] writtenMask = new bool[source.Width * source.Height];
        float globalMean = LocalFragmentMath.ComputeMean(source.Brightness);
        float globalStandardDeviation = LocalFragmentMath.ComputePopulationStandardDeviation(source.Brightness, globalMean);
        float targetStandardDeviation = LocalFragmentMath.ComputeAdaptiveTargetStandardDeviation(globalStandardDeviation);

        if (settings.UseMultithreading)
        {
            ProcessParallel(source, settings, result, writtenMask, globalMean, globalStandardDeviation, targetStandardDeviation);
        }
        else
        {
            ProcessSequential(source, settings, result, writtenMask, globalMean, globalStandardDeviation, targetStandardDeviation);
        }

        return source.ToBitmap(result);
    }

    private static void ProcessSequential(
        GrayImageBuffer source,
        LocalFragmentSettings settings,
        byte[] result,
        bool[] writtenMask,
        float globalMean,
        float globalStandardDeviation,
        float targetStandardDeviation)
    {
        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                FragmentBounds bounds = CreateBounds(source, settings, x, y);
                byte[] fragment = ProcessFragment(source, bounds, settings, globalMean, globalStandardDeviation, targetStandardDeviation);
                ApplySequentialOverlap(fragment, bounds, source.Width, result, writtenMask);
            }
        }
    }

    private static void ProcessParallel(
        GrayImageBuffer source,
        LocalFragmentSettings settings,
        byte[] result,
        bool[] writtenMask,
        float globalMean,
        float globalStandardDeviation,
        float targetStandardDeviation)
    {
        ParallelOptions options = new ParallelOptions
        {
            MaxDegreeOfParallelism = Math.Max(1, settings.MaxDegreeOfParallelism)
        };

        byte[][] rowFragments = new byte[source.Width][];

        for (int y = 0; y < source.Height; y++)
        {
            int rowY = y;
            Parallel.For(0, source.Width, options, x =>
            {
                FragmentBounds bounds = CreateBounds(source, settings, x, rowY);
                rowFragments[x] = ProcessFragment(source, bounds, settings, globalMean, globalStandardDeviation, targetStandardDeviation);
            });

            for (int x = 0; x < source.Width; x++)
            {
                FragmentBounds bounds = CreateBounds(source, settings, x, y);
                ApplySequentialOverlap(rowFragments[x], bounds, source.Width, result, writtenMask);
            }
        }
    }

    private static FragmentBounds CreateBounds(GrayImageBuffer source, LocalFragmentSettings settings, int x, int y)
    {
        return new FragmentBounds(
            x,
            y,
            Math.Min(settings.FragmentWidth, source.Width - x),
            Math.Min(settings.FragmentHeight, source.Height - y));
    }

    private static byte[] ProcessFragment(
        GrayImageBuffer source,
        FragmentBounds bounds,
        LocalFragmentSettings settings,
        float globalMean,
        float globalStandardDeviation,
        float targetStandardDeviation)
    {
        int pixelCount = bounds.Width * bounds.Height;
        byte[] fragment = ArrayPool<byte>.Shared.Rent(pixelCount);
        Span<byte> output = fragment.AsSpan(0, pixelCount);
        float[] values = ArrayPool<float>.Shared.Rent(pixelCount);
        Span<float> fragmentValues = values.AsSpan(0, pixelCount);

        try
        {
            int index = 0;
            float sum = 0f;

            for (int localY = 0; localY < bounds.Height; localY++)
            {
                int sourceY = bounds.Y + localY;

                for (int localX = 0; localX < bounds.Width; localX++)
                {
                    float value = source.GetBrightness(bounds.X + localX, sourceY);
                    fragmentValues[index] = value;
                    sum += value;
                    index++;
                }
            }

            float fragmentMean = sum / pixelCount;
            float fragmentStandardDeviation = LocalFragmentMath.ComputePopulationStandardDeviation(fragmentValues, fragmentMean);
            float? coefficient = ResolveCoefficient(settings, globalStandardDeviation, fragmentStandardDeviation, targetStandardDeviation);

            if (coefficient is null)
            {
                for (int i = 0; i < pixelCount; i++)
                {
                    output[i] = LocalFragmentMath.RoundClamp(fragmentValues[i]);
                }

                return output.ToArray();
            }

            for (int i = 0; i < pixelCount; i++)
            {
                float sourceValue = fragmentValues[i];
                float transformed = sourceValue + (coefficient.Value * (sourceValue - fragmentMean));
                output[i] = LocalFragmentMath.RoundClamp(transformed);
            }

            return output.ToArray();
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(fragment);
            ArrayPool<float>.Shared.Return(values);
        }
    }

    private static float? ResolveCoefficient(
        LocalFragmentSettings settings,
        float globalStandardDeviation,
        float fragmentStandardDeviation,
        float targetStandardDeviation)
    {
        if (globalStandardDeviation <= LocalFragmentMath.Epsilon)
        {
            return 0f;
        }

        return settings.ProcessorKind switch
        {
            LocalFragmentProcessorKind.Method1 => (targetStandardDeviation / globalStandardDeviation) - 1f,
            LocalFragmentProcessorKind.Method2 => fragmentStandardDeviation <= LocalFragmentMath.Epsilon
                ? null
                : (targetStandardDeviation / fragmentStandardDeviation) - 1f,
            LocalFragmentProcessorKind.Method3 => fragmentStandardDeviation <= LocalFragmentMath.Epsilon
                ? null
                : ((targetStandardDeviation / fragmentStandardDeviation)
                    * MathF.Pow(fragmentStandardDeviation / globalStandardDeviation, 1f - ComputeAdaptiveQ(globalStandardDeviation))) - 1f,
            _ => throw new ArgumentOutOfRangeException(nameof(settings.ProcessorKind), settings.ProcessorKind, "Unknown local fragment method.")
        };
    }

    private static float ComputeAdaptiveQ(float globalStandardDeviation)
    {
        // Adaptive rule:
        // q = clamp(1 - sigma_gl_y / 80, 0, 1)
        float q = 1f - (globalStandardDeviation / LocalFragmentSettings.AdaptiveQReferenceDeviation);
        return Math.Clamp(q, 0f, 1f);
    }

    private static void ApplySequentialOverlap(
        ReadOnlySpan<byte> fragment,
        FragmentBounds bounds,
        int imageWidth,
        byte[] result,
        bool[] writtenMask)
    {
        int index = 0;

        for (int localY = 0; localY < bounds.Height; localY++)
        {
            int targetY = bounds.Y + localY;

            for (int localX = 0; localX < bounds.Width; localX++)
            {
                int pixelIndex = (targetY * imageWidth) + bounds.X + localX;
                byte newValue = fragment[index];

                if (!writtenMask[pixelIndex])
                {
                    result[pixelIndex] = newValue;
                    writtenMask[pixelIndex] = true;
                }
                else
                {
                    result[pixelIndex] = LocalFragmentMath.RoundClamp((result[pixelIndex] + newValue) / 2f);
                }

                index++;
            }
        }
    }
}

internal static class LocalFragmentMath
{
    internal const float Epsilon = 0.0001f;

    internal static byte ToByteBrightness(byte r, byte g, byte b)
    {
        return RoundClamp((0.2126f * r) + (0.7152f * g) + (0.0722f * b));
    }

    internal static float ComputeAdaptiveTargetStandardDeviation(float sourceStandardDeviation)
    {
        float target = sourceStandardDeviation
            + (LocalFragmentSettings.AdaptiveSigmaBlend * (LocalFragmentSettings.AdaptiveSigmaReferenceDeviation - sourceStandardDeviation));
        return Math.Clamp(target, 0f, 128f);
    }

    internal static float ComputeMean(ReadOnlySpan<byte> values)
    {
        double sum = 0d;

        for (int i = 0; i < values.Length; i++)
        {
            sum += values[i];
        }

        return (float)(sum / values.Length);
    }

    internal static float ComputePopulationStandardDeviation(ReadOnlySpan<byte> values, float mean)
    {
        double squaredDifferenceSum = 0d;

        for (int i = 0; i < values.Length; i++)
        {
            double difference = values[i] - mean;
            squaredDifferenceSum += difference * difference;
        }

        return (float)Math.Sqrt(squaredDifferenceSum / values.Length);
    }

    internal static float ComputePopulationStandardDeviation(ReadOnlySpan<float> values, float mean)
    {
        double squaredDifferenceSum = 0d;

        for (int i = 0; i < values.Length; i++)
        {
            double difference = values[i] - mean;
            squaredDifferenceSum += difference * difference;
        }

        return (float)Math.Sqrt(squaredDifferenceSum / values.Length);
    }

    internal static byte RoundClamp(float value)
    {
        if (value <= 0f)
        {
            return 0;
        }

        if (value >= 255f)
        {
            return byte.MaxValue;
        }

        return (byte)Math.Round(value, MidpointRounding.AwayFromZero);
    }
}
