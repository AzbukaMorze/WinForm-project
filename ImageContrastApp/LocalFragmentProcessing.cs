using System;
using System.Buffers;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace ImageContrastApp;

internal enum LocalFragmentProcessorKind
{
    Identity,
    SimpleLocalContrast,
    FrequencyProportionalStretch
}

internal sealed class LocalFragmentSettings
{
    internal int FragmentWidth { get; init; } = 9;

    internal int FragmentHeight { get; init; } = 9;

    internal float ContrastFactor { get; init; } = 0.5f;

    internal bool UseMultithreading { get; init; }

    internal int MaxDegreeOfParallelism { get; init; } = Environment.ProcessorCount;

    internal LocalFragmentProcessorKind ProcessorKind { get; init; } = LocalFragmentProcessorKind.SimpleLocalContrast;
}

internal readonly record struct FragmentBounds(int X, int Y, int Width, int Height);

internal readonly record struct FragmentProcessingContext(
    BitmapPixelBuffer Source,
    FragmentBounds Bounds,
    LocalFragmentSettings Settings);

internal interface IFragmentProcessor
{
    LocalFragmentProcessorKind Kind { get; }

    void ProcessFragment(in FragmentProcessingContext context, Span<byte> destinationRgb);
}

internal static class LocalFragmentProcessorFactory
{
    internal static IFragmentProcessor Create(LocalFragmentProcessorKind kind)
    {
        return kind switch
        {
            LocalFragmentProcessorKind.Identity => new IdentityFragmentProcessor(),
            LocalFragmentProcessorKind.SimpleLocalContrast => new SimpleLocalContrastProcessor(),
            LocalFragmentProcessorKind.FrequencyProportionalStretch => new FrequencyProportionalStretchProcessor(),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown local fragment processor.")
        };
    }
}

internal static class LuminanceHelper
{
    internal static float FromRgbBytes(byte r, byte g, byte b)
    {
        return ((0.2126f * r) + (0.7152f * g) + (0.0722f * b)) / 255f;
    }

    internal static byte ClampToByte(float value)
    {
        if (value <= 0f)
        {
            return 0;
        }

        if (value >= 255f)
        {
            return byte.MaxValue;
        }

        return (byte)MathF.Round(value);
    }

    internal static float Clamp01(float value)
    {
        if (value < 0f)
        {
            return 0f;
        }

        if (value > 1f)
        {
            return 1f;
        }

        return value;
    }
}

internal static class LocalFragmentEngine
{
    internal static Bitmap Process(Bitmap image, LocalFragmentSettings settings)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(settings);

