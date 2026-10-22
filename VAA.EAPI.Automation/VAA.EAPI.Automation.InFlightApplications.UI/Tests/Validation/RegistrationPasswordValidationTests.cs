using VAA.EAPI.Automation.Core.Playwright.Browser;
using VAA.EAPI.Automation.InFlightApplications.UI.Assertions;
using VAA.EAPI.Automation.InFlightApplications.UI.Flows;
using VAA.EAPI.Automation.InFlightApplications.UI.Infrastructure;
using VAA.EAPI.Automation.InFlightApplications.UI.Pages;

namespace VAA.EAPI.Automation.InFlightApplications.UI.Tests.Validation;

[Collection(StatefulTestCollection.Name)]
public sealed class RegistrationPasswordValidationTests
{
    [SkippableFact]
    [Trait("Category", "InFlightApplications-UI-OtpJourney")]
    public async Task MismatchedPasswords_DisableSubmit()
    {
        RegistrationPasswordState state = await RunAsync(enterMismatchedPasswords: true);
        PortalAssertions.PasswordsDoNotMatch(state);
    }

    [SkippableFact]
    [Trait("Category", "InFlightApplications-UI-OtpJourney")]
    public async Task PasswordPage_ShowsTermsAndUnselectedMarketingPreferences()
    {
        RegistrationPasswordState state = await RunAsync(enterMismatchedPasswords: false);
        PortalAssertions.PasswordPageHasTermsAndDefaultPreferences(state);
    }

    private static async Task<RegistrationPasswordState> RunAsync(bool enterMismatchedPasswords)
    {
        TestOptions.RequireOtpJourneys();
        PlaywrightBrowserOptions options = TestOptions.FromEnvironment();
        PortalValidationFlow flow = new(options);
        EapiOtpCodeProvider provider = new();
        return await flow.ValidateRegistrationPasswordAsync(provider, enterMismatchedPasswords);
    }
}
