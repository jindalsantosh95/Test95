using VAA.EAPI.Automation.Core.Playwright.Browser;
using VAA.EAPI.Automation.InFlightApplications.UI.Assertions;
using VAA.EAPI.Automation.InFlightApplications.UI.Flows;
using VAA.EAPI.Automation.InFlightApplications.UI.Infrastructure;
using VAA.EAPI.Automation.InFlightApplications.UI.Pages;

namespace VAA.EAPI.Automation.InFlightApplications.UI.Tests.Validation;

[Collection(StatefulTestCollection.Name)]
public sealed class RegistrationDetailsValidationTests
{
    [SkippableFact]
    [Trait("Category", "InFlightApplications-UI-OtpJourney")]
    public async Task UnresolvedAddress_DisablesSubmit()
    {
        DetailsState state = await RunAsync(RegistrationDetailsScenario.UnresolvedAddress);
        PortalAssertions.DetailsSubmitIsDisabled(state);
    }

    [SkippableFact]
    [Trait("Category", "InFlightApplications-UI-OtpJourney")]
    public async Task NonLookupCountry_ShowsManualAddressWithoutLookupControls()
    {
        DetailsState state = await RunAsync(RegistrationDetailsScenario.NonLookupCountry);
        PortalAssertions.ManualAddressIsShownWithoutLookup(state);
    }

    [SkippableFact]
    [Trait("Category", "InFlightApplications-UI-OtpJourney")]
    public async Task ChangingCountry_ResetsMobileDetails()
    {
        DetailsState state = await RunAsync(RegistrationDetailsScenario.CountryResetsMobile);
        PortalAssertions.MobileWasResetForUs(state);
    }

    [SkippableTheory]
    [InlineData(RegistrationDetailsScenario.InvalidUkPostcode, "Enter a valid UK postcode")]
    [InlineData(RegistrationDetailsScenario.InvalidUsZipCode, "Enter a valid US ZIP code")]
    [Trait("Category", "InFlightApplications-UI-OtpJourney")]
    public async Task InvalidPostcode_ShowsCountrySpecificValidation(
        RegistrationDetailsScenario scenario,
        string expectedText)
    {
        DetailsState state = await RunAsync(scenario);
        PortalAssertions.ContainsDetailsValidation(state, expectedText);
    }

    private static async Task<DetailsState> RunAsync(RegistrationDetailsScenario scenario)
    {
        TestOptions.RequireOtpJourneys();
        PlaywrightBrowserOptions options = TestOptions.FromEnvironment();
        PortalValidationFlow flow = new(options);
        EapiOtpCodeProvider otpCodeProvider = new();
        return await flow.ValidateRegistrationDetailsAsync(otpCodeProvider, scenario);
    }
}
