using Microsoft.Playwright;

namespace VAA.EAPI.Automation.Trips.UI.Pages;

public abstract class BasePage
{
    protected BasePage(IPage page)
    {
        Page = page;
    }

    protected IPage Page { get; }
}
