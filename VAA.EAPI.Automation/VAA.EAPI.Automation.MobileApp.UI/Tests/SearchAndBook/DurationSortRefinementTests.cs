using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class DurationSortRefinementTests : ResultsRefinementTestBase
{
    public DurationSortRefinementTests(ITestOutputHelper output)
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
    public void SearchBook_TC09_DurationSort_OrdersResultsAndRecommendedRestoresBaseline()
        => RunRefinementTest(
            page => page.AssertDurationSortAndRecommendedReset(),
            "Duration sort ordered results and Recommended restored the baseline.");
}
