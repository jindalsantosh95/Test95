using VAA.EAPI.Automation.Core.Playwright.Browser;
using VAA.EAPI.Automation.InFlightApplications.UI.Assertions;
using VAA.EAPI.Automation.InFlightApplications.UI.Flows;
using VAA.EAPI.Automation.InFlightApplications.UI.Infrastructure;

namespace VAA.EAPI.Automation.InFlightApplications.UI.Tests.Journeys;

[Collection(StatefulTestCollection.Name)]
public sealed class LoginJourneyTests
{
    [SkippableTheory]
    [InlineData("E2E_LOGIN_EMAIL_MFA", true)]
    [InlineData("E2E_LOGIN_EMAIL_NO_MFA", false)]
    [Trait("Category", "InFlightApplications-UI-AccountJourney")]
    [Trait("Feature", "Login")]
    public async Task ConfiguredAccount_SignsInAndReachesConnection(string emailVariable, bool requiresMfa)
    {
        PortalAccountCredentials account = TestOptions.RequireAccount(emailVariable);
        PlaywrightBrowserOptions options = TestOptions.FromEnvironment();
        PortalValidationFlow flow = new(options);
        EapiOtpCodeProvider otpCodeProvider = new();

        AuthenticatedJourneyResult result = await flow.CompleteLoginJourneyAsync(
            account.Email,
            account.Password,
            requiresMfa,
            otpCodeProvider);

        PortalAssertions.AuthenticatedAndAccessible(result);
    }
}
