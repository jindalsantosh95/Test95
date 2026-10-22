using System.Runtime.CompilerServices;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.MobileApp.UI.Flows;
using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using VAA.EAPI.Automation.MobileApp.UI.Models;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public abstract class OneWayFlightSearchTestBase : SearchBookTestBase
{
    protected OneWayFlightSearchTestBase(ITestOutputHelper output)
        : base(output)
    {
    }

    protected void RunMixedPassengerOneWay(
        bool virginFlight,
        bool completeCheckout,
        [CallerMemberName] string testName = "")
    {
        TrySetBrowserStackSessionName(testName);
        try
        {
            PassengerCounts passengers = new(Adults: 2, Teens: 1, Children: 1, Infants: 1);
            FlightSearch search = FlightSearch.OneWay(
                "LHR", "JFK", monthsFromToday: 3, passengers: passengers);
            Handlers.AppEntryHandler entryHandler = new(Fixture.Driver);
            NativeFlightSearchPage searchPage = new(Fixture.Driver);

            entryHandler.EnterAsGuest();
            searchPage.Open();
            searchPage.SearchOneWay(search);
            searchPage.WaitForResults();
            SelectedFlight selected = searchPage.SelectFirstFlightByCarrier(
                virginFlight, expectFlightSummary: true);
            FlightSummary summary = searchPage.ReadSummary();

            Assert.Contains(search.Origin, summary.OriginText, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(search.Destination, summary.DestinationText, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(selected.CabinName, summary.FareText, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(virginFlight,
                selected.OperatingCarrierCode.Is("VS"));

            if (!completeCheckout)
            {
                TrySetBrowserStackStatus("passed",
                    $"Five mixed passengers reached Flight Summary for a non-Virgin one-way " +
                    $"flight in {selected.CabinName}. Checkout is owned by TC01 and TC03.",
                    testName);
                return;
            }

            CompletedNativeBooking booking = new NativeBookingCheckoutFlow(Fixture.Driver).Complete(passengers);
            Assert.False(booking.Confirmation.TotalPriceText.IsNullOrWhiteSpace());

            string carrier = virginFlight ? "Virgin Atlantic" : "non-Virgin";
            ReportSuccessfulBooking(booking,
                $"Five mixed passengers completed a {carrier} " +
                $"one-way flight in {selected.CabinName}, paid, confirmed and found it in Trips.",
                testName);
        }
        catch (Exception exception)
        {
            TrySetBrowserStackStatus("failed", DescribeBrowserStackFailure(exception), testName);
            throw;
        }
    }

    protected void RunSingleAdultUSOrigin([CallerMemberName] string testName = "")
    {
        TrySetBrowserStackSessionName(testName);
        try
        {
            PassengerCounts passengers = PassengerCounts.OneAdult;
            FlightSearch search = FlightSearch.OneWay(
                "JFK", "LHR", monthsFromToday: 3, passengers: passengers);
            Handlers.AppEntryHandler entryHandler = new(Fixture.Driver);
            NativeFlightSearchPage searchPage = new(Fixture.Driver);

            entryHandler.EnterAsGuest();
            searchPage.Open();
            searchPage.SearchOneWay(search);
            searchPage.WaitForResults();
            searchPage.AssertVisibleResultPricesUseCurrency("$");
            SelectedFlight selected = searchPage.SelectFirstFlightByCarrier(
                virginFlight: true, expectFlightSummary: true, requiredCabin: "Economy",
                requiredFareTitle: "Economy Classic");
            FlightSummary summary = searchPage.ReadSummary();
            Assert.Contains(search.Origin, summary.OriginText, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(search.Destination, summary.DestinationText, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(selected.CabinName, summary.FareText, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Economy Classic", summary.FareText, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("$", summary.PriceText);

            CompletedNativeBooking booking = new NativeBookingCheckoutFlow(Fixture.Driver).Complete(passengers);
            Assert.Contains("$", booking.Confirmation.TotalPriceText);
            ReportSuccessfulBooking(booking,
                "One guest adult completed a JFK to LHR booking with USD results, summary and confirmation, and found it in Trips.",
                testName);
        }
        catch (Exception exception)
        {
            TrySetBrowserStackStatus("failed", DescribeBrowserStackFailure(exception), testName);
            throw;
        }
    }
}

public sealed class VirginOneWayFlightSearchTests : OneWayFlightSearchTestBase
{
    public VirginOneWayFlightSearchTests(ITestOutputHelper output)
        : base(output)
    {
    }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-SmokeE2E")]
    [Trait("TestType", "SmokeE2E")]
    [Trait("Suite", "Regression")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Journey", "OneWay")]
    [Trait("Carrier", "Virgin")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC01_MixedPassengerOneWay_SelectsVirginFlight()
        => RunMixedPassengerOneWay(virginFlight: true, completeCheckout: true);
}

public sealed class USOriginOneWayFlightSearchTests : OneWayFlightSearchTestBase
{
    public USOriginOneWayFlightSearchTests(ITestOutputHelper output)
        : base(output)
    {
    }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-SmokeE2E")]
    [Trait("TestType", "SmokeE2E")]
    [Trait("Suite", "Sanity")]
    [Trait("Suite", "Regression")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Journey", "OneWay")]
    [Trait("Market", "US")]
    [Trait("Passengers", "OneAdult")]
    [Trait("Carrier", "Virgin")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC23_OneAdultGuestFromJfkToLhr_CompletesUsdBookingAndAppearsInTrips()
        => RunSingleAdultUSOrigin();
}
