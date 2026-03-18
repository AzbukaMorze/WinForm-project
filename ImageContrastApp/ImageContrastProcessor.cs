using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImageContrastApp;

internal static class ImageContrastProcessor
{
    internal static Bitmap AdjustContrast(Bitmap image, float contrastFactor)
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

            Marshal.Copy(sourceData.Scan0, sourceBuffer, 0, sourceBytes);

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

                    r = Clamp01(((r - 0.5f) * contrastFactor) + 0.5f);
                    g = Clamp01(((g - 0.5f) * contrastFactor) + 0.5f);
                    b = Clamp01(((b - 0.5f) * contrastFactor) + 0.5f);

                    resultBuffer[dIndex] = (byte)(b * 255f);
                    resultBuffer[dIndex + 1] = (byte)(g * 255f);
                    resultBuffer[dIndex + 2] = (byte)(r * 255f);
                    resultBuffer[dIndex + 3] = sourceBuffer[sIndex + 3];
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
