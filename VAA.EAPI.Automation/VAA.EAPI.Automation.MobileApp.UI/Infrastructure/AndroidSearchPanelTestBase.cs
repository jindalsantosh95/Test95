using System.Runtime.CompilerServices;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Infrastructure;

public abstract class AndroidSearchPanelTestBase : SearchBookTestBase
{
    protected AndroidSearchPanelTestBase(ITestOutputHelper output) : base(output)
    { }

    protected void RunPanelScenario(Action<AndroidSearchPanelPage> scenario, [CallerMemberName] string testName = "")
    {
        try
        {
            AndroidSearchPanelPage page = FlightSearchPanelPageFactory.Create(Fixture.Driver) as AndroidSearchPanelPage
                ?? throw new NotSupportedException("This Android-only UI scenario requires an Android-configured session.");
            page.Open();
            scenario(page);
            TrySetBrowserStackStatus("passed", "Android Search panel assertions completed; no flight search submitted.", testName);
        }
        catch (Exception exception)
        {
            TrySetBrowserStackStatus("failed", DescribeBrowserStackFailure(exception), testName);
            throw;
        }
    }
}
