using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImageContrastApp;

internal static class ImageContrastProcessor
{
    internal static Bitmap AdjustGlobalContrast(Bitmap image, float globalContrastK)
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
            float[] luminanceValues = new float[source.Width * source.Height];

            Marshal.Copy(sourceData.Scan0, sourceBuffer, 0, sourceBytes);

            double luminanceSum = 0d;
            int pixelIndex = 0;
            for (int y = 0; y < source.Height; y++)
            {
                int sourceRow = sourceData.Stride >= 0
                    ? y * sourceStride
                    : (source.Height - 1 - y) * sourceStride;

                for (int x = 0; x < source.Width; x++)
                {
                    int sIndex = sourceRow + (x * 4);
                    float b = sourceBuffer[sIndex] / 255f;
                    float g = sourceBuffer[sIndex + 1] / 255f;
                    float r = sourceBuffer[sIndex + 2] / 255f;
                    float luminance = (0.299f * r) + (0.587f * g) + (0.114f * b);

                    luminanceValues[pixelIndex] = luminance;
                    luminanceSum += luminance;
                    pixelIndex++;
                }
            }

            float averageLuminance = (float)(luminanceSum / luminanceValues.Length);

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

                    float b = sourceBuffer[sIndex] / 255f;
                    float g = sourceBuffer[sIndex + 1] / 255f;
                    float r = sourceBuffer[sIndex + 2] / 255f;
                    float sourceLuminance = luminanceValues[pixelIndex];

                    // Average-based global contrast: z = y + k * (y - y_avg)
                    float targetLuminance = Clamp01(sourceLuminance + (globalContrastK * (sourceLuminance - averageLuminance)));
                    float ratio = sourceLuminance > 0.0001f ? targetLuminance / sourceLuminance : 1f;

                    float rOut = Clamp01(r * ratio);
                    float gOut = Clamp01(g * ratio);
                    float bOut = Clamp01(b * ratio);

                    resultBuffer[dIndex] = (byte)(bOut * 255f);
                    resultBuffer[dIndex + 1] = (byte)(gOut * 255f);
                    resultBuffer[dIndex + 2] = (byte)(rOut * 255f);
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

    private static float Clamp01(float value)
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
