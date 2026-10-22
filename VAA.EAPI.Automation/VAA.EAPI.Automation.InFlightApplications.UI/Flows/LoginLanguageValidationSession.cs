using VAA.EAPI.Automation.Core.Playwright.Browser;
using VAA.EAPI.Automation.InFlightApplications.UI.Pages;

namespace VAA.EAPI.Automation.InFlightApplications.UI.Flows;

public sealed class LoginLanguageValidationSession : IAsyncDisposable
{
    private readonly PlaywrightBrowserSession _browserSession;
    private readonly LoginPage _loginPage;

    internal LoginLanguageValidationSession(
        PlaywrightBrowserSession browserSession,
        LoginPage loginPage)
    {
        _browserSession = browserSession;
        _loginPage = loginPage;
    }

    public Task<LanguageState> ReadLanguageStateAsync() => _loginPage.ReadLanguageStateAsync();

    public Task SelectLanguageWithKeyboardAsync(string language) =>
        _loginPage.SelectLanguageWithKeyboardAsync(language);

    public Task MarkPassedAsync(string reason) => _browserSession.MarkPassedAsync(reason);

    public ValueTask DisposeAsync() => _browserSession.DisposeAsync();
}
