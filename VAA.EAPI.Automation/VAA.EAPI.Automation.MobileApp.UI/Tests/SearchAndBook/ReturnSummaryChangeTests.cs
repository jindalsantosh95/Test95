using VAA.EAPI.Automation.MobileApp.UI.Assertions;
using VAA.EAPI.Automation.MobileApp.UI.Handlers;
using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using VAA.EAPI.Automation.MobileApp.UI.Models;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class ReturnSummaryChangeTests(ITestOutputHelper output) : SearchBookTestBase(output)
{
    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Feature", "Flight-Summary")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC40_ReturnSummaryChange_RequiresInboundReselectionAndUpdatesItinerariesAndTotal()
    {
        const string testName = nameof(SearchBook_TC40_ReturnSummaryChange_RequiresInboundReselectionAndUpdatesItinerariesAndTotal);
        TrySetBrowserStackSessionName(testName);
        try
        {
            FlightSearch search = FlightSearch.Return("LHR", "JFK", 3);
            NativeFlightSearchPage page = new(Fixture.Driver);
            new AppEntryHandler(Fixture.Driver).EnterAsGuest();
            page.Open();
            page.SearchReturn(search);
            page.WaitForResults();
            SelectedFlight originalOutbound = page.SelectFirstFlightByCarrier(true, false, captureFareTitle: true);
            SelectedFlight originalInbound = page.SelectFirstFlightByCarrier(true, true, captureFareTitle: true);
            FlightSearch inboundSearch = new(search.Destination, search.Origin,
                search.ReturnDate ?? throw new InvalidOperationException("Return date required."), search.Passengers);
            page.AssertFlightSummaryMatchesSelection(search, originalOutbound, true);
            page.AssertFlightSummaryMatchesSelection(inboundSearch, originalInbound, true, itineraryIndex: 1);

            page.ChangeSummaryFlight(0);
            page.AssertResultsRouteContains(search.Origin, search.Destination);
            page.AssertResultsSelectedDate(search.DepartureDate);
            SelectedFlight outbound = page.SelectFirstFlightByCarrier(true, false,
                excludedFlight: originalOutbound, captureFareTitle: true);
            page.AssertInboundReselectionRequired(search);
            SelectedFlight inbound = page.SelectFirstFlightByCarrier(true, true,
                excludedFlight: originalInbound, captureFareTitle: true);

            Assert.NotEqual(SummaryChangeAssertions.Schedule(originalOutbound.ResultText), SummaryChangeAssertions.Schedule(outbound.ResultText));
            Assert.NotEqual(SummaryChangeAssertions.Schedule(originalInbound.ResultText), SummaryChangeAssertions.Schedule(inbound.ResultText));
            page.AssertFlightSummaryMatchesSelection(search, outbound, true);
            Assert.Equal(outbound.FareTitle.Trim(), page.ReadSummaryFareTitle().Trim(), ignoreCase: true);
            page.AssertFlightSummaryMatchesSelection(inboundSearch, inbound, true, itineraryIndex: 1);
            Assert.Equal(inbound.FareTitle.Trim(), page.ReadSummaryFareTitle(1).Trim(), ignoreCase: true);
            Assert.Equal(SummaryChangeAssertions.ReturnTotal(outbound.FareLabel, inbound.FareLabel),
                SummaryChangeAssertions.Money(page.ReadSummary().PriceText));
            TrySetBrowserStackStatus("passed", "Outbound change required inbound reselection; both replacement itineraries, fares and total matched. No checkout attempted.", testName);
        }
        catch (Exception exception)
        {
            TrySetBrowserStackStatus("failed", DescribeBrowserStackFailure(exception), testName);
            throw;
        }
    }
}
