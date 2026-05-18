using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImageContrastApp;

internal static class ImageContrastProcessor
{
    internal static Bitmap AdjustGlobalContrast(Bitmap image, float targetStandardDeviation)
    {
        Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
        using Bitmap source = image.Clone(rect, PixelFormat.Format32bppArgb);
        Bitmap result = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb);

        BitmapData? sourceData = null;
        BitmapData? resultData = null;

        try
        {
            sourceData = source.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            resultData = result.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            int sourceStride = Math.Abs(sourceData.Stride);
            int resultStride = Math.Abs(resultData.Stride);

            int sourceBytes = sourceStride * source.Height;
            int resultBytes = resultStride * result.Height;

            byte[] sourceBuffer = new byte[sourceBytes];
            byte[] resultBuffer = new byte[resultBytes];
            byte[] brightnessValues = new byte[source.Width * source.Height];

            Marshal.Copy(sourceData.Scan0, sourceBuffer, 0, sourceBytes);

            double brightnessSum = 0d;
            int pixelIndex = 0;
            for (int y = 0; y < source.Height; y++)
            {
                int sourceRow = sourceData.Stride >= 0
                    ? y * sourceStride
                    : (source.Height - 1 - y) * sourceStride;

                for (int x = 0; x < source.Width; x++)
                {
                    int sIndex = sourceRow + (x * 4);
                    byte brightness = LocalFragmentMath.ToByteBrightness(sourceBuffer[sIndex + 2], sourceBuffer[sIndex + 1], sourceBuffer[sIndex]);

                    brightnessValues[pixelIndex] = brightness;
                    brightnessSum += brightness;
                    pixelIndex++;
                }
            }

            float averageBrightness = (float)(brightnessSum / brightnessValues.Length);
            float sourceStandardDeviation = LocalFragmentMath.ComputePopulationStandardDeviation(brightnessValues, averageBrightness);
            float contrastCoefficient = sourceStandardDeviation > 0.0001f
                ? (targetStandardDeviation / sourceStandardDeviation) - 1f
                : 0f;

            pixelIndex = 0;
            for (int y = 0; y < source.Height; y++)
            {
                int sourceRow = sourceData.Stride >= 0
                    ? y * sourceStride
                    : (source.Height - 1 - y) * sourceStride;

                int resultRow = resultData.Stride >= 0
                    ? y * resultStride
                    : (result.Height - 1 - y) * resultStride;

                for (int x = 0; x < source.Width; x++)
                {
                    int sIndex = sourceRow + (x * 4);
                    int dIndex = resultRow + (x * 4);
                    float sourceBrightness = brightnessValues[pixelIndex];

                    // Global television transform on grayscale brightness:
                    // z = y + k * (y - y_bar), with k = sigma_z / sigma_y - 1
                    float transformedBrightness = sourceBrightness + (contrastCoefficient * (sourceBrightness - averageBrightness));
                    byte gray = LocalFragmentMath.RoundClamp(transformedBrightness);

                    resultBuffer[dIndex] = gray;
                    resultBuffer[dIndex + 1] = gray;
                    resultBuffer[dIndex + 2] = gray;
                    resultBuffer[dIndex + 3] = sourceBuffer[sIndex + 3];
                    pixelIndex++;
                }
            }

            Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBytes);
        }
        finally
        {
            if (sourceData is not null)
            {
                source.UnlockBits(sourceData);
            }

            if (resultData is not null)
            {
                result.UnlockBits(resultData);
            }
        }

        return result;
    }

}
