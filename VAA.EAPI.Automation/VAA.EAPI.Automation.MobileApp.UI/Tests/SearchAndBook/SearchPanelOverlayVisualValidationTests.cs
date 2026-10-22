using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class AirportSelectorVisualValidationTests : SearchPanelVisualValidationTestBase
{
    public AirportSelectorVisualValidationTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-VisualRegression")]
    [Trait("TestType", "VisualValidation")]
    [Trait("Feature", "Search-Panel-Airport-Selector")]
    [Trait("Platform", "iOS")]
    [Trait("Readiness", "Regression")]
    public void SearchBookVisual_TC06_DestinationAirportSearchField_MatchesFigmaComponent()
        => AssertSearchPanelComponent(
            (page, accessibilityId) => page.CaptureDestinationAirportSearchField(accessibilityId),
            "search-book-destination-airport-search-field-ios",
            nameof(SearchBookVisual_TC06_DestinationAirportSearchField_MatchesFigmaComponent));
}

public sealed class CalendarVisualValidationTests : SearchPanelVisualValidationTestBase
{
    public CalendarVisualValidationTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-VisualRegression")]
    [Trait("TestType", "VisualValidation")]
    [Trait("Feature", "Search-Panel-Calendar")]
    [Trait("Platform", "iOS")]
    [Trait("Readiness", "Regression")]
    public void SearchBookVisual_TC07_InitialCalendarHeader_MatchesFigmaComponent()
        => AssertSearchPanelComponent(
            (page, accessibilityId) => page.CaptureInitialCalendarHeader(accessibilityId),
            "search-book-initial-calendar-header-ios",
            nameof(SearchBookVisual_TC07_InitialCalendarHeader_MatchesFigmaComponent));
}

public sealed class DiscountCodeFieldVisualValidationTests : SearchPanelVisualValidationTestBase
{
    public DiscountCodeFieldVisualValidationTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-VisualRegression")]
    [Trait("TestType", "VisualValidation")]
    [Trait("Feature", "Search-Panel-Discount-Code")]
    [Trait("Platform", "iOS")]
    [Trait("Readiness", "Ready")]
    public void SearchBookVisual_TC08_EmptyDiscountCodeField_MatchesFigmaComponent()
        => AssertSearchPanelComponent(
            (page, accessibilityId) => page.CaptureEmptyDiscountCodeField(accessibilityId),
            "search-book-empty-discount-code-field-ios",
            nameof(SearchBookVisual_TC08_EmptyDiscountCodeField_MatchesFigmaComponent));
}

public sealed class DiscountCodeApplyVisualValidationTests : SearchPanelVisualValidationTestBase
{
    public DiscountCodeApplyVisualValidationTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-VisualRegression")]
    [Trait("TestType", "VisualValidation")]
    [Trait("Feature", "Search-Panel-Discount-Code")]
    [Trait("Platform", "iOS")]
    [Trait("Readiness", "Regression")]
    public void SearchBookVisual_TC09_EmptyDiscountCodeApplyButton_MatchesDisabledFigmaState()
        => AssertSearchPanelComponent(
            (page, accessibilityId) => page.CaptureDisabledDiscountCodeApplyButton(accessibilityId),
            "search-book-empty-discount-code-apply-ios",
            nameof(SearchBookVisual_TC09_EmptyDiscountCodeApplyButton_MatchesDisabledFigmaState));
}

public sealed class PassengerSelectorVisualValidationTests : SearchPanelVisualValidationTestBase
{
    public PassengerSelectorVisualValidationTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-VisualRegression")]
    [Trait("TestType", "VisualValidation")]
    [Trait("Feature", "Search-Panel-Passenger-Selector")]
    [Trait("Platform", "iOS")]
    [Trait("Readiness", "Ready")]
    public void SearchBookVisual_TC10_DefaultPassengerSelector_MatchesFigmaComponent()
        => AssertSearchPanelComponent(
            (page, accessibilityId) => page.CaptureDefaultPassengerSelector(accessibilityId),
            "search-book-default-passenger-selector-ios",
            nameof(SearchBookVisual_TC10_DefaultPassengerSelector_MatchesFigmaComponent));
}
