using SkiaSharp;
using VAA.EAPI.Automation.MobileApp.UI.Handlers;
using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using VAA.EAPI.Automation.MobileApp.UI.VisualValidation;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public abstract class SearchPanelVisualValidationTestBase : SearchBookTestBase
{
    protected SearchPanelVisualValidationTestBase(ITestOutputHelper output)
        : base(output, SearchPanelVisualRuntimeContract.ConfigurationOverrides) { }

    protected void AssertSearchPanelComponent(
        Func<NativeFlightSearchPage, string, byte[]> capture,
        string contractId,
        string artifactName,
        Action<NativeFlightSearchPage>? prepare = null,
        Action? prepareAuthenticatedState = null)
    {
        TrySetBrowserStackSessionName();
        try
        {
            VisualBaselineContract contract = VisualBaselineManifest.Load(AppContext.BaseDirectory).Get(contractId);
            SearchPanelVisualRuntimeContract.Validate(Fixture.Config, Fixture.Driver, contract.Runtime);
            AppEntryHandler entryHandler = new(Fixture.Driver);
            NativeFlightSearchPage searchPage = new(Fixture.Driver);

            entryHandler.EnterAsGuest();
            prepareAuthenticatedState?.Invoke();
            searchPage.Open();
            prepare?.Invoke(searchPage);
            byte[] actual = capture(searchPage, contract.AccessibilityId);
            (int actualWidth, int actualHeight) = ReadDimensions(actual);

            string baselineSourcePath = Path.Combine(AppContext.BaseDirectory, contract.ReferenceImage);
            byte[] baselineSource = File.ReadAllBytes(baselineSourcePath);
            (int baselineSourceWidth, int baselineSourceHeight) = ReadDimensions(baselineSource);
            VisualBaselineCrop crop = contract.Figma.TargetCropPixels;
            byte[] baseline = VisualImageNormalizer.CompositeTransparencyOnWhite(
                new AppiumComponentScreenshot().Crop(
                baselineSource,
                baselineSourceWidth,
                baselineSourceHeight,
                crop.X,
                crop.Y,
                crop.Width,
                crop.Height));
            string artifactDirectory = Path.Combine(
                Directory.GetCurrentDirectory(),
                "TestResults",
                "mobile-visual-validation",
                Guid.NewGuid().ToString("N"));
            AnchoredComponentComparer comparer = new();
            VisualBaselineComparison comparison = contract.Comparison;
            VisualComparisonOptions options = new(
                ChannelTolerance: comparison.ChannelTolerance,
                MaximumMismatchRatio: comparison.MaximumMismatchRatio);
            string baselineDescription =
                $"{baselineSourcePath} crop ({crop.X},{crop.Y},{crop.Width},{crop.Height})";
            IReadOnlyList<VisualComparisonResult> results = comparison.Strategy == "horizontal-anchors-and-centre"
                ? comparer.CompareHorizontalAnchorsAndCenter(
                    baseline,
                    crop.Width,
                    crop.Height,
                    actual,
                    actualWidth,
                    actualHeight,
                    comparison.LeftAnchorWidthPixels,
                    comparison.CenterAnchorWidthPixels!.Value,
                    comparison.RightAnchorWidthPixels,
                    comparisonHeight: Math.Min(comparison.ComparisonHeightPixels, Math.Min(crop.Height, actualHeight)),
                    baselineDescription,
                    artifactDirectory,
                    artifactName,
                    options)
                : comparer.CompareHorizontalAnchors(
                    baseline,
                    crop.Width,
                    crop.Height,
                    actual,
                    actualWidth,
                    actualHeight,
                    comparison.LeftAnchorWidthPixels,
                    comparison.RightAnchorWidthPixels,
                    comparisonHeight: Math.Min(comparison.ComparisonHeightPixels, Math.Min(crop.Height, actualHeight)),
                    baselineDescription,
                    artifactDirectory,
                    artifactName,
                    options);

            foreach (VisualComparisonResult result in results)
            {
                Output.WriteLine(result.Describe(baselineSourcePath));
            }

            Assert.True(
                results.All(result => result.IsMatch),
                string.Join(Environment.NewLine, results.Select(result => result.Describe(baselineSourcePath))));
            TrySetBrowserStackStatus("passed", $"The native component matched Figma node {contract.Figma.NodeId}.");
        }
        catch (Exception exception)
        {
            TrySetBrowserStackStatus("failed", DescribeBrowserStackFailure(exception));
            throw;
        }
    }

    private static (int Width, int Height) ReadDimensions(byte[] png)
    {
        using SKBitmap image = SKBitmap.Decode(png)
                               ?? throw new InvalidDataException("Could not decode the component screenshot as PNG.");
        return (image.Width, image.Height);
    }
}

