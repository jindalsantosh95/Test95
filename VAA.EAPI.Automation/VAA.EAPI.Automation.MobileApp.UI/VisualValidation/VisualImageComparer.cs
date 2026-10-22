using SkiaSharp;

namespace VAA.EAPI.Automation.MobileApp.UI.VisualValidation;

public sealed class VisualImageComparer
{
    public VisualComparisonResult Compare(
        string baselineImagePath,
        byte[] actualPng,
        string artifactDirectory,
        string artifactName,
        VisualComparisonOptions? options = null)
    {
        VisualComparisonOptions comparisonOptions = options ?? VisualComparisonOptions.Default;
        comparisonOptions.Validate();

        if (!File.Exists(baselineImagePath))
        {
            throw new FileNotFoundException(
                "The approved visual baseline is missing. Export and review the pinned Figma node before enabling this check.",
                baselineImagePath);
        }

        return Compare(
            File.ReadAllBytes(baselineImagePath),
            baselineImagePath,
            actualPng,
            artifactDirectory,
            artifactName,
            comparisonOptions);
    }

    public VisualComparisonResult Compare(
        byte[] baselinePng,
        string baselineDescription,
        byte[] actualPng,
        string artifactDirectory,
        string artifactName,
        VisualComparisonOptions? options = null)
    {
        VisualComparisonOptions comparisonOptions = options ?? VisualComparisonOptions.Default;
        comparisonOptions.Validate();

        Directory.CreateDirectory(artifactDirectory);
        string safeArtifactName = CreateSafeArtifactName(artifactName);
        string expectedImagePath = Path.Combine(artifactDirectory, $"{safeArtifactName}.expected.png");
        string actualImagePath = Path.Combine(artifactDirectory, $"{safeArtifactName}.actual.png");
        string differenceImagePath = Path.Combine(artifactDirectory, $"{safeArtifactName}.diff.png");
        File.WriteAllBytes(expectedImagePath, baselinePng);
        File.WriteAllBytes(actualImagePath, actualPng);

        using SKBitmap baseline = Decode(baselinePng, baselineDescription);
        using SKBitmap actual = Decode(actualPng, "the Appium screenshot");
        bool dimensionsMatch = baseline.Width == actual.Width && baseline.Height == actual.Height;
        int comparedWidth = Math.Max(baseline.Width, actual.Width);
        int comparedHeight = Math.Max(baseline.Height, actual.Height);
        int comparedPixels = checked(comparedWidth * comparedHeight);
        int mismatchedPixels = 0;

        using SKBitmap difference = new(new SKImageInfo(comparedWidth, comparedHeight));
        for (int y = 0; y < comparedHeight; y++)
        {
            for (int x = 0; x < comparedWidth; x++)
            {
                bool baselineContainsPixel = x < baseline.Width && y < baseline.Height;
                bool actualContainsPixel = x < actual.Width && y < actual.Height;
                SKColor baselinePixel = baselineContainsPixel ? baseline.GetPixel(x, y) : SKColors.Transparent;
                SKColor actualPixel = actualContainsPixel ? actual.GetPixel(x, y) : SKColors.Transparent;
                bool pixelMatches = baselineContainsPixel &&
                                    actualContainsPixel &&
                                    IsWithinTolerance(baselinePixel, actualPixel, comparisonOptions.ChannelTolerance);

                if (!pixelMatches)
                {
                    mismatchedPixels++;
                }

                difference.SetPixel(x, y, CreateDifferencePixel(actualPixel, pixelMatches));
            }
        }

        SavePng(difference, differenceImagePath);
        double mismatchRatio = (double)mismatchedPixels / comparedPixels;
        bool isMatch = dimensionsMatch && mismatchRatio <= comparisonOptions.MaximumMismatchRatio;

        return new VisualComparisonResult(
            isMatch,
            dimensionsMatch,
            baseline.Width,
            baseline.Height,
            actual.Width,
            actual.Height,
            mismatchedPixels,
            comparedPixels,
            mismatchRatio,
            expectedImagePath,
            actualImagePath,
            differenceImagePath);
    }

    private static SKBitmap Decode(byte[] imageBytes, string source)
        => SKBitmap.Decode(imageBytes)
           ?? throw new InvalidDataException($"Could not decode PNG image from {source}.");

    private static bool IsWithinTolerance(SKColor expected, SKColor actual, byte tolerance)
        => Math.Abs(expected.Red - actual.Red) <= tolerance &&
           Math.Abs(expected.Green - actual.Green) <= tolerance &&
           Math.Abs(expected.Blue - actual.Blue) <= tolerance &&
           Math.Abs(expected.Alpha - actual.Alpha) <= tolerance;

    private static SKColor CreateDifferencePixel(SKColor actual, bool pixelMatches)
    {
        if (!pixelMatches)
        {
            return new SKColor(255, 0, 128, 255);
        }

        byte luminance = (byte)((actual.Red + actual.Green + actual.Blue) / 3);
        return new SKColor(luminance, luminance, luminance, 96);
    }

    private static string CreateSafeArtifactName(string artifactName)
    {
        char[] invalidCharacters = Path.GetInvalidFileNameChars();
        char[] safeCharacters = artifactName
            .Select(character => invalidCharacters.Contains(character) ? '-' : character)
            .ToArray();
        return new string(safeCharacters);
    }

    private static void SavePng(SKBitmap bitmap, string path)
    {
        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData encodedImage = image.Encode(SKEncodedImageFormat.Png, 100);
        using FileStream output = File.Create(path);
        encodedImage.SaveTo(output);
    }
}
