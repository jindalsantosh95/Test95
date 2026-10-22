using Microsoft.Playwright;

using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.InFlightApplications.UI.Pages;

public sealed class RegistrationProfilePage : BasePage
{
    private ILocator FirstName => Page.Locator("input[name='firstName']");

    private ILocator LastName => Page.Locator("input[name='lastName']");

    private ILocator DateOfBirth => Page.Locator("input[name='dateOfBirth']");

    private ILocator Gender => Page.Locator("select[name='gender']");

    private ILocator Submit => Page.Locator("button[type='submit']");

    private ILocator Form => Page.Locator("form");

    public RegistrationProfilePage(IPage page) : base(page)
    {
    }

    public async Task OpenAsync()
    {
        await Page.Context.ClearCookiesAsync();
        await Page.GotoAsync("/register", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        await FirstName.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
    }

    public async Task EnterProfileAsync(string firstName, string lastName, DateOnly dateOfBirth, string? gender = null)
    {
        await FirstName.FillAsync(firstName);
        await LastName.FillAsync(lastName);
        await DateOfBirth.FillAsync(dateOfBirth.ToString("ddMMyyyy"));

        if (gender is not null && !gender.IsNullOrWhiteSpace())
        {
            string genderLabel = ToGenderLabel(gender);
            await Gender.SelectOptionAsync(new SelectOptionValue { Label = genderLabel });
        }
    }

    private static string ToGenderLabel(string gender)
    {
        return gender.Trim().ToUpperInvariant() switch
        {
            "M" => "Male",
            "F" => "Female",
            "X" => "Unspecified (x)",
            "U" => "Undisclosed (u)",
            _ => throw new ArgumentException($"Unsupported gender code: {gender}", nameof(gender))
        };
    }

    public async Task<FormState> ReadStateAsync()
    {
        string? selectedGenderLabel = await Gender.Locator("option:checked").CountAsync() > 0
            ? await Gender.Locator("option:checked").InnerTextAsync()
            : null;

        return new FormState(
            Page.Url,
            await Submit.IsEnabledAsync(),
            await Form.InnerTextAsync(),
            selectedGenderLabel);
    }

    public async Task SubmitAsync()
    {
        await ClickAsync(Submit);
    }
}
