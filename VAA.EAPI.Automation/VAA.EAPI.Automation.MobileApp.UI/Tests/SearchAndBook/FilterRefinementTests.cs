using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class ConnectionsFilterRefinementTests : ResultsRefinementTestBase
{
    public ConnectionsFilterRefinementTests(ITestOutputHelper output)
        : base(output)
    {
    }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Feature", "Search-ConnectionsFilters")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC12_ConnectionsFilters_EveryResultMatchesSelectedCount()
        => RunRefinementTest(
            page => page.AssertEnabledConnectionFiltersAffectResults(),
            "Every enabled connections filter returned matching results.");
}

public sealed class DepartureTimeFilterRefinementTests : ResultsRefinementTestBase
{
    public DepartureTimeFilterRefinementTests(ITestOutputHelper output)
        : base(output)
    {
    }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Feature", "Search-DepartureTimeFilters")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC13_DepartureTimeFilters_EveryResultFallsInsideSelectedWindow()
        => RunRefinementTest(
            page => page.AssertEnabledDepartureTimeFiltersAffectResults(),
            "Every enabled departure-time filter returned results within its selected window.");
}
