using VAA.EAPI.Automation.MobileApp.UI.Assertions;
using VAA.EAPI.Automation.MobileApp.UI.Flows;
using VAA.EAPI.Automation.MobileApp.UI.Handlers;
using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using VAA.EAPI.Automation.MobileApp.UI.Models;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class UnsupportedCarrierSeatBypassTests(ITestOutputHelper output) : SearchBookTestBase(output,
    new Dictionary<string, string?> { ["IOSLaunch:AutomationFeatureFlags"] = "850100-search-native-entry" })
{
    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Journey", "Seats")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC50_UnsupportedOperatingCarriers_BypassSeatsWhileVirginRemainsEligible()
    {
        const string testName = nameof(SearchBook_TC50_UnsupportedOperatingCarriers_BypassSeatsWhileVirginRemainsEligible);
        TrySetBrowserStackSessionName(testName);
        try
        {
            new AppEntryHandler(Fixture.Driver).EnterAsGuest();
            NativeFlightSearchPage search = new(Fixture.Driver);
            NativeBookingCheckoutFlow checkout = new(Fixture.Driver);
            FlightSearch criteria = FlightSearch.OneWay("LHR", "JFK", 3, PassengerCounts.OneAdult);
            search.Open();
            search.SearchOneWay(criteria);
            search.WaitForResults();
            SelectedFlight unsupported = search.SelectFirstFlightByCarrier(false, true,
                requiredFareTitle: "Economy Standard", requireUnsupportedSeatCarriers: true,
                requiredCabinTitle: "Economy Standard");
            Assert.NotNull(unsupported.Details);
            Assert.True(SeatCarrierEligibilityAssertions.AllLegsAreKnownUnsupported(unsupported.Details));
            Assert.Equal("Economy Standard", unsupported.FareTitle, ignoreCase: true);
            Assert.Equal(unsupported.FareTitle, search.ReadSummaryFareTitle(), ignoreCase: true);
            Output.WriteLine("Unsupported operators: " + string.Join(", ",
                unsupported.Details.Legs.Select(leg => leg.OperatingCarrier)));
            checkout.VerifyGuestSeatBypassToPayment(PassengerCounts.OneAdult);
            Output.WriteLine("Unsupported itinerary reached payment directly; no seat Skip/Continue or payment action.");

            // Same-session positive control prevents a persisted global skip flag giving a false pass.
            search.ReturnToSearchPanelWithoutPayment();
            search.SearchOneWay(criteria);
            search.WaitForResults();
            search.SelectFirstFlightByCarrier(true, true, requiredCabin: "Economy",
                requiredFareTitle: "Economy Classic");
            checkout.OpenSeatSelectionForOneGuestAdult();
            NativeSeatSelectionPage seats = new(Fixture.Driver);
            seats.WaitForActivePassenger(0);
            Assert.Equal(1, seats.ReadAssignablePassengerCount());
            TrySetBrowserStackStatus("passed", "Unsupported carriers bypassed seats; same-session Virgin positive control opened seats. No payment.", testName);
        }
        catch (Exception exception)
        {
            TrySetBrowserStackStatus("failed", DescribeBrowserStackFailure(exception), testName);
            throw;
        }
    }
}
