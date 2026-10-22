using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.MobileApp.UI.Assertions;
using VAA.EAPI.Automation.MobileApp.UI.Flows;
using VAA.EAPI.Automation.MobileApp.UI.Handlers;
using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using VAA.EAPI.Automation.MobileApp.UI.Models;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public abstract class ControlledSeatMapUnavailableTestBase(ITestOutputHelper output)
    : ControlledSearchFailureTestBase(output, EapiFaultCase.SeatMapFailure)
{
    protected async Task VerifyOutboundFailureAllowsInboundMapAsync(string testName)
    {
        try
        {
            NativeFlightSearchPage search = await EnterAndStartProxyAsync();
            search.SearchReturn(FlightSearch.Return("LHR", "JFK", 3, passengers: PassengerCounts.OneAdult));
            search.WaitForResults();
            search.SelectFirstFlightByCarrier(true, false, requiredCabin: "Economy",
                requiredFareTitle: "Economy Classic");
            search.SelectFirstFlightByCarrier(true, true, requiredCabin: "Economy",
                requiredFareTitle: "Economy Classic");

            new NativeBookingCheckoutFlow(Fixture.Driver).OpenSeatSelectionForOneGuestAdult();
            await ActiveProxy.WaitForInjectionAsync(TimeSpan.FromSeconds(30));
            NativeSeatSelectionPage seats = new(Fixture.Driver);
            seats.WaitForMapUnavailableMessage();
            ActiveProxy.EnsureNoGuardViolation();

            // Query rules inject once; the same isolated proxy forwards the next sector normally.
            seats.ContinuePastUnavailableMap();
            seats.WaitForAvailableMap();
            TrySetBrowserStackStatus("passed",
                "Outbound seat map unavailable reassurance displayed without Retry; Continue opened the inbound seat map.", testName);
        }
        catch (Exception exception)
        {
            TrySetBrowserStackStatus("failed", DescribeBrowserStackFailure(exception), testName);
            throw;
        }
    }

    protected async Task VerifyInboundFailureAllowsPaymentProgressionAsync(string testName)
    {
        try
        {
            NativeFlightSearchPage search = await EnterAndStartProxyAsync();
            search.SearchReturn(FlightSearch.Return("LHR", "JFK", 3, passengers: PassengerCounts.OneAdult));
            search.WaitForResults();
            SelectedFlight outbound = search.SelectFirstFlightByCarrier(false, false,
                requiredFareTitle: "Economy Standard", requireUnsupportedSeatCarriers: true,
                requiredCabinTitle: "Economy Standard");
            Assert.NotNull(outbound.Details);
            Assert.True(SeatCarrierEligibilityAssertions.AllLegsAreKnownUnsupported(outbound.Details));
            SelectedFlight inbound = search.SelectFirstFlightByCarrier(true, true, requiredCabin: "Economy",
                requiredFareTitle: "Economy Classic");
            Assert.True(inbound.OperatingCarrierCode.Is("VS"));

            new NativeBookingCheckoutFlow(Fixture.Driver).OpenSeatSelectionForOneGuestAdult();
            await ActiveProxy.WaitForInjectionAsync(TimeSpan.FromSeconds(30));
            NativeSeatSelectionPage seats = new(Fixture.Driver);
            seats.WaitForMapUnavailableMessage();
            ActiveProxy.EnsureNoGuardViolation();
            seats.ContinuePastUnavailableMap();
            seats.WaitForPaymentHandoff();
            TrySetBrowserStackStatus("passed",
                "Inbound seat map unavailable reassurance displayed without Retry; Continue reached payment without a payment action.", testName);
        }
        catch (Exception exception)
        {
            TrySetBrowserStackStatus("failed", DescribeBrowserStackFailure(exception), testName);
            throw;
        }
    }

    private async Task<NativeFlightSearchPage> EnterAndStartProxyAsync()
    {
        Assert.True(MobileFaultFactAttribute.CanRun(Fixture.Config, postHpp: false));
        new AppEntryHandler(Fixture.Driver).EnterAsGuest();
        NativeFlightSearchPage search = new(Fixture.Driver);
        search.Open();
        await StartLocalProxyIfRequiredAsync();
        return search;
    }
}

public sealed class ControlledOutboundSeatMapFailureTests(ITestOutputHelper output)
    : ControlledSeatMapUnavailableTestBase(output)
{
    [MobileFaultFact]
    [Trait("Category", "MobileApp-SearchBook-FaultInjection")]
    [Trait("Suite", "FaultInjection")]
    [Trait("Area", "SearchAndBook")]
    [Trait("Feature", "Seats-ErrorHandling")]
    [Trait("TestType", "Functional")]
    [Trait("Readiness", "Draft")]
    [Trait("Platform", "iOS")]
    public Task SearchBookFault_TC10_OutboundSeatMapFailure_ShowsReassuranceAndOpensInboundMap()
        => VerifyOutboundFailureAllowsInboundMapAsync(
            nameof(SearchBookFault_TC10_OutboundSeatMapFailure_ShowsReassuranceAndOpensInboundMap));
}

public sealed class ControlledInboundSeatMapFailureTests(ITestOutputHelper output)
    : ControlledSeatMapUnavailableTestBase(output)
{
    [MobileFaultFact]
    [Trait("Category", "MobileApp-SearchBook-FaultInjection")]
    [Trait("Suite", "FaultInjection")]
    [Trait("Area", "SearchAndBook")]
    [Trait("Feature", "Seats-ErrorHandling")]
    [Trait("TestType", "Functional")]
    [Trait("Readiness", "Draft")]
    [Trait("Platform", "iOS")]
    public Task SearchBookFault_TC12_InboundSeatMapFailure_ShowsReassuranceAndContinuesToPayment()
        => VerifyInboundFailureAllowsPaymentProgressionAsync(
            nameof(SearchBookFault_TC12_InboundSeatMapFailure_ShowsReassuranceAndContinuesToPayment));
}
