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
        float globalMean = LocalFragmentMath.ComputeMean(source.Brightness);
        float globalStandardDeviation = LocalFragmentMath.ComputePopulationStandardDeviation(source.Brightness, globalMean);
        float contrastCoefficient = globalStandardDeviation > LocalFragmentMath.Epsilon
            ? (targetStandardDeviation / globalStandardDeviation) - 1f
            : 0f;

        if (useMultithreading)
        {
            ProcessParallel(source, result, contrastCoefficient, windowWidth, windowHeight, maxDegreeOfParallelism);
        }
        else
        {
            ProcessSequential(source, result, contrastCoefficient, windowWidth, windowHeight);
        }

        return source.ToBitmap(result);
    }

    private static void ProcessSequential(
        GrayImageBuffer source,
        byte[] result,
        float contrastCoefficient,
        int windowWidth,
        int windowHeight)
    {
        for (int y = 0; y < source.Height; y++)
        {
            ProcessRow(source, result, contrastCoefficient, windowWidth, windowHeight, y);
        }
    }

    private static void ProcessParallel(
        GrayImageBuffer source,
        byte[] result,
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
            ProcessRow(source, result, contrastCoefficient, windowWidth, windowHeight, y);
        });
    }

    private static void ProcessRow(
        GrayImageBuffer source,
        byte[] result,
        float contrastCoefficient,
        int windowWidth,
        int windowHeight,
        int y)
    {
        for (int x = 0; x < source.Width; x++)
        {
            FragmentBounds bounds = CreateBounds(source, x, y, windowWidth, windowHeight);
            float localMean = ComputeLocalMean(source, bounds);
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

    private static float ComputeLocalMean(GrayImageBuffer source, FragmentBounds bounds)
    {
        double sum = 0d;

        for (int localY = 0; localY < bounds.Height; localY++)
        {
            int sourceY = bounds.Y + localY;

            for (int localX = 0; localX < bounds.Width; localX++)
            {
                sum += source.GetBrightness(bounds.X + localX, sourceY);
            }
        }

        return (float)(sum / (bounds.Width * bounds.Height));
    }
}
