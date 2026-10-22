using SkiaSharp;

namespace VAA.EAPI.Automation.MobileApp.UI.VisualValidation;

public static class VisualImageNormalizer
{
    public static byte[] CompositeTransparencyOnWhite(byte[] png)
    {
        using SKBitmap source = SKBitmap.Decode(png)
                                ?? throw new InvalidDataException("Could not decode the visual baseline as PNG.");
        using SKBitmap flattened = new(new SKImageInfo(source.Width, source.Height));

        for (int y = 0; y < source.Height; y++)
        {
            for (int x = 0; x < source.Width; x++)
            {
                SKColor colour = source.GetPixel(x, y);
                int alpha = colour.Alpha;
                flattened.SetPixel(x, y, new SKColor(
                    Composite(colour.Red, alpha),
                    Composite(colour.Green, alpha),
                    Composite(colour.Blue, alpha),
                    byte.MaxValue));
            }
        }

        using SKImage image = SKImage.FromBitmap(flattened);
        using SKData encoded = image.Encode(SKEncodedImageFormat.Png, 100);
        return encoded.ToArray();
    }

    private static byte Composite(byte channel, int alpha)
        => (byte)(((channel * alpha) + (byte.MaxValue * (byte.MaxValue - alpha))) / byte.MaxValue);
}
