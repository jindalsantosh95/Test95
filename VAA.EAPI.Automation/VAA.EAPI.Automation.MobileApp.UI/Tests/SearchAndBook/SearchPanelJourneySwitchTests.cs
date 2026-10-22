using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class SearchPanelJourneySwitchTests : SearchPanelTestBase
{
    public SearchPanelJourneySwitchTests(ITestOutputHelper output) : base(output) { }

    [Fact(Skip = "PANEL-TC02 pending: date confirmation fix requires both-adapter requalification before restoring readiness.")]
    [Trait("Category", "MobileApp-SearchPanel-Pending")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Feature", "SearchPanel")]
    [Trait("Platform", "iOS")]
    [Trait("Platform", "Android")]
    [Trait("Readiness", "Blocked")]
    public void SearchPanel_TC02_JourneyType_SwitchingToReturnRequiresReturnDate()
        => RunPanelScenario(page =>
    {
        page.SetRoundTrip(false);
        page.SetOneAdult();
        page.SelectAirport(true, "LHR");
        page.SelectAirport(false, "JFK");
        DateOnly departure = DateOnly.FromDateTime(DateTime.Today).AddDays(2);
        page.SelectDates(departure);
        Assert.False(page.IsRoundTrip);
        Assert.True(page.HasDates(departure));
        Assert.True(page.SearchEnabled);

        page.SetRoundTrip(true);
        Assert.True(page.IsRoundTrip);
        Assert.False(page.SearchEnabled);
        page.SelectDates(departure, departure.AddDays(5));
        Assert.True(page.HasDates(departure, departure.AddDays(5)));
        Assert.True(page.SearchEnabled);

        page.SetRoundTrip(false);
        Assert.True(page.HasDates(departure));
        Assert.True(page.SearchEnabled);
    });
}
