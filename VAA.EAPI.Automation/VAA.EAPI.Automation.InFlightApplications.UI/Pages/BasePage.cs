using Microsoft.Playwright;

namespace VAA.EAPI.Automation.InFlightApplications.UI.Pages;

public abstract class BasePage
{
    protected BasePage(IPage page)
    {
        Page = page;
    }

    protected IPage Page { get; }

    protected async Task ClickAsync(ILocator locator)
    {
        await VAA.EAPI.Automation.Core.Playwright.Browser.VirginAtlanticConsent
            .DismissPrivacyBannerIfPresentAsync(Page);
        await locator.ClickAsync();
    }
}
