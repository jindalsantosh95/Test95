using System.Runtime.CompilerServices;
using VAA.EAPI.Automation.MobileApp.UI.Handlers;
using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public abstract class DiscountCodePanelTestBase : SearchBookTestBase
{
    protected DiscountCodePanelTestBase(ITestOutputHelper output) : base(output) { }

    protected void RunDiscountCodeScenario(
        Action<NativeFlightSearchPage> scenario,
        [CallerMemberName] string testName = "")
    {
        TrySetBrowserStackSessionName(testName);
        try
        {
            new AppEntryHandler(Fixture.Driver).EnterAsGuest();
            NativeFlightSearchPage page = new(Fixture.Driver);
            page.Open();
            // Local installations may retain state; establish a clean panel through its own UI.
            page.OpenDiscountCodeEditor();
            page.ClearDiscountCode();
            scenario(page);
            TrySetBrowserStackStatus("passed",
                "Discount-code panel contract verified; no search or discount entitlement requested.", testName);
        }
        catch (Exception exception)
        {
            TrySetBrowserStackStatus("failed", DescribeBrowserStackFailure(exception), testName);
            throw;
        }
    }
}
