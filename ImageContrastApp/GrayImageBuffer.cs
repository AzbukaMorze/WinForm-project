using System;
using System.Drawing;

namespace ImageContrastApp;

internal sealed class GrayImageBuffer
{
    private readonly BitmapPixelBuffer colorCarrier;
    private readonly byte[] brightness;

    private GrayImageBuffer(int width, int height, byte[] brightness, BitmapPixelBuffer colorCarrier)
    {
        Width = width;
        Height = height;
        this.brightness = brightness;
        this.colorCarrier = colorCarrier;
    }

    internal int Width { get; }

    internal int Height { get; }

    internal ReadOnlySpan<byte> Brightness => brightness;

    internal static GrayImageBuffer FromBitmap(Bitmap image)
    {
        BitmapPixelBuffer source = BitmapPixelBuffer.FromBitmap(image);
        byte[] brightness = new byte[source.Width * source.Height];
        int index = 0;

        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                source.GetRgb(x, y, out byte r, out byte g, out byte b);
                brightness[index] = LocalFragmentMath.ToByteBrightness(r, g, b);
                index++;
            }
        }

        return new GrayImageBuffer(source.Width, source.Height, brightness, source);
    }

    internal byte GetBrightness(int x, int y)
    {
        return brightness[(y * Width) + x];
    }

    internal Bitmap ToBitmap(ReadOnlySpan<byte> grayValues)
    {
        if (grayValues.Length != brightness.Length)
        {
            throw new ArgumentException("Grayscale buffer size does not match the image dimensions.", nameof(grayValues));
        }

        byte[] rgbValues = new byte[grayValues.Length * 3];
        int grayIndex = 0;
        int rgbIndex = 0;

        while (grayIndex < grayValues.Length)
        {
            byte gray = grayValues[grayIndex];
            rgbValues[rgbIndex] = gray;
            rgbValues[rgbIndex + 1] = gray;
            rgbValues[rgbIndex + 2] = gray;
            grayIndex++;
            rgbIndex += 3;
        }

        return colorCarrier.ToBitmap(rgbValues);
    }
}
