using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.MobileApp.UI.Assertions;
using VAA.EAPI.Automation.MobileApp.UI.Flows;
using VAA.EAPI.Automation.MobileApp.UI.Handlers;
using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using VAA.EAPI.Automation.MobileApp.UI.Models;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public abstract class ChildSeatGuidanceTestBase(ITestOutputHelper output) : SearchBookTestBase(output,
    new Dictionary<string, string?> { ["IOSLaunch:AutomationFeatureFlags"] = "850100-search-native-entry" })
{
    protected static readonly PassengerCounts AdultAndChild = new(Adults: 1, Children: 1);

    protected void EnterAsGuestAndOpenSearch()
    {
        new AppEntryHandler(Fixture.Driver).EnterAsGuest();
        new NativeFlightSearchPage(Fixture.Driver).Open();
    }

    protected void AssertGuidanceDisplayed(string testName)
    {
        new NativeBookingCheckoutFlow(Fixture.Driver).OpenSeatSelectionForGuests(AdultAndChild);
        new NativeSeatSelectionPage(Fixture.Driver).WaitForChildGuidanceDisplayed();
        TrySetBrowserStackStatus("passed",
            "Child seat proximity guidance displayed before the first eligible seat sector; no dismissal, seat submission or payment.",
            testName);
    }

    protected void Run(string testName, Action<NativeFlightSearchPage> selectFlights, bool expectGuidance)
    {
        TrySetBrowserStackSessionName(testName);
        try
        {
            EnterAsGuestAndOpenSearch();
            NativeFlightSearchPage search = new(Fixture.Driver);
            selectFlights(search);
            if (expectGuidance)
            {
                AssertGuidanceDisplayed(testName);
            }
            else
            {
                new NativeBookingCheckoutFlow(Fixture.Driver).VerifyGuestSeatBypassToPayment(AdultAndChild);
                TrySetBrowserStackStatus("passed",
                    "All unsupported sectors bypassed child guidance and seat selection; no payment action.", testName);
            }
        }
        catch (Exception exception)
        {
            TrySetBrowserStackStatus("failed", DescribeBrowserStackFailure(exception), testName);
            throw;
        }
    }
}

public sealed class VirginChildSeatGuidanceTests(ITestOutputHelper output) : ChildSeatGuidanceTestBase(output)
{
    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("Area", "SearchAndBook")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Journey", "Seats")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC51_AdultAndChildVirginOneWay_DisplaysSeatProximityGuidance()
        => Run(nameof(SearchBook_TC51_AdultAndChildVirginOneWay_DisplaysSeatProximityGuidance), search =>
        {
            search.SearchOneWay(FlightSearch.OneWay("LHR", "JFK", 3, AdultAndChild));
            search.WaitForResults();
            search.SelectFirstFlightByCarrier(true, true, requiredCabin: "Economy",
                requiredFareTitle: "Economy Classic");
        }, expectGuidance: true);
}

public sealed class InboundEligibleChildSeatGuidanceTests(ITestOutputHelper output) : ChildSeatGuidanceTestBase(output)
{
    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("Area", "SearchAndBook")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Journey", "Seats")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC52_UnsupportedOutboundEligibleInbound_DisplaysGuidanceForFirstEligibleSector()
        => Run(nameof(SearchBook_TC52_UnsupportedOutboundEligibleInbound_DisplaysGuidanceForFirstEligibleSector), search =>
        {
            search.SearchReturn(FlightSearch.Return("YYZ", "LHR", 3, passengers: AdultAndChild));
            search.WaitForResults();
            SelectedFlight outbound = search.SelectFirstFlightByCarrier(false, false,
                requireUnsupportedSeatCarriers: true, captureFareTitle: true);
            Output.WriteLine($"Outbound selected: carrier={outbound.OperatingCarrierCode}; cabin={outbound.CabinName}; fare={outbound.FareTitle}.");
            Assert.NotNull(outbound.Details);
            Assert.True(SeatCarrierEligibilityAssertions.AllLegsAreKnownUnsupported(outbound.Details));
            SelectedFlight inbound = search.SelectFirstFlightByCarrier(true, true,
                requiredCabin: "Economy", requiredFareTitle: "Economy Classic",
                requiredLegs: SeatCarrierEligibilityAssertions.HasSeatSupportedLeg);
            Output.WriteLine($"Inbound selected: carrier={inbound.OperatingCarrierCode}; cabin={inbound.CabinName}; fare={inbound.FareTitle}.");
            Assert.True(inbound.FareTitle.Is("Economy Classic"));
            Assert.NotNull(inbound.Details);
            Assert.True(SeatCarrierEligibilityAssertions.HasSeatSupportedLeg(inbound.Details));
        }, expectGuidance: true);
}

public sealed class UnsupportedChildSeatGuidanceBypassTests(ITestOutputHelper output) : ChildSeatGuidanceTestBase(output)
{
    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("Area", "SearchAndBook")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Journey", "Seats")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC53_AdultAndChildUnsupportedOneWay_BypassesGuidanceAndSeats()
        => Run(nameof(SearchBook_TC53_AdultAndChildUnsupportedOneWay_BypassesGuidanceAndSeats), search =>
        {
            search.SearchOneWay(FlightSearch.OneWay("MAN", "MIA", 3, AdultAndChild));
            search.WaitForResults();
            SelectedFlight selected = search.SelectFirstFlightByCarrier(false, true,
                requiredFareTitle: "Economy Standard", requireUnsupportedSeatCarriers: true,
                requiredCabinTitle: "Economy Standard", requiredOperatingCarrierCode: "AF");
            Output.WriteLine($"Selected: carrier={selected.OperatingCarrierCode}; cabin={selected.CabinName}; fare={selected.FareTitle}.");
            Assert.True(selected.FareTitle.Is("Economy Standard"));
            Assert.NotNull(selected.Details);
            Assert.True(SeatCarrierEligibilityAssertions.AllLegsAreKnownUnsupported(selected.Details));
        }, expectGuidance: false);
}

public sealed class DeltaChildSeatGuidanceTests(ITestOutputHelper output) : ChildSeatGuidanceTestBase(output)
{
    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("Area", "SearchAndBook")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Journey", "Seats")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC54_AdultAndChildDirectDeltaOneWay_DisplaysSeatProximityGuidance()
        => Run(nameof(SearchBook_TC54_AdultAndChildDirectDeltaOneWay_DisplaysSeatProximityGuidance), search =>
        {
            search.SearchOneWay(FlightSearch.OneWay("LHR", "JFK", 3, AdultAndChild));
            search.WaitForResults();
            SelectedFlight selected = search.SelectFirstFlightByCarrier(false, true,
                captureFareTitle: true, requiredCabinTitle: "Delta Main",
                requiredOperatingCarrierCode: "DL", requiredConnections: 0);
            Assert.True(selected.OperatingCarrierCode.Is("DL"));
            Assert.True(selected.CabinName.Is("Delta Main"));
            Assert.False(selected.FareTitle.IsNullOrWhiteSpace());
        }, expectGuidance: true);
}
