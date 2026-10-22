using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class SearchPanelCriteriaTests : SearchPanelTestBase
{
    public SearchPanelCriteriaTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-SearchPanel-Readiness")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Feature", "SearchPanel")]
    [Trait("Platform", "iOS")]
    [Trait("Platform", "Android")]
    [Trait("Readiness", "AppContractRequired")]
    public void SearchPanel_TC01_SearchPanel_ValidCriteriaEnableSearch()
        => RunPanelScenario(page =>
    {
        page.SetRoundTrip(true);
        page.SetOneAdult();
        page.SelectAirport(true, "LHR");
        page.SelectAirport(false, "JFK");
        DateOnly departure = DateOnly.FromDateTime(DateTime.Today).AddDays(2);
        DateOnly returning = departure.AddDays(5);
        page.SelectDates(departure, returning);
        Assert.True(page.HasAirport(true, "LHR"));
        Assert.True(page.HasAirport(false, "JFK"));
        Assert.True(page.HasDates(departure, returning));
        Assert.True(page.HasPassengerTotal(1));
        Assert.True(page.SearchEnabled);
    });
}
