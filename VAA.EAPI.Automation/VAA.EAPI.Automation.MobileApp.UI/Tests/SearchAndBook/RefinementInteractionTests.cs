using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class FilterSwitchingRefinementTests : ResultsRefinementTestBase
{
    public FilterSwitchingRefinementTests(ITestOutputHelper output)
        : base(output)
    {
    }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Feature", "Search-ResultsRefinement-Interaction")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC14_FilterSwitching_ReplacesDiscoveredAirlineAndValidatesResults()
        => RunRefinementTest(
            page => page.AssertAvailableAirlineFilterSwitching(),
            "Switching between two available airline filters replaced the selection and validated results.");
}

public sealed class SortAndFilterPersistenceRefinementTests : ResultsRefinementTestBase
{
    public SortAndFilterPersistenceRefinementTests(ITestOutputHelper output)
        : base(output)
    {
    }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("Suite", "Sanity")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Feature", "Search-ResultsRefinement-Interaction")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC15_PriceSortWithDiscoveredAirlineFilter_PreservesOrderingAndMatchesCarrier()
        => RunRefinementTest(
            page => page.AssertPriceSortPersistsWithAvailableAirlineFilter(),
            "Price ordering persisted while every result matched the available airline filter.");
}

public sealed class ClearFiltersRefinementTests : ResultsRefinementTestBase
{
    public ClearFiltersRefinementTests(ITestOutputHelper output)
        : base(output)
    {
    }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Feature", "Search-ResultsRefinement-Interaction")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC16_ClearFilters_RemovesDiscoveredAirlineAndPreservesPriceSort()
        => RunRefinementTest(
            page => page.AssertClearRemovesAvailableAirlineFilterAndPreservesPriceSort(),
            "Clear removed the available airline filter while preserving the selected price sort.");
}
