using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class AirlineFilterRefinementTests : ResultsRefinementTestBase
{
    public AirlineFilterRefinementTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Feature", "Search-AirlineFilters")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC11_AvailableAirlineFilters_EveryResultContainsSelectedOperatedLeg()
        => RunRefinementTest(
            page => page.AssertEveryAvailableAirlineFilterMatchesResults(),
            "Every airline filter offered for the current search matched every returned itinerary.");
}
