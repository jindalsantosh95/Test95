using System.Runtime.CompilerServices;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.MobileApp.UI.Flows;
using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using VAA.EAPI.Automation.MobileApp.UI.Models;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public abstract class ReturnMultiPassengerFlightSearchTestBase : SearchBookTestBase
{
    protected ReturnMultiPassengerFlightSearchTestBase(ITestOutputHelper output)
        : base(output)
    {
    }

    protected void RunReturn(
        bool virginOutbound,
        bool virginInbound,
        bool completeCheckout,
        [CallerMemberName] string testName = "")
    {
        TrySetBrowserStackSessionName(testName);
        try
        {
            PassengerCounts passengers = new(Adults: 2, Children: 1);
            FlightSearch search = FlightSearch.Return(
                "LHR", "JFK", monthsFromToday: 3,
                passengers: passengers);
            Handlers.AppEntryHandler entryHandler = new(Fixture.Driver);
            NativeFlightSearchPage searchPage = new(Fixture.Driver);

            entryHandler.EnterAsGuest();
            searchPage.Open();
            searchPage.SearchReturn(search);
            searchPage.WaitForResults();

            string outboundPriceContext = searchPage.ReadPriceContext();
            Assert.Contains("Round trip", outboundPriceContext, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("3", outboundPriceContext, StringComparison.OrdinalIgnoreCase);

            SelectedFlight outbound = searchPage.SelectFirstFlightByCarrier(
                virginOutbound, expectFlightSummary: false);
            SelectedFlight inbound = searchPage.SelectFirstFlightByCarrier(
                virginInbound, expectFlightSummary: true);
            FlightSummary summary = searchPage.ReadSummary();

            Assert.Contains(search.Origin, summary.OriginText, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(search.Destination, summary.DestinationText, StringComparison.OrdinalIgnoreCase);

            Assert.Equal(virginOutbound,
                outbound.OperatingCarrierCode.Is("VS"));
            Assert.Equal(virginInbound,
                inbound.OperatingCarrierCode.Is("VS"));

            if (!completeCheckout)
            {
                TrySetBrowserStackStatus("passed",
                    "Two adults and one child reached combined Flight Summary with a Virgin outbound " +
                    "and partner inbound. Checkout is owned by TC01 and TC03.",
                    testName);
                return;
            }

            CompletedNativeBooking booking = new NativeBookingCheckoutFlow(Fixture.Driver).Complete(passengers);
            Assert.False(booking.Confirmation.TotalPriceText.IsNullOrWhiteSpace());

            ReportSuccessfulBooking(booking,
                "Two adults and one child completed the required " +
                "outbound/inbound carrier mix, paid, confirmed and found it in Trips.",
                testName);
        }
        catch (Exception exception)
        {
            TrySetBrowserStackStatus("failed", DescribeBrowserStackFailure(exception), testName);
            throw;
        }
    }
}

public sealed class VirginReturnMultiPassengerFlightSearchTests : ReturnMultiPassengerFlightSearchTestBase
{
    public VirginReturnMultiPassengerFlightSearchTests(ITestOutputHelper output)
        : base(output)
    {
    }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-SmokeE2E")]
    [Trait("TestType", "SmokeE2E")]
    [Trait("Suite", "Regression")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Journey", "Return")]
    [Trait("Passengers", "MultiPassenger")]
    [Trait("Carrier", "Virgin")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC03_TwoAdultsAndChildReturn_SelectsVirginOutboundAndInbound()
        => RunReturn(virginOutbound: true, virginInbound: true, completeCheckout: true);
}
