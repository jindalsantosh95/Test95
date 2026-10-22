using VAA.EAPI.Automation.MobileApp.UI.Flows;
using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using VAA.EAPI.Automation.MobileApp.UI.Models;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class CardPresentationBookingTests(ITestOutputHelper output) : SearchBookTestBase(output,
    new Dictionary<string, string?>
    {
        ["IOSLaunch:AutomationFeatureFlags"] = "850100-search-native-entry,850101-search-native-skip-seat-selection",
    })
{
    [Fact]
    [Trait("Category", "MobileApp-SearchBook-SmokeE2E")]
    [Trait("TestType", "SmokeE2E")]
    [Trait("Suite", "Regression")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Journey", "CardPresentation")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC55_ShortNoticeLosCardPresentation_AgreementCompletesBookingAndTrips()
    {
        const string testName = nameof(SearchBook_TC55_ShortNoticeLosCardPresentation_AgreementCompletesBookingAndTrips);
        TrySetBrowserStackSessionName(testName);
        try
        {
            FlightSearch search = new("LHR", "LOS", DateOnly.FromDateTime(DateTime.Today).AddDays(3), PassengerCounts.OneAdult);
            new Handlers.AppEntryHandler(Fixture.Driver).EnterAsGuest();
            NativeFlightSearchPage searchPage = new(Fixture.Driver);
            searchPage.Open();
            searchPage.SearchOneWay(search);
            searchPage.WaitForResults();
            searchPage.SelectFirstFlightByCarrier(virginFlight: true, expectFlightSummary: true,
                requiredCabin: "Economy", requiredFareTitle: "Economy Classic");
            FlightSummary summary = searchPage.ReadSummary();
            Assert.Contains(search.Origin, summary.OriginText, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(search.Destination, summary.DestinationText, StringComparison.OrdinalIgnoreCase);
            CompletedNativeBooking booking = new NativeBookingCheckoutFlow(Fixture.Driver)
                .CompleteWithCardPresentationForOneAdult();
            ReportSuccessfulBooking(booking,
                "Guest accepted airport card-presentation verification after HPP and completed confirmation and Trips.", testName);
        }
        catch (Exception exception)
        {
            TrySetBrowserStackStatus("failed", DescribeBrowserStackFailure(exception), testName);
            throw;
        }
    }
}