        if (settings.FragmentWidth < 1 || settings.FragmentHeight < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(settings), "Fragment dimensions must be positive.");
        }

        BitmapPixelBuffer source = BitmapPixelBuffer.FromBitmap(image);
        IFragmentProcessor processor = LocalFragmentProcessorFactory.Create(settings.ProcessorKind);

        long[] sumR = new long[source.Width * source.Height];
        long[] sumG = new long[source.Width * source.Height];
        long[] sumB = new long[source.Width * source.Height];
        int[] counts = new int[source.Width * source.Height];

        if (settings.UseMultithreading)
        {
            ProcessParallel(source, settings, processor, sumR, sumG, sumB, counts);
        }
        else
        {
            ProcessSequential(source, settings, processor, sumR, sumG, sumB, counts);
        }

        byte[] normalizedRgb = Normalize(source, sumR, sumG, sumB, counts);
        return source.ToBitmap(normalizedRgb);
    }

    private static void ProcessSequential(
        BitmapPixelBuffer source,
        LocalFragmentSettings settings,
        IFragmentProcessor processor,
        long[] sumR,
        long[] sumG,
        long[] sumB,
        int[] counts)
    {
        for (int y = 0; y < source.Height; y++)
        {
            ProcessFragmentRow(source, settings, processor, y, sumR, sumG, sumB, counts, useInterlocked: false);
        }
    }

    private static void ProcessParallel(
        BitmapPixelBuffer source,
        LocalFragmentSettings settings,
        IFragmentProcessor processor,
        long[] sumR,
        long[] sumG,
        long[] sumB,
        int[] counts)
    {
        ParallelOptions options = new ParallelOptions
        {
            MaxDegreeOfParallelism = Math.Max(1, settings.MaxDegreeOfParallelism)
        };

        Parallel.For(0, source.Height, options, y =>
        {
            ProcessFragmentRow(source, settings, processor, y, sumR, sumG, sumB, counts, useInterlocked: true);
        });
    }

    private static void ProcessFragmentRow(
        BitmapPixelBuffer source,
        LocalFragmentSettings settings,
        IFragmentProcessor processor,
        int fragmentY,
        long[] sumR,
        long[] sumG,
        long[] sumB,
        int[] counts,
        bool useInterlocked)
    {
        for (int fragmentX = 0; fragmentX < source.Width; fragmentX++)
        {
            FragmentBounds bounds = new FragmentBounds(
                fragmentX,
                fragmentY,
                Math.Min(settings.FragmentWidth, source.Width - fragmentX),
                Math.Min(settings.FragmentHeight, source.Height - fragmentY));

            int fragmentPixelCount = bounds.Width * bounds.Height;
            byte[] rented = ArrayPool<byte>.Shared.Rent(fragmentPixelCount * 3);

            try
            {
                Span<byte> output = rented.AsSpan(0, fragmentPixelCount * 3);
                FragmentProcessingContext context = new FragmentProcessingContext(source, bounds, settings);
                processor.ProcessFragment(context, output);
                AccumulateFragment(bounds, output, source.Width, sumR, sumG, sumB, counts, useInterlocked);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rented);
            }
        }
    }

    private static void AccumulateFragment(
        FragmentBounds bounds,
        ReadOnlySpan<byte> fragmentRgb,
        int imageWidth,
        long[] sumR,
        long[] sumG,
        long[] sumB,
        int[] counts,
        bool useInterlocked)
    {
        int rgbIndex = 0;

        for (int localY = 0; localY < bounds.Height; localY++)
        {
            int targetY = bounds.Y + localY;

            for (int localX = 0; localX < bounds.Width; localX++)
            {
                int targetX = bounds.X + localX;
                int pixelIndex = (targetY * imageWidth) + targetX;
                byte r = fragmentRgb[rgbIndex];
                byte g = fragmentRgb[rgbIndex + 1];
                byte b = fragmentRgb[rgbIndex + 2];

                if (useInterlocked)
                {
                    Interlocked.Add(ref sumR[pixelIndex], r);
                    Interlocked.Add(ref sumG[pixelIndex], g);
                    Interlocked.Add(ref sumB[pixelIndex], b);
                    Interlocked.Increment(ref counts[pixelIndex]);
                }
                else
                {
                    sumR[pixelIndex] += r;
                    sumG[pixelIndex] += g;
                    sumB[pixelIndex] += b;
                    counts[pixelIndex]++;
                }

                rgbIndex += 3;
            }
        }
    }

    private static byte[] Normalize(
        BitmapPixelBuffer source,
        long[] sumR,
        long[] sumG,
        long[] sumB,
        int[] counts)
    {
        byte[] normalizedRgb = new byte[source.Width * source.Height * 3];
        int rgbIndex = 0;

        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                int pixelIndex = (y * source.Width) + x;
                int count = counts[pixelIndex];

                if (count <= 0)
                {
                    source.GetRgb(x, y, out byte sourceR, out byte sourceG, out byte sourceB);
                    normalizedRgb[rgbIndex] = sourceR;
                    normalizedRgb[rgbIndex + 1] = sourceG;
                    normalizedRgb[rgbIndex + 2] = sourceB;
                }
                else
                {
                    normalizedRgb[rgbIndex] = LuminanceHelper.ClampToByte((float)sumR[pixelIndex] / count);
                    normalizedRgb[rgbIndex + 1] = LuminanceHelper.ClampToByte((float)sumG[pixelIndex] / count);
                    normalizedRgb[rgbIndex + 2] = LuminanceHelper.ClampToByte((float)sumB[pixelIndex] / count);
                }

                rgbIndex += 3;
            }
        }

        return normalizedRgb;
    }
}
