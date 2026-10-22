using Microsoft.Playwright;

namespace VAA.EAPI.Automation.InFlightApplications.UI.Pages;

public sealed class PasswordResetNewPasswordPage : BasePage
{
    private const string SuccessClass = "bg-background-semantic-success-bold";

    private ILocator NewPassword => Page.Locator("input[name='newPassword']");

    private ILocator ConfirmPassword => Page.Locator("input[name='confirmPassword']");

    private ILocator Submit => Page.Locator("button[type='submit']");

    public PasswordResetNewPasswordPage(IPage page) : base(page)
    {
    }

    public async Task WaitUntilOpenAsync()
    {
        await NewPassword.WaitForAsync(new() { State = WaitForSelectorState.Visible });
    }

    public async Task EnterNewPasswordAsync(string value)
    {
        await NewPassword.FillAsync(value);
        await NewPassword.BlurAsync();
    }

    public async Task EnterConfirmationAsync(string value)
    {
        await ConfirmPassword.FillAsync(value);
        await ConfirmPassword.BlurAsync();
    }

    public async Task ClearAsync()
    {
        await NewPassword.FillAsync(string.Empty);
        await ConfirmPassword.FillAsync(string.Empty);
    }

    public async Task<PasswordRulesState> ReadRulesAsync()
    {
        return new PasswordRulesState(
            Page.Url,
            await Submit.IsEnabledAsync(),
            await IsRuleSatisfiedAsync("8-20 characters"),
            await IsRuleSatisfiedAsync("1 number"),
            await IsRuleSatisfiedAsync("1 upper case letter"),
            await IsRuleSatisfiedAsync("1 lower case letter"),
            await IsRuleSatisfiedAsync("No more than 3 special characters"),
            await IsRuleSatisfiedAsync("Cannot contain"),
            await IsRuleSatisfiedAsync("Passwords must match"));
    }

    private async Task<bool> IsRuleSatisfiedAsync(string ruleText)
    {
        ILocator indicator = Page.Locator("li", new() { HasText = ruleText }).Locator("span").First;
        string? classes = await indicator.GetAttributeAsync("class");
        return classes?.Contains(SuccessClass, StringComparison.Ordinal) == true;
    }
}

public sealed record PasswordRulesState(
    string Url,
    bool IsSubmitEnabled,
    bool HasValidLength,
    bool HasNumber,
    bool HasUpperCase,
    bool HasLowerCase,
    bool HasValidSpecialCharacters,
    bool HasNoForbiddenSymbols,
    bool PasswordsMatch);
