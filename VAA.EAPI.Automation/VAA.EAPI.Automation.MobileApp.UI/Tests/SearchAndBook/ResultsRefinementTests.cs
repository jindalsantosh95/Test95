using VAA.EAPI.Automation.MobileApp.UI.Handlers;
using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using VAA.EAPI.Automation.MobileApp.UI.Models;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class ResultsRefinementTests : SearchBookTestBase
{
    public ResultsRefinementTests(ITestOutputHelper output)
        : base(output)
    {
    }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Smoke")]
    [Trait("Area", "SearchAndBook")]
    [Trait("Feature", "Search-ResultsRefinement")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC06_ResultsSortAndFilter_PersistUntilFiltersAreCleared()
    {
        TrySetBrowserStackSessionName();
        try
        {
            NativeFlightSearchPage page = new(Fixture.Driver);
            new AppEntryHandler(Fixture.Driver).EnterAsGuest();
            page.Open();
            page.SearchOneWay(FlightSearch.OneWay(
                "LHR", "JFK", monthsFromToday: 3, passengers: new PassengerCounts(1, 0, 0, 0)));
            page.WaitForResults();

            ResultsRefinement refinement = page.ApplyDiscoveredPriceSortAndFilter();
            page.AssertRefinementPersistsThenClear(refinement);

            TrySetBrowserStackStatus(
                "passed",
                $"Results applied '{refinement.SortLabel}' and '{refinement.FilterLabel}', persisted both, then cleared the filter.");
        }
        catch (Exception exception)
        {
            TrySetBrowserStackStatus("failed", DescribeBrowserStackFailure(exception));
            throw;
        }
    }

}
