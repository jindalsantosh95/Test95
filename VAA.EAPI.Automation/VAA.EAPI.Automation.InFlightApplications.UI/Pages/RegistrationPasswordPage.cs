using Microsoft.Playwright;

namespace VAA.EAPI.Automation.InFlightApplications.UI.Pages;

public sealed class RegistrationPasswordPage : BasePage
{
    private ILocator Password => Page.Locator("input[name='password']");

    private ILocator ConfirmPassword => Page.Locator("input[name='confirmPassword']");

    private ILocator FlyingClubMarketingOptOut => Page.Locator("input[name='flyingClubMarketingOptOut']");

    private ILocator VirginRedMarketingOptOut => Page.Locator("input[name='virginRedMarketingOptOut']");

    private ILocator Submit => Page.Locator("button[type='submit']");

    private ILocator Form => Page.Locator("form");

    public RegistrationPasswordPage(IPage page) : base(page)
    {
    }

    public async Task WaitUntilOpenAsync()
    {
        await Password.WaitForAsync(new() { State = WaitForSelectorState.Visible });
    }

    public async Task EnterPasswordsAsync(string password, string confirmation)
    {
        await Password.FillAsync(password);
        await ConfirmPassword.FillAsync(confirmation);
        await ConfirmPassword.BlurAsync();
    }

    public async Task SubmitAsync()
    {
        await ClickAsync(Submit);
    }

    public async Task<RegistrationPasswordState> ReadStateAsync()
    {
        return new RegistrationPasswordState(
            Page.Url,
            await Submit.IsEnabledAsync(),
            await Form.InnerTextAsync(),
            await FlyingClubMarketingOptOut.IsCheckedAsync(),
            await VirginRedMarketingOptOut.IsCheckedAsync());
    }
}

public sealed record RegistrationPasswordState(
    string Url,
    bool IsSubmitEnabled,
    string VisibleText,
    bool IsFlyingClubMarketingOptOutSelected,
    bool IsVirginRedMarketingOptOutSelected);
