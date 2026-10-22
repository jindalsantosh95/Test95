using VAA.EAPI.Automation.Core.Playwright.Browser;
using VAA.EAPI.Automation.InFlightApplications.UI.Assertions;
using VAA.EAPI.Automation.InFlightApplications.UI.Flows;
using VAA.EAPI.Automation.InFlightApplications.UI.Infrastructure;

namespace VAA.EAPI.Automation.InFlightApplications.UI.Tests.Journeys;

[Collection(StatefulTestCollection.Name)]
public sealed class PasswordResetJourneyTests
{
    [SkippableFact]
    [Trait("Category", "InFlightApplications-UI-AccountJourney")]
    [Trait("Feature", "PasswordReset")]
    public async Task ConfiguredAccount_VerifiesOtpAndValidatesNewPassword()
    {
        PortalAccountCredentials account = TestOptions.RequireAccount("E2E_LOGIN_EMAIL_MFA");
        PlaywrightBrowserOptions options = TestOptions.FromEnvironment();
        PortalValidationFlow flow = new(options);
        EapiOtpCodeProvider otpCodeProvider = new();

        PasswordResetJourneyResult result = await flow.ValidatePasswordResetJourneyAsync(
            account.Email,
            otpCodeProvider);

        PortalAssertions.PasswordResetRulesAreValidated(result);
    }
}
