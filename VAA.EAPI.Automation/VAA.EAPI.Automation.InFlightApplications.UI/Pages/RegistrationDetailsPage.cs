using Microsoft.Playwright;

namespace VAA.EAPI.Automation.InFlightApplications.UI.Pages;

public sealed class RegistrationDetailsPage : BasePage
{
    private ILocator Heading => Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
    {
        Name = "Where in the world are you?"
    });

    private ILocator Country => Page.Locator("select[name='countryRegion']");

    private ILocator MobileCountry => Page.Locator("select[name='mobileCountry']");

    private ILocator MobileNumber => Page.Locator("input[name='mobileNumber']");

    private ILocator AddressLine1 => Page.Locator("input[name='addressLine1']");

    private ILocator City => Page.Locator("input[name='city']");

    private ILocator County => Page.Locator("input[name='county']");

    private ILocator Postcode => Page.Locator("input[name='postcode']");

    private ILocator FindAddress => Page.GetByRole(AriaRole.Button, new() { Name = "Find address" });

    private ILocator AddAddressManually => Page.GetByRole(AriaRole.Button, new() { Name = "Add address manually" });

    private ILocator Submit => Page.Locator("button[type='submit']");

    private ILocator Form => Page.Locator("form");

    public RegistrationDetailsPage(IPage page) : base(page)
    {
    }

    public async Task<JourneyResult> WaitUntilOpenAsync()
    {
        await Heading.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        return new JourneyResult(Page.Url, await Heading.InnerTextAsync());
    }

    public async Task EnterMobileAsync(string mobileNumber)
    {
        await MobileNumber.FillAsync(mobileNumber);
        await MobileNumber.BlurAsync();
    }

    public async Task SelectCountryAsync(string countryCode)
    {
        await Country.SelectOptionAsync(countryCode);
    }

    public async Task EnterLookupAddressAsync(string addressLine1, string postcode)
    {
        await AddressLine1.FillAsync(addressLine1);
        await Postcode.FillAsync(postcode);
        await Postcode.BlurAsync();
    }

    public async Task EnterManualAddressAsync(
        string addressLine1,
        string city,
        string county,
        string postcode)
    {
        await AddressLine1.FillAsync(addressLine1);
        await City.FillAsync(city);
        await County.FillAsync(county);
        await Postcode.FillAsync(postcode);
        await Postcode.BlurAsync();
    }

    public async Task FindAddressAsync()
    {
        await ClickAsync(FindAddress);
    }

    public async Task WaitForValidationAsync(string expectedText)
    {
        await Page.GetByText(expectedText, new() { Exact = false })
            .WaitForAsync(new() { State = WaitForSelectorState.Visible });
    }

    public async Task CompleteUnresolvedAddressAsync()
    {
        await EnterMobileAsync("07917414088");
        await AddressLine1.FillAsync("241 Old Shoreham Road");
        await Postcode.FillAsync("BN41 1XR");
    }

    public async Task CompleteManualAddressAsync()
    {
        await EnterMobileAsync("07917414088");
        await ClickAsync(AddAddressManually);
        await AddressLine1.FillAsync("241 Old Shoreham Road");
        await City.FillAsync("Brighton");
        await County.FillAsync("East Sussex");
        await Postcode.FillAsync("BN41 1XR");
    }

    public async Task SubmitAsync()
    {
        await ClickAsync(Submit);
    }

    public async Task<DetailsState> ReadStateAsync()
    {
        return new DetailsState(
            Page.Url,
            await Submit.IsEnabledAsync(),
            await Form.InnerTextAsync(),
            await AddressLine1.IsVisibleAsync(),
            await City.IsVisibleAsync(),
            await FindAddress.IsVisibleAsync(),
            await AddAddressManually.IsVisibleAsync(),
            await MobileCountry.InputValueAsync(),
            await MobileNumber.InputValueAsync());
    }
}

public sealed record DetailsState(
    string Url,
    bool IsSubmitEnabled,
    string VisibleText,
    bool IsAddressLineVisible,
    bool IsCityVisible,
    bool IsFindAddressVisible,
    bool IsAddAddressManuallyVisible,
    string MobileCountry,
    string MobileNumber);
