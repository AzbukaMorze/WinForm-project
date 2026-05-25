using System;
using System.Drawing;

namespace ImageContrastApp;

internal readonly record struct ImageBrightnessStats(
    float Mean,
    float StandardDeviation,
    byte Minimum,
    byte Maximum,
    float BlackClipPercent,
    float WhiteClipPercent)
{
    internal static ImageBrightnessStats FromBitmap(Bitmap image)
    {
        ArgumentNullException.ThrowIfNull(image);

        GrayImageBuffer buffer = GrayImageBuffer.FromBitmap(image);
        ReadOnlySpan<byte> values = buffer.Brightness;
        if (values.Length == 0)
        {
            return new ImageBrightnessStats(0f, 0f, 0, 0, 0f, 0f);
        }

        double sum = 0d;
        byte minimum = byte.MaxValue;
        byte maximum = byte.MinValue;
        int blackClipCount = 0;
        int whiteClipCount = 0;

        for (int i = 0; i < values.Length; i++)
        {
            byte value = values[i];
            sum += value;
            minimum = Math.Min(minimum, value);
            maximum = Math.Max(maximum, value);

            if (value == 0)
            {
                blackClipCount++;
            }
            else if (value == byte.MaxValue)
            {
                whiteClipCount++;
            }
        }

        float mean = (float)(sum / values.Length);
        float standardDeviation = LocalFragmentMath.ComputePopulationStandardDeviation(values, mean);
        float blackClipPercent = blackClipCount * 100f / values.Length;
        float whiteClipPercent = whiteClipCount * 100f / values.Length;

        return new ImageBrightnessStats(
            mean,
            standardDeviation,
            minimum,
            maximum,
            blackClipPercent,
            whiteClipPercent);
    }
}
