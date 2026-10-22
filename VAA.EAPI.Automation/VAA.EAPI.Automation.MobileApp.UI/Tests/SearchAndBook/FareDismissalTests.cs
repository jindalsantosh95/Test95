using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class FareDismissalTests : ResultsRefinementTestBase
{
    public FareDismissalTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Feature", "Search-FareDismissal")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC33_FareSheetCancellation_PreservesResultAndOriginalCabinOnReopen()
        => RunRefinementTest(page => page.AssertFareDismissalPreservesResults(),
            "Cancelling a different cabin retained the original result and reopening restored its cabin.");
}
