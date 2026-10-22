using Microsoft.Playwright;
using VAA.EAPI.Automation.Core.Playwright.Browser;

namespace VAA.EAPI.Automation.SearchAndBook.UI.Pages;

/// <summary>
/// Owns browser-level interaction with the Virgin Atlantic application shell.
/// </summary>
public sealed class ApplicationPage : BasePage
{
    public ApplicationPage(IPage page)
        : base(page)
    {
    }

    public async Task OpenAsync(
        Uri baseUrl,
        string relativeUrl,
        int timeoutMs)
    {
        await ApplicationNavigation.NavigateAsync(
            Page,
            baseUrl,
            relativeUrl,
            WaitUntilState.Commit,
            timeoutMs);
        await DismissPrivacyBannerIfPresentAsync();
    }

    public Task DismissPrivacyBannerIfPresentAsync() =>
        VirginAtlanticConsent.DismissPrivacyBannerIfPresentAsync(Page);

    public Task PauseForInteractiveInspectionAsync() => Page.PauseAsync();
}
