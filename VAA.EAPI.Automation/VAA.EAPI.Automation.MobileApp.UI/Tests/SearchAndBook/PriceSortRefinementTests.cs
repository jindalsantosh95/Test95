using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class PriceSortRefinementTests : ResultsRefinementTestBase
{
    public PriceSortRefinementTests(ITestOutputHelper output)
        : base(output)
    {
    }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Feature", "Search-ResultsRefinement")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC07_PriceSort_OrdersDisplayedResultsAscending()
        => RunRefinementTest(
            page => page.AssertPriceSortOrdersResults(),
            "Price sort ordered every displayed result in ascending order.");
}
