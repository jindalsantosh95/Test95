using VAA.EAPI.Automation.Core.Playwright.Browser;
using VAA.EAPI.Automation.InFlightApplications.UI.Assertions;
using VAA.EAPI.Automation.InFlightApplications.UI.Flows;
using VAA.EAPI.Automation.InFlightApplications.UI.Pages;

namespace VAA.EAPI.Automation.InFlightApplications.UI.Tests.Validation;

public sealed class LoginValidationTests
{
    [SkippableTheory]
    [Trait("Category", "InFlightApplications-UI-Validation")]
    [Trait("Feature", "Login")]
    [InlineData("user@example.com")]
    [InlineData("1234567")]
    public async Task ValidIdentifierAndPassword_EnableSubmit(string identifier)
    {
        PortalValidationFlow flow = CreateFlow();

        FormState result = await flow.ValidateLoginAsync(identifier, "Password123", false);

        PortalAssertions.SubmitIs(result, true);
    }

    [SkippableTheory]
    [Trait("Category", "InFlightApplications-UI-Validation")]
    [Trait("Feature", "Login")]
    [InlineData("not-an-email")]
    [InlineData("user@@bad.com")]
    [InlineData("12345678901")]
    public async Task InvalidIdentifier_ShowsFieldError(string identifier)
    {
        PortalValidationFlow flow = CreateFlow();

        FormState result = await flow.ValidateLoginAsync(identifier, "Password123", false);

        PortalAssertions.ContainsValidation(
            result,
            "Email or Flying Club number contains invalid characters");
    }

    [SkippableFact]
    [Trait("Category", "InFlightApplications-UI-Validation")]
    [Trait("Feature", "Login")]
    public async Task UnknownCredentials_KeepUserOnLoginForm()
    {
        PortalValidationFlow flow = CreateFlow();

        FormState result = await flow.ValidateLoginAsync(
            "nonexistent-e2e-user@example.com",
            "WrongPassword123",
            true);

        PortalAssertions.RemainsOnLogin(result);
    }

    private static PortalValidationFlow CreateFlow()
    {
        PlaywrightBrowserOptions options = TestOptions.FromEnvironment();
        return new PortalValidationFlow(options);
    }
}
