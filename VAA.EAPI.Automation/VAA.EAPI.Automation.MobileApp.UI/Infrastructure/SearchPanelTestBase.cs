using System.Runtime.CompilerServices;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Infrastructure;

/// <summary>Uses configured driver/platform; never overrides the user's run mode or device.</summary>
public abstract class SearchPanelTestBase : SearchBookTestBase
{
    protected SearchPanelTestBase(ITestOutputHelper output) : base(output) { }

    protected void RunPanelScenario(Action<IFlightSearchPanelPage> scenario, [CallerMemberName] string testName = "")
    {
        TrySetBrowserStackSessionName(testName);
        try
        {
            IFlightSearchPanelPage page = FlightSearchPanelPageFactory.Create(Fixture.Driver);
            page.Open();
            scenario(page);
            TrySetBrowserStackStatus("passed", "Search panel assertions completed; no search submitted.", testName);
        }
        catch (Exception exception)
        {
            TrySetBrowserStackStatus("failed", DescribeBrowserStackFailure(exception), testName);
            throw;
        }
    }
}
