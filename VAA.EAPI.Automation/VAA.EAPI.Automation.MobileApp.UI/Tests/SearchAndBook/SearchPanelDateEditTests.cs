using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class SearchPanelDateEditTests : SearchPanelTestBase
{
    public SearchPanelDateEditTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-SearchPanel-Readiness")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Feature", "SearchPanel")]
    [Trait("Platform", "iOS")]
    [Trait("Platform", "Android")]
    [Trait("Readiness", "AppContractRequired")]
    [Trait("Suite", "Readiness")]
    [Trait("Area", "SearchAndBook")]
    public void SearchBook_TC36_TravelDates_EditUpdatesSummaryAndCancelPreservesCommittedDates()
        => RunPanelScenario(page =>
    {
        page.SetRoundTrip(true);
        // Exercise virtualised months in both directions, not just dates already on screen.
        DateOnly first = DateOnly.FromDateTime(DateTime.Today).AddMonths(3);
        page.SelectDates(first, first.AddDays(3));
        Assert.True(page.HasDates(first, first.AddDays(3)));

        DateOnly edited = first.AddMonths(-1);
        page.SelectDates(edited, edited.AddDays(5));
        Assert.True(page.HasDates(edited, edited.AddDays(5)));

        page.OpenDates();
        page.ChooseDate(edited.AddDays(2));
        page.CloseDates();
        Assert.True(page.HasDates(edited, edited.AddDays(5)));
    });
}
