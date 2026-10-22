using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class AndroidSearchDismissalTests : AndroidSearchPanelTestBase
{
    public AndroidSearchDismissalTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-AndroidReadiness")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Feature", "SearchPanel")]
    [Trait("Platform", "Android")]
    [Trait("Readiness", "AppContractRequired")]
    public void AndroidSearchBook_TC06_SelectionSheets_DismissalPreservesRoute()
        => RunPanelScenario(page =>
    {
        page.SelectAirport(true, "LHR");
        page.SelectAirport(false, "JFK");
        page.OpenAirports(true);
        page.CloseAirports(back: true);
        page.OpenAirports(false);
        page.CloseAirports();
        page.OpenDates();
        page.CloseDates();
        page.OpenPassengers();
        page.ClosePassengers();
        page.OpenPromo();
        page.ClosePromo();
        Assert.True(page.HasAirport(true, "LHR"));
        Assert.True(page.HasAirport(false, "JFK"));
    });
}
