using Microsoft.Playwright;

using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.InFlightApplications.UI.Pages;

public sealed class RegistrationPreferencesPage : BasePage
{
    private ILocator FlyingClub => Page.Locator("input[name='earningPreference'][value='flying-club']");

    private ILocator Partner => Page.Locator("input[name='earningPreference'][value='partner']");

    private ILocator AirlineProgramme => Page.Locator("select[name='airlineProgramme']");

    private ILocator Submit => Page.Locator("button[type='submit']");

    private ILocator Form => Page.Locator("form");

    public RegistrationPreferencesPage(IPage page) : base(page)
    {
    }

    public async Task WaitUntilOpenAsync()
    {
        await FlyingClub.WaitForAsync(new() { State = WaitForSelectorState.Visible });
    }

    public async Task ChoosePartnerAsync()
    {
        await Partner.CheckAsync(new() { Force = true });
    }

    public async Task ChooseFlyingClubAsync()
    {
        await FlyingClub.CheckAsync(new() { Force = true });
    }

    public async Task SelectFirstAirlineProgrammeAsync()
    {
        await AirlineProgramme.WaitForAsync(new() { State = WaitForSelectorState.Visible });
        IReadOnlyList<IElementHandle> options = await AirlineProgramme.Locator("option:not([disabled])").ElementHandlesAsync();
        foreach (IElementHandle option in options)
        {
            string value = await option.GetAttributeAsync("value") ?? string.Empty;
            if (!value.IsNullOrWhiteSpace())
            {
                await AirlineProgramme.SelectOptionAsync(value);
                return;
            }
        }

        throw new InvalidOperationException("No selectable airline programme was returned.");
    }

    public async Task SubmitAsync()
    {
        await ClickAsync(Submit);
    }

    public async Task<PreferencesState> ReadStateAsync()
    {
        return new PreferencesState(
            Page.Url,
            await FlyingClub.IsCheckedAsync(),
            await AirlineProgramme.IsVisibleAsync(),
            await Submit.IsEnabledAsync(),
            await Form.InnerTextAsync());
    }
}

public sealed record PreferencesState(
    string Url,
    bool IsFlyingClubSelected,
    bool IsAirlineProgrammeVisible,
    bool IsSubmitEnabled,
    string VisibleText);
