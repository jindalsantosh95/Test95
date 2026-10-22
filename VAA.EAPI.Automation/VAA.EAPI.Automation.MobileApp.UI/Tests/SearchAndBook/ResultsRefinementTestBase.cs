using VAA.EAPI.Automation.MobileApp.UI.Handlers;
using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using VAA.EAPI.Automation.MobileApp.UI.Models;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using System.Runtime.CompilerServices;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public abstract class ResultsRefinementTestBase : SearchBookTestBase
{
    protected ResultsRefinementTestBase(ITestOutputHelper output)
        : base(output)
    {
    }

    protected NativeFlightSearchPage OpenOneWayResults([CallerMemberName] string testName = "")
    {
        TrySetBrowserStackSessionName(testName);
        NativeFlightSearchPage page = new(Fixture.Driver);
        new AppEntryHandler(Fixture.Driver).EnterAsGuest();
        page.Open();
        page.SearchOneWay(FlightSearch.OneWay(
            "LHR", "JFK", monthsFromToday: 3, passengers: new PassengerCounts(1, 0, 0, 0)));
        page.WaitForResults();
        page.RestoreDefaultRefinements();
        return page;
    }

    protected void RunRefinementTest(
        Action<NativeFlightSearchPage> assertion,
        string successReason,
        [CallerMemberName] string testName = "")
    {
        TrySetBrowserStackSessionName(testName);
        try
        {
            assertion(OpenOneWayResults(testName));
            TrySetBrowserStackStatus("passed", successReason, testName);
        }
        catch (Exception exception)
        {
            TrySetBrowserStackStatus("failed", DescribeBrowserStackFailure(exception), testName);
            throw;
        }
    }
}
