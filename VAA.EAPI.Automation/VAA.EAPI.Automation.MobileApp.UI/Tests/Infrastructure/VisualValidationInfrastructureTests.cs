using Microsoft.Extensions.Configuration;
using SkiaSharp;
using VAA.EAPI.Automation.MobileApp.UI.VisualValidation;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

public sealed class VisualValidationInfrastructureTests
{
    [Fact]
    [Trait("Category", "MobileApp-VisualValidation-Offline")]
    [Trait("TestType", "Infrastructure")]
    public void VisualValidation_TC00_BrowserStackContract_PinsDeviceAndDisablesFallback()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(SearchPanelVisualRuntimeContract.ConfigurationOverrides)
            .Build();

        SearchPanelVisualRuntimeContract.ValidateConfiguration(configuration);
        Assert.Equal(
            "iPhone 17 Pro",
            SearchPanelVisualRuntimeContract.ConfigurationOverrides["BrowserStack:DeviceName"]);
        Assert.Equal(
            "false",
            SearchPanelVisualRuntimeContract.ConfigurationOverrides[
                "BrowserStack:DeviceFallbacksEnabled"]);
        Assert.Equal(
            "iOS",
            SearchPanelVisualRuntimeContract.ConfigurationOverrides["Platform"]);
        Assert.Equal(
            "iPhone 17 Pro",
            SearchPanelVisualRuntimeContract.ConfigurationOverrides["Appium:iOS:DeviceName"]);
        Assert.Equal(
            "26",
            SearchPanelVisualRuntimeContract.ConfigurationOverrides["Appium:iOS:PlatformVersion"]);
        Assert.Equal(
            "Light",
            SearchPanelVisualRuntimeContract.ConfigurationOverrides["IOSLaunch:Appearance"]);
        Assert.Equal(
            "en-GB",
            SearchPanelVisualRuntimeContract.ConfigurationOverrides["IOSLaunch:Locale"]);
        Assert.Equal(
            "Default",
            SearchPanelVisualRuntimeContract.ConfigurationOverrides["IOSLaunch:TextSize"]);
    }

    [Fact]
    [Trait("Category", "MobileApp-VisualValidation-Offline")]
    [Trait("TestType", "Infrastructure")]
    public void VisualValidation_TC01_IdenticalComponent_MatchesApprovedBaseline()
    {
        string workingDirectory = CreateWorkingDirectory();

        try
        {
            string baselinePath = Path.Combine(workingDirectory, "baseline.png");
            byte[] image = CreateSolidImage(8, 6, SKColors.Navy);
            File.WriteAllBytes(baselinePath, image);

            VisualComparisonResult result = new VisualImageComparer().Compare(
                baselinePath,
                image,
                workingDirectory,
                nameof(VisualValidation_TC01_IdenticalComponent_MatchesApprovedBaseline));

            Assert.True(result.IsMatch, result.Describe(baselinePath));
            Assert.Equal(0, result.MismatchedPixels);
            Assert.True(File.Exists(result.ExpectedImagePath));
            Assert.True(File.Exists(result.ActualImagePath));
            Assert.True(File.Exists(result.DifferenceImagePath));
        }
        finally
        {
            Directory.Delete(workingDirectory, true);
        }
    }

    [Fact]
    [Trait("Category", "MobileApp-VisualValidation-Offline")]
    [Trait("TestType", "Infrastructure")]
    public void VisualValidation_TC02_ChangedComponent_FailsAndWritesDiagnosticArtifacts()
    {
        string workingDirectory = CreateWorkingDirectory();

        try
        {
            string baselinePath = Path.Combine(workingDirectory, "baseline.png");
            File.WriteAllBytes(baselinePath, CreateSolidImage(4, 4, SKColors.Navy));
            byte[] actual = CreateSolidImage(4, 4, SKColors.Red);

            VisualComparisonResult result = new VisualImageComparer().Compare(
                baselinePath,
                actual,
                workingDirectory,
                nameof(VisualValidation_TC02_ChangedComponent_FailsAndWritesDiagnosticArtifacts));

            Assert.False(result.IsMatch);
            Assert.Equal(16, result.MismatchedPixels);
            Assert.Equal(1, result.MismatchRatio);
            Assert.True(File.Exists(result.ExpectedImagePath));
            Assert.True(File.Exists(result.ActualImagePath));
            Assert.True(File.Exists(result.DifferenceImagePath));
        }
        finally
        {
            Directory.Delete(workingDirectory, true);
        }
    }

    [Fact]
    [Trait("Category", "MobileApp-VisualValidation-Offline")]
    [Trait("TestType", "Infrastructure")]
    public void VisualValidation_TC03_RetinaScreenshot_CropsUsingViewportScale()
    {
        byte[] screenshot = CreateQuadrantImage();

        byte[] crop = new AppiumComponentScreenshot().Crop(
            screenshot,
            viewportWidth: 4,
            viewportHeight: 4,
            componentX: 2,
            componentY: 0,
            componentWidth: 2,
            componentHeight: 2);

        using SKBitmap cropped = SKBitmap.Decode(crop)
                                 ?? throw new InvalidDataException("Could not decode the cropped test image.");
        Assert.Equal(4, cropped.Width);
        Assert.Equal(4, cropped.Height);
        Assert.Equal(SKColors.Red, cropped.GetPixel(0, 0));
        Assert.Equal(SKColors.Red, cropped.GetPixel(3, 3));
    }

    [Fact]
    [Trait("Category", "MobileApp-VisualValidation-Offline")]
    [Trait("TestType", "Infrastructure")]
    public void VisualValidation_TC04_ResponsiveWhitespaceGrowth_PreservesMatchingAnchors()
    {
        string workingDirectory = CreateWorkingDirectory();

        try
        {
            byte[] baseline = CreateAnchoredImage(width: 10, middleColor: SKColors.White);
            byte[] actual = CreateAnchoredImage(width: 14, middleColor: SKColors.LightGray);

            IReadOnlyList<VisualComparisonResult> results = new AnchoredComponentComparer()
                .CompareHorizontalAnchors(
                    baselinePng: baseline,
                    baselineWidth: 10,
                    baselineHeight: 4,
                    actualPng: actual,
                    actualWidth: 14,
                    actualHeight: 4,
                    leftAnchorWidth: 3,
                    rightAnchorWidth: 3,
                    comparisonHeight: 4,
                    baselineDescription: "responsive baseline",
                    artifactDirectory: workingDirectory,
                    artifactName: nameof(VisualValidation_TC04_ResponsiveWhitespaceGrowth_PreservesMatchingAnchors),
                    options: new VisualComparisonOptions(0, 0));

            Assert.All(results, result => Assert.True(result.IsMatch));
        }
        finally
        {
            Directory.Delete(workingDirectory, true);
        }
    }

    [Fact]
    [Trait("Category", "MobileApp-VisualValidation-Offline")]
    [Trait("TestType", "Infrastructure")]
    public void VisualValidation_TC05_ChangedAnchor_FailsResponsiveComparison()
    {
        string workingDirectory = CreateWorkingDirectory();

        try
        {
            byte[] baseline = CreateAnchoredImage(width: 10, middleColor: SKColors.White);
            byte[] actual = CreateAnchoredImage(width: 14, middleColor: SKColors.LightGray, changeLeftAnchor: true);

            IReadOnlyList<VisualComparisonResult> results = new AnchoredComponentComparer()
                .CompareHorizontalAnchors(
                    baselinePng: baseline,
                    baselineWidth: 10,
                    baselineHeight: 4,
                    actualPng: actual,
                    actualWidth: 14,
                    actualHeight: 4,
                    leftAnchorWidth: 3,
                    rightAnchorWidth: 3,
                    comparisonHeight: 4,
                    baselineDescription: "responsive baseline",
                    artifactDirectory: workingDirectory,
                    artifactName: nameof(VisualValidation_TC05_ChangedAnchor_FailsResponsiveComparison),
                    options: new VisualComparisonOptions(0, 0));

            Assert.False(results[0].IsMatch);
            Assert.True(results[1].IsMatch);
        }
        finally
        {
            Directory.Delete(workingDirectory, true);
        }
    }

    [Fact]
    [Trait("Category", "MobileApp-VisualValidation-Offline")]
    [Trait("TestType", "Infrastructure")]
    public void VisualValidation_TC06_ChangedCentre_FailsResponsiveComparison()
    {
        string workingDirectory = CreateWorkingDirectory();

        try
        {
            byte[] baseline = CreateAnchoredImage(width: 10, middleColor: SKColors.White);
            byte[] actual = CreateAnchoredImage(width: 14, middleColor: SKColors.LightGray);

            IReadOnlyList<VisualComparisonResult> results = new AnchoredComponentComparer()
                .CompareHorizontalAnchorsAndCenter(
                    baselinePng: baseline,
                    baselineWidth: 10,
                    baselineHeight: 4,
                    actualPng: actual,
                    actualWidth: 14,
                    actualHeight: 4,
                    leftAnchorWidth: 3,
                    centerAnchorWidth: 4,
                    rightAnchorWidth: 3,
                    comparisonHeight: 4,
                    baselineDescription: "responsive baseline",
                    artifactDirectory: workingDirectory,
                    artifactName: nameof(VisualValidation_TC06_ChangedCentre_FailsResponsiveComparison),
                    options: new VisualComparisonOptions(0, 0));

            Assert.True(results[0].IsMatch);
            Assert.True(results[1].IsMatch);
            Assert.False(results[2].IsMatch);
        }
        finally
        {
            Directory.Delete(workingDirectory, true);
        }
    }

    [Fact]
    [Trait("Category", "MobileApp-VisualValidation-Offline")]
    [Trait("TestType", "Infrastructure")]
    public void VisualValidation_TC07_TransparentFigmaPixels_AreCompositedOnWhite()
    {
        byte[] transparent = CreateSolidImage(2, 2, SKColors.Transparent);
        byte[] flattened = VisualImageNormalizer.CompositeTransparencyOnWhite(transparent);

        using SKBitmap image = SKBitmap.Decode(flattened)
                               ?? throw new InvalidDataException("Could not decode the normalised image.");
        Assert.Equal(SKColors.White, image.GetPixel(0, 0));
        Assert.Equal(byte.MaxValue, image.GetPixel(0, 0).Alpha);
    }

    [Fact]
    [Trait("Category", "MobileApp-VisualValidation-Offline")]
    [Trait("TestType", "Infrastructure")]
    public void VisualValidation_TC08_NaNMismatchRatio_IsRejected()
    {
        VisualComparisonOptions options = new(ChannelTolerance: 8, MaximumMismatchRatio: double.NaN);

        Assert.Throws<ArgumentOutOfRangeException>(options.Validate);
    }

    [Fact]
    [Trait("Category", "MobileApp-VisualValidation-Offline")]
    [Trait("TestType", "Infrastructure")]
    public void VisualValidation_TC09_Manifest_ProvidesExecutableContracts()
    {
        VisualBaselineManifest manifest = VisualBaselineManifest.Load(AppContext.BaseDirectory);
        VisualBaselineContract contract = manifest.Get("search-book-flying-from-selection-row-ios");

        Assert.Equal(14, manifest.Baselines.Count);
        Assert.DoesNotContain(
            manifest.Baselines,
            baseline => baseline.Status.Equals("blocked", StringComparison.OrdinalIgnoreCase));
        Assert.Equal("NativeFlightSearchFlyingFromButton", contract.AccessibilityId);
        Assert.Equal("horizontal-anchors", contract.Comparison.Strategy);
        Assert.Equal("iPhone 17 Pro", contract.Runtime.Device);
    }

    [Theory]
    [Trait("Category", "MobileApp-VisualValidation-Offline")]
    [Trait("TestType", "Infrastructure")]
    [InlineData(255, 0.06)]
    [InlineData(24, 1.0)]
    public void VisualValidation_TC10_VacuousComparisonThreshold_IsRejected(
        int channelTolerance,
        double maximumMismatchRatio)
    {
        VisualComparisonOptions options = new((byte)channelTolerance, maximumMismatchRatio);

        Assert.Throws<ArgumentOutOfRangeException>(options.Validate);
    }

    private static string CreateWorkingDirectory()
    {
        string path = Path.Combine(Path.GetTempPath(), $"mobile-visual-validation-{Guid.NewGuid():N}");
        Directory.CreateDirectory(path);
        return path;
    }

    private static byte[] CreateSolidImage(int width, int height, SKColor color)
    {
        using SKBitmap bitmap = new(new SKImageInfo(width, height));
        bitmap.Erase(color);
        return Encode(bitmap);
    }

    private static byte[] CreateQuadrantImage()
    {
        using SKBitmap bitmap = new(new SKImageInfo(8, 8));
        bitmap.Erase(SKColors.Blue);
        for (int y = 0; y < 4; y++)
        {
            for (int x = 4; x < 8; x++)
            {
                bitmap.SetPixel(x, y, SKColors.Red);
            }
        }

        return Encode(bitmap);
    }

    private static byte[] CreateAnchoredImage(
        int width,
        SKColor middleColor,
        bool changeLeftAnchor = false)
    {
        using SKBitmap bitmap = new(new SKImageInfo(width, 4));
        bitmap.Erase(middleColor);
        SKColor leftColor = changeLeftAnchor ? SKColors.Yellow : SKColors.Red;
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                bitmap.SetPixel(x, y, leftColor);
                bitmap.SetPixel(bitmap.Width - 1 - x, y, SKColors.Green);
            }
        }

        return Encode(bitmap);
    }

    private static byte[] Encode(SKBitmap bitmap)
    {
        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData encodedImage = image.Encode(SKEncodedImageFormat.Png, 100);
        return encodedImage.ToArray();
    }
}
