using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class DepartureTimeSortRefinementTests : ResultsRefinementTestBase
{
    public DepartureTimeSortRefinementTests(ITestOutputHelper output)
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
    public void SearchBook_TC08_DepartureTimeSort_OrdersResultsAndRecommendedRestoresBaseline()
        => RunRefinementTest(
            page => page.AssertDepartureTimeSortAndRecommendedReset(),
            "Departure-time sort ordered results and Recommended restored the baseline.");
}
