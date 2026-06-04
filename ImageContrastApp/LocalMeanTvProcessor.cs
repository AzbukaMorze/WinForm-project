using System;
using System.Drawing;
using System.Threading.Tasks;

namespace ImageContrastApp;

internal static class LocalMeanTvProcessor
{
    internal static Bitmap AdjustContrast(
        Bitmap image,
        float targetStandardDeviation,
        int windowWidth,
        int windowHeight,
        bool useMultithreading,
        int maxDegreeOfParallelism)
    {
        ArgumentNullException.ThrowIfNull(image);

        if (windowWidth < 1 || windowHeight < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(windowWidth), "Window dimensions must be positive.");
        }

        GrayImageBuffer source = GrayImageBuffer.FromBitmap(image);
        byte[] result = new byte[source.Width * source.Height];
        long[] prefixSums = BuildPrefixSums(source);
        float globalMean = LocalFragmentMath.ComputeMean(source.Brightness);
        float globalStandardDeviation = LocalFragmentMath.ComputePopulationStandardDeviation(source.Brightness, globalMean);
        float contrastCoefficient = globalStandardDeviation > LocalFragmentMath.Epsilon
            ? (targetStandardDeviation / globalStandardDeviation) - 1f
            : 0f;

        if (useMultithreading)
        {
            ProcessParallel(source, result, prefixSums, contrastCoefficient, windowWidth, windowHeight, maxDegreeOfParallelism);
        }
        else
        {
            ProcessSequential(source, result, prefixSums, contrastCoefficient, windowWidth, windowHeight);
        }

        return source.ToBitmap(result);
    }

    private static void ProcessSequential(
        GrayImageBuffer source,
        byte[] result,
        long[] prefixSums,
        float contrastCoefficient,
        int windowWidth,
        int windowHeight)
    {
        for (int y = 0; y < source.Height; y++)
        {
            ProcessRow(source, result, prefixSums, contrastCoefficient, windowWidth, windowHeight, y);
        }
    }

    private static void ProcessParallel(
        GrayImageBuffer source,
        byte[] result,
        long[] prefixSums,
        float contrastCoefficient,
        int windowWidth,
        int windowHeight,
        int maxDegreeOfParallelism)
    {
        ParallelOptions options = new()
        {
            MaxDegreeOfParallelism = Math.Max(1, maxDegreeOfParallelism)
        };

        Parallel.For(0, source.Height, options, y =>
        {
            ProcessRow(source, result, prefixSums, contrastCoefficient, windowWidth, windowHeight, y);
        });
    }

    private static void ProcessRow(
        GrayImageBuffer source,
        byte[] result,
        long[] prefixSums,
        float contrastCoefficient,
        int windowWidth,
        int windowHeight,
        int y)
    {
        for (int x = 0; x < source.Width; x++)
        {
            FragmentBounds bounds = CreateBounds(source, x, y, windowWidth, windowHeight);
            float localMean = ComputeLocalMean(prefixSums, source.Width, bounds);
            float sourceBrightness = source.GetBrightness(x, y);
            float transformed = sourceBrightness + (contrastCoefficient * (sourceBrightness - localMean));
            result[(y * source.Width) + x] = LocalFragmentMath.RoundClamp(transformed);
        }
    }

    private static FragmentBounds CreateBounds(GrayImageBuffer source, int x, int y, int windowWidth, int windowHeight)
    {
        return new FragmentBounds(
            x,
            y,
            Math.Min(windowWidth, source.Width - x),
            Math.Min(windowHeight, source.Height - y));
    }

    private static long[] BuildPrefixSums(GrayImageBuffer source)
    {
        int prefixStride = source.Width + 1;
        long[] prefixSums = new long[prefixStride * (source.Height + 1)];
        ReadOnlySpan<byte> brightness = source.Brightness;

        for (int y = 0; y < source.Height; y++)
        {
            long rowSum = 0L;
            int sourceRowOffset = y * source.Width;
            int prefixRowOffset = (y + 1) * prefixStride;
            int previousPrefixRowOffset = y * prefixStride;

            for (int x = 0; x < source.Width; x++)
            {
                rowSum += brightness[sourceRowOffset + x];
                prefixSums[prefixRowOffset + x + 1] = prefixSums[previousPrefixRowOffset + x + 1] + rowSum;
            }
        }

        return prefixSums;
    }

    private static float ComputeLocalMean(long[] prefixSums, int imageWidth, FragmentBounds bounds)
    {
        int prefixStride = imageWidth + 1;
        int left = bounds.X;
        int top = bounds.Y;
        int right = bounds.X + bounds.Width;
        int bottom = bounds.Y + bounds.Height;

        long sum = prefixSums[(bottom * prefixStride) + right]
            - prefixSums[(top * prefixStride) + right]
            - prefixSums[(bottom * prefixStride) + left]
            + prefixSums[(top * prefixStride) + left];

        return (float)((double)sum / (bounds.Width * bounds.Height));
    }
}
