using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.MobileApp.UI.Flows;
using VAA.EAPI.Automation.MobileApp.UI.Models;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class GuestSeatBookingTests(ITestOutputHelper output) : SeatScenarioTestBase(output)
{
    [Fact]
    [Trait("Category", "MobileApp-SearchBook-SmokeE2E")]
    [Trait("TestType", "SmokeE2E")]
    [Trait("Suite", "Regression")]
    [Trait("Suite", "Sanity")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Journey", "Seats")]
    [Trait("Passengers", "OneAdult")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC43_OneAdultGuestSeatSelection_CompletesBookingAndAppearsInTrips()
    {
        const string testName = nameof(SearchBook_TC43_OneAdultGuestSeatSelection_CompletesBookingAndAppearsInTrips);
        TrySetBrowserStackSessionName(testName);
        try
        {
            FlightSearch search = FlightSearch.OneWay("LHR", "JFK", monthsFromToday: 3,
                passengers: PassengerCounts.OneAdult);
            new Handlers.AppEntryHandler(Fixture.Driver).EnterAsGuest();
            NativeFlightSearchPage searchPage = new(Fixture.Driver);
            searchPage.Open();
            searchPage.SearchOneWay(search);
            searchPage.WaitForResults();
            SelectedFlight selected = searchPage.SelectFirstFlightByCarrier(
                virginFlight: true, expectFlightSummary: true, requiredCabin: "Economy",
                requiredFareTitle: "Economy Classic");
            FlightSummary summary = searchPage.ReadSummary();
            Assert.Contains(search.Origin, summary.OriginText, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(search.Destination, summary.DestinationText, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(selected.CabinName, summary.FareText, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Economy Classic", summary.FareText, StringComparison.OrdinalIgnoreCase);

            CompletedNativeBooking booking = new NativeBookingCheckoutFlow(Fixture.Driver)
                .CompleteWithSeatForOneAdult();
            Assert.False(booking.Confirmation.TotalPriceText.IsNullOrWhiteSpace());
            ReportSuccessfulBooking(booking,
                "One adult selected an available seat, paid, confirmed and found the booking in Trips.", testName);
        }
        catch (Exception exception)
        {
            TrySetBrowserStackStatus("failed", DescribeBrowserStackFailure(exception), testName);
            throw;
        }
    }
}