public sealed class SearchPanelFlyingFromVisualValidationTests : SearchPanelVisualValidationTestBase
{
    public SearchPanelFlyingFromVisualValidationTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-VisualRegression")]
    [Trait("TestType", "VisualValidation")]
    [Trait("Feature", "Search-Panel")]
    [Trait("Platform", "iOS")]
    [Trait("Readiness", "Regression")]
    public void SearchBookVisual_TC01_FlyingFromRow_MatchesFigmaComponent()
        => AssertSearchPanelComponent(
            (page, accessibilityId) => page.CaptureFlyingFromSelectionRow(accessibilityId),
            "search-book-flying-from-selection-row-ios",
            nameof(SearchBookVisual_TC01_FlyingFromRow_MatchesFigmaComponent));
}

public sealed class SearchPanelFlyingToVisualValidationTests : SearchPanelVisualValidationTestBase
{
    public SearchPanelFlyingToVisualValidationTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-VisualRegression")]
    [Trait("TestType", "VisualValidation")]
    [Trait("Feature", "Search-Panel")]
    [Trait("Platform", "iOS")]
    [Trait("Readiness", "Regression")]
    public void SearchBookVisual_TC02_FlyingToRow_MatchesFigmaComponent()
        => AssertSearchPanelComponent(
            (page, accessibilityId) => page.CaptureFlyingToSelectionRow(accessibilityId),
            "search-book-flying-to-selection-row-ios",
            nameof(SearchBookVisual_TC02_FlyingToRow_MatchesFigmaComponent));
}

public sealed class SearchPanelJourneyTypeVisualValidationTests : SearchPanelVisualValidationTestBase
{
    public SearchPanelJourneyTypeVisualValidationTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-VisualRegression")]
    [Trait("TestType", "VisualValidation")]
    [Trait("Feature", "Search-Panel")]
    [Trait("Platform", "iOS")]
    [Trait("Readiness", "Ready")]
    public void SearchBookVisual_TC03_JourneyTypePicker_MatchesFigmaComponent()
        => AssertSearchPanelComponent(
            (page, accessibilityId) => page.CaptureJourneyTypePicker(accessibilityId),
            "search-book-journey-type-picker-ios",
            nameof(SearchBookVisual_TC03_JourneyTypePicker_MatchesFigmaComponent));
}

public sealed class SearchPanelWhenVisualValidationTests : SearchPanelVisualValidationTestBase
{
    public SearchPanelWhenVisualValidationTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-VisualRegression")]
    [Trait("TestType", "VisualValidation")]
    [Trait("Feature", "Search-Panel")]
    [Trait("Platform", "iOS")]
    [Trait("Readiness", "Regression")]
    public void SearchBookVisual_TC04_WhenRow_MatchesFigmaComponent()
        => AssertSearchPanelComponent(
            (page, accessibilityId) => page.CaptureWhenSelectionRow(accessibilityId),
            "search-book-when-selection-row-ios",
            nameof(SearchBookVisual_TC04_WhenRow_MatchesFigmaComponent));
}

public sealed class SearchPanelWhoVisualValidationTests : SearchPanelVisualValidationTestBase
{
    public SearchPanelWhoVisualValidationTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-VisualRegression")]
    [Trait("TestType", "VisualValidation")]
    [Trait("Feature", "Search-Panel")]
    [Trait("Platform", "iOS")]
    [Trait("Readiness", "Regression")]
    public void SearchBookVisual_TC05_WhoRow_MatchesFigmaComponent()
        => AssertSearchPanelComponent(
            (page, accessibilityId) => page.CaptureWhoSelectionRow(accessibilityId),
            "search-book-who-selection-row-ios",
            nameof(SearchBookVisual_TC05_WhoRow_MatchesFigmaComponent));
}
