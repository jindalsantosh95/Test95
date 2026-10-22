using Microsoft.Playwright;

namespace VAA.EAPI.Automation.InFlightApplications.UI.Pages;

public sealed class OtpPage : BasePage
{
    private ILocator Code => Page.Locator("input[name='code']");

    private ILocator Submit => Page.Locator("button[type='submit']");

    private ILocator Form => Page.Locator("form");

    public OtpPage(IPage page) : base(page)
    {
    }

    public async Task WaitUntilOpenAsync()
    {
        await Code.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task EnterCodeAsync(string code)
    {
        await Code.FillAsync(code);
    }

    public async Task SubmitAsync()
    {
        await ClickAsync(Submit);
    }

    public async Task WaitForInvalidCodeErrorAsync()
    {
        await Form.GetByText("There was an issue with your code", new() { Exact = false })
            .WaitForAsync(new() { State = WaitForSelectorState.Visible });
    }

    public async Task<FormState> ReadStateAsync()
    {
        return new FormState(Page.Url, await Submit.IsEnabledAsync(), await Form.InnerTextAsync());
    }
}
