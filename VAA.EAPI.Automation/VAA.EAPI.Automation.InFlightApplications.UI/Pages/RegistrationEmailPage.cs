using Microsoft.Playwright;

namespace VAA.EAPI.Automation.InFlightApplications.UI.Pages;

public sealed class RegistrationEmailPage : BasePage
{
    private ILocator Email => Page.Locator("input[name='email']");

    private ILocator Submit => Page.Locator("button[type='submit']");

    private ILocator Form => Page.Locator("form");

    public RegistrationEmailPage(IPage page) : base(page)
    {
    }

    public async Task WaitUntilOpenAsync()
    {
        await Email.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task EnterEmailAsync(string email)
    {
        await Email.FillAsync(email);
    }

    public async Task<FormState> ReadStateAsync()
    {
        return new FormState(Page.Url, await Submit.IsEnabledAsync(), await Form.InnerTextAsync());
    }

    public async Task SubmitAsync()
    {
        await ClickAsync(Submit);
    }
}
