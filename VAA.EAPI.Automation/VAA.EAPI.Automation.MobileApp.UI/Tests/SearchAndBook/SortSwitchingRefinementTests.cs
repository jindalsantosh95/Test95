using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class SortSwitchingRefinementTests : ResultsRefinementTestBase
{
    public SortSwitchingRefinementTests(ITestOutputHelper output)
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
    public void SearchBook_TC10_SortSwitching_PreservesSelectionAndRecommendedRestoresBaseline()
        => RunRefinementTest(
            page => page.AssertSortSwitchingPreservesSelectionAndRecommendedReset(),
            "Switching sort options preserved each selection and Recommended restored the baseline.");
}
