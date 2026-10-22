using Microsoft.Playwright;

namespace VAA.EAPI.Automation.InFlightApplications.UI.Pages;

public sealed class PasswordResetPage : BasePage
{
    private ILocator Identifier => Page.Locator("input[name='identifier']");

    private ILocator Submit => Page.Locator("button[type='submit']");

    private ILocator Form => Page.Locator("form");

    public PasswordResetPage(IPage page) : base(page)
    {
    }

    public async Task OpenAsync()
    {
        await Page.GotoAsync("/forgotten-password", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await Identifier.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task EnterIdentifierAsync(string identifier)
    {
        await Identifier.FillAsync(identifier);
    }

    public async Task<FormState> ReadStateAsync()
    {
        return new FormState(Page.Url, await Submit.IsEnabledAsync(), await Form.InnerTextAsync());
    }

    public async Task SubmitAsync()
    {
        await ClickAsync(Submit);
    }

    public async Task SubmitAndWaitForOtpAsync()
    {
        for (int attempt = 1; attempt <= 2; attempt++)
        {
            await ClickAsync(Submit);
            try
            {
                await Page.WaitForURLAsync(
                    "**/forgotten-password/code",
                    new() { Timeout = 20_000 });
                return;
            }
            catch (System.TimeoutException exception)
            {
                if (attempt == 2)
                {
                    throw new InvalidOperationException(
                        "Password reset did not open the OTP page after two attempts.",
                        exception);
                }

                // The upstream reset request occasionally accepts a retry without navigating.
            }
        }
    }
}
