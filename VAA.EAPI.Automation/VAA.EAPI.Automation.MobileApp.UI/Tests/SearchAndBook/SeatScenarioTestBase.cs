using System.Diagnostics;
using VAA.EAPI.Automation.MobileApp.UI.Flows;
using VAA.EAPI.Automation.MobileApp.UI.Handlers;
using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using VAA.EAPI.Automation.MobileApp.UI.Models;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public abstract class SeatScenarioTestBase(ITestOutputHelper output) : SearchBookTestBase(output,
    new Dictionary<string, string?> { ["IOSLaunch:AutomationFeatureFlags"] = "850100-search-native-entry" })
{
    protected void RunSeatScenario(string testName, Action<NativeSeatSelectionPage> scenario,
        PassengerCounts? passengers = null)
    {
        TrySetBrowserStackSessionName(testName);
        try
        {
            Stopwatch duration = Stopwatch.StartNew();
            new AppEntryHandler(Fixture.Driver).EnterAsGuest();
            NativeFlightSearchPage search = new(Fixture.Driver);
            search.Open();
            PassengerCounts party = passengers ?? PassengerCounts.OneAdult;
            search.SearchOneWay(FlightSearch.OneWay("LHR", "JFK", 3, party));
            search.WaitForResults();
            search.SelectFirstFlightByCarrier(virginFlight: true, expectFlightSummary: true,
                requiredCabin: "Economy", requiredFareTitle: "Economy Classic");
            Output.WriteLine($"Seat timing: startup/search/summary = {duration.Elapsed.TotalSeconds:F2}s");
            duration.Restart();
            new NativeBookingCheckoutFlow(Fixture.Driver).OpenSeatSelectionForGuests(party);
            Output.WriteLine($"Seat timing: passenger entry/submission/seat screen = {duration.Elapsed.TotalSeconds:F2}s");
            NativeSeatSelectionPage seats = new(Fixture.Driver, message => Output.WriteLine(message));
            // The heading can precede the loaded map/passenger controls. All seat-only
            // scenarios must observe readiness before reading the initial assignment.
            duration.Restart();
            seats.WaitForActivePassenger(0);
            Output.WriteLine($"Seat timing: initial passenger readiness = {duration.Elapsed.TotalSeconds:F2}s");
            scenario(seats);
            TrySetBrowserStackStatus("passed", "Seat-only assertions passed; no seat submission or payment.", testName);
        }
        catch (Exception exception)
        {
            TrySetBrowserStackStatus("failed", DescribeBrowserStackFailure(exception), testName);
            throw;
        }
    }
}
