using Microsoft.Playwright;

using VAA.EAPI.Automation.Core.Playwright.Accessibility;

namespace VAA.EAPI.Automation.InFlightApplications.UI.Pages;

public sealed class PortalPage : BasePage
{
    public PortalPage(IPage page) : base(page)
    {
    }

    public async Task OpenAsync(string relativePath)
    {
        await Page.GotoAsync(relativePath, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
    }

    public async Task WaitUntilReadyAsync()
    {
        await Page.Locator("body").WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public string CurrentUrl => Page.Url;

    public async Task<string> ReadTitleAsync()
    {
        return await Page.TitleAsync();
    }

    public async Task<AccessibilityScanResult> ScanAccessibilityAsync()
    {
        await VAA.EAPI.Automation.Core.Playwright.Browser.VirginAtlanticConsent
            .DismissPrivacyBannerIfPresentAsync(Page);
        PlaywrightAccessibilityScanner scanner = new();
        return await scanner.ScanAsync(Page);
    }
}
