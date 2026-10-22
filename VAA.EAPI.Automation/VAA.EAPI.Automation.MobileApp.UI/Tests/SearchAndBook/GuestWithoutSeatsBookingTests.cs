using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.MobileApp.UI.Flows;
using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using VAA.EAPI.Automation.MobileApp.UI.Models;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class GuestWithoutSeatsBookingTests(ITestOutputHelper output) : SearchBookTestBase(output,
    new Dictionary<string, string?>
    {
        ["IOSLaunch:AutomationFeatureFlags"] = "850100-search-native-entry,850101-search-native-skip-seat-selection",
    })
{
    [Fact]
    [Trait("Category", "MobileApp-SearchBook-SmokeE2E")]
    [Trait("TestType", "SmokeE2E")]
    [Trait("Suite", "Regression")]
    [Trait("Suite", "Sanity")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Journey", "WithoutSeats")]
    [Trait("Passengers", "OneAdult")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC48_OneAdultGuestWithoutSeats_CompletesBookingAndAppearsInTrips()
    {
        const string testName = nameof(SearchBook_TC48_OneAdultGuestWithoutSeats_CompletesBookingAndAppearsInTrips);
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
                .Complete(PassengerCounts.OneAdult);
            Assert.False(booking.Confirmation.TotalPriceText.IsNullOrWhiteSpace());
            ReportSuccessfulBooking(booking,
                "One guest adult completed payment, confirmation and Trips without selecting a seat.", testName);
        }
        catch (Exception exception)
        {
            TrySetBrowserStackStatus("failed", DescribeBrowserStackFailure(exception), testName);
            throw;
        }
    }
}
