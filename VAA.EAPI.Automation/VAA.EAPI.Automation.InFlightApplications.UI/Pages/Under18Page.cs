using Microsoft.Playwright;

namespace VAA.EAPI.Automation.InFlightApplications.UI.Pages;

public sealed class Under18Page : BasePage
{
    private ILocator Heading => Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
    {
        Name = "Flying Club can wait"
    });

    private ILocator Continue => Page.Locator("button[type='submit']");

    public Under18Page(IPage page) : base(page)
    {
    }

    public async Task WaitUntilOpenAsync()
    {
        await Heading.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task ContinueAsync()
    {
        await ClickAsync(Continue);
    }
}
