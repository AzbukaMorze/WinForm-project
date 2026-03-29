using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImageContrastApp;

internal sealed class BitmapPixelBuffer
{
    private readonly byte[] pixels;

    private BitmapPixelBuffer(int width, int height, byte[] pixels)
    {
        Width = width;
        Height = height;
        this.pixels = pixels;
    }

    internal int Width { get; }

    internal int Height { get; }

    internal static BitmapPixelBuffer FromBitmap(Bitmap image)
    {
        Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
        using Bitmap source = image.Clone(rect, PixelFormat.Format32bppArgb);
        BitmapData? sourceData = null;

        try
        {
            sourceData = source.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            int sourceStride = Math.Abs(sourceData.Stride);
            byte[] rawBuffer = new byte[sourceStride * source.Height];
            byte[] normalized = new byte[source.Width * source.Height * 4];

            Marshal.Copy(sourceData.Scan0, rawBuffer, 0, rawBuffer.Length);

            for (int y = 0; y < source.Height; y++)
            {
                int sourceRow = sourceData.Stride >= 0
                    ? y * sourceStride
                    : (source.Height - 1 - y) * sourceStride;

                Buffer.BlockCopy(rawBuffer, sourceRow, normalized, y * source.Width * 4, source.Width * 4);
            }

            return new BitmapPixelBuffer(source.Width, source.Height, normalized);
        }
        finally
        {
            if (sourceData is not null)
            {
                source.UnlockBits(sourceData);
            }
        }
    }

    internal Bitmap ToBitmap(ReadOnlySpan<byte> rgbValues)
    {
        if (rgbValues.Length != Width * Height * 3)
        {
            throw new ArgumentException("RGB buffer size does not match the image dimensions.", nameof(rgbValues));
        }

        Bitmap result = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
        Rectangle rect = new Rectangle(0, 0, Width, Height);
        BitmapData? resultData = null;

        try
        {
            resultData = result.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            int resultStride = Math.Abs(resultData.Stride);
            byte[] rawBuffer = new byte[resultStride * Height];

            for (int y = 0; y < Height; y++)
            {
                int sourceRow = y * Width * 3;
                int targetRow = resultData.Stride >= 0
                    ? y * resultStride
                    : (Height - 1 - y) * resultStride;

                for (int x = 0; x < Width; x++)
                {
                    int rgbIndex = sourceRow + (x * 3);
                    int pixelIndex = GetPixelIndex(x, y);
                    int targetIndex = targetRow + (x * 4);

                    rawBuffer[targetIndex] = rgbValues[rgbIndex + 2];
                    rawBuffer[targetIndex + 1] = rgbValues[rgbIndex + 1];
                    rawBuffer[targetIndex + 2] = rgbValues[rgbIndex];
                    rawBuffer[targetIndex + 3] = pixels[pixelIndex + 3];
                }
            }

            Marshal.Copy(rawBuffer, 0, resultData.Scan0, rawBuffer.Length);
        }
        finally
        {
            if (resultData is not null)
            {
                result.UnlockBits(resultData);
            }
        }

        return result;
    }

    internal void GetRgb(int x, int y, out byte r, out byte g, out byte b)
    {
        int index = GetPixelIndex(x, y);
        b = pixels[index];
        g = pixels[index + 1];
        r = pixels[index + 2];
    }

    internal byte GetAlpha(int x, int y)
    {
        return pixels[GetPixelIndex(x, y) + 3];
    }

    internal float GetLuminance(int x, int y)
    {
        GetRgb(x, y, out byte r, out byte g, out byte b);
        return LuminanceHelper.FromRgbBytes(r, g, b);
    }

    private int GetPixelIndex(int x, int y)
    {
        return ((y * Width) + x) * 4;
    }
}
