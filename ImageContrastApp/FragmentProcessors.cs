using System;
using System.Buffers;

namespace ImageContrastApp;

internal sealed class IdentityFragmentProcessor : IFragmentProcessor
{
    public LocalFragmentProcessorKind Kind => LocalFragmentProcessorKind.Identity;

    public void ProcessFragment(in FragmentProcessingContext context, Span<byte> destinationRgb)
    {
        int index = 0;

        for (int y = 0; y < context.Bounds.Height; y++)
        {
            int sourceY = context.Bounds.Y + y;

            for (int x = 0; x < context.Bounds.Width; x++)
            {
                int sourceX = context.Bounds.X + x;
                context.Source.GetRgb(sourceX, sourceY, out byte r, out byte g, out byte b);
                destinationRgb[index] = r;
                destinationRgb[index + 1] = g;
                destinationRgb[index + 2] = b;
                index += 3;
            }
        }
    }
}

internal sealed class SimpleLocalContrastProcessor : IFragmentProcessor
{
    public LocalFragmentProcessorKind Kind => LocalFragmentProcessorKind.SimpleLocalContrast;

    public void ProcessFragment(in FragmentProcessingContext context, Span<byte> destinationRgb)
    {
        int pixelCount = context.Bounds.Width * context.Bounds.Height;
        float[] rentedLuminance = ArrayPool<float>.Shared.Rent(pixelCount);
        Span<float> luminanceValues = rentedLuminance.AsSpan(0, pixelCount);

        try
        {
            float luminanceSum = 0f;
            int pixelIndex = 0;

            for (int y = 0; y < context.Bounds.Height; y++)
            {
                int sourceY = context.Bounds.Y + y;

                for (int x = 0; x < context.Bounds.Width; x++)
                {
                    int sourceX = context.Bounds.X + x;
                    float luminance = context.Source.GetLuminance(sourceX, sourceY);
                    luminanceValues[pixelIndex] = luminance;
                    luminanceSum += luminance;
                    pixelIndex++;
                }
            }

            float averageLuminance = luminanceSum / pixelCount;
            pixelIndex = 0;
            int outputIndex = 0;

            for (int y = 0; y < context.Bounds.Height; y++)
            {
                int sourceY = context.Bounds.Y + y;

                for (int x = 0; x < context.Bounds.Width; x++)
                {
                    int sourceX = context.Bounds.X + x;
                    context.Source.GetRgb(sourceX, sourceY, out byte r, out byte g, out byte b);

                    float sourceLuminance = luminanceValues[pixelIndex];
                    float targetLuminance = LuminanceHelper.Clamp01(
                        sourceLuminance + (context.Settings.ContrastFactor * (sourceLuminance - averageLuminance)));

                    float ratio = sourceLuminance > 0.0001f ? targetLuminance / sourceLuminance : 1f;

                    destinationRgb[outputIndex] = LuminanceHelper.ClampToByte(r * ratio);
                    destinationRgb[outputIndex + 1] = LuminanceHelper.ClampToByte(g * ratio);
                    destinationRgb[outputIndex + 2] = LuminanceHelper.ClampToByte(b * ratio);

                    pixelIndex++;
                    outputIndex += 3;
                }
            }
        }
        finally
        {
            ArrayPool<float>.Shared.Return(rentedLuminance);
        }
    }
}

internal sealed class FrequencyProportionalStretchProcessor : IFragmentProcessor
{
    public LocalFragmentProcessorKind Kind => LocalFragmentProcessorKind.FrequencyProportionalStretch;

    public void ProcessFragment(in FragmentProcessingContext context, Span<byte> destinationRgb)
    {
        throw new NotImplementedException("Frequency-proportional stretching is reserved for a later processor implementation.");
    }
}
