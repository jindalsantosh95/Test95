using VAA.EAPI.Automation.MobileApp.UI.Flows;
using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class RecentAirportSearchTests : SearchPanelTestBase
{
    public RecentAirportSearchTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("Category", "MobileApp-SearchPanel-Readiness")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Feature", "Search-RecentAirports")]
    [Trait("Platform", "iOS")]
    [Trait("Platform", "Android")]
    public void SearchBook_TC17_RecentAirportSearches_RetainFiveMostRecentPerDirection()
        => RunPanelScenario(RecentAirportSearchFlow.VerifyLatestFivePerDirection);
}
