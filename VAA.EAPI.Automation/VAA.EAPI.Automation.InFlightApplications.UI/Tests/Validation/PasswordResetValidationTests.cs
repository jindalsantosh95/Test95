using VAA.EAPI.Automation.Core.Playwright.Browser;
using VAA.EAPI.Automation.InFlightApplications.UI.Assertions;
using VAA.EAPI.Automation.InFlightApplications.UI.Flows;
using VAA.EAPI.Automation.InFlightApplications.UI.Pages;

namespace VAA.EAPI.Automation.InFlightApplications.UI.Tests.Validation;

public sealed class PasswordResetValidationTests
{
    [SkippableFact]
    [Trait("Category", "InFlightApplications-UI-Validation")]
    [Trait("Feature", "PasswordReset")]
    public async Task EmptyForm_DisablesSubmit()
    {
        PortalValidationFlow flow = CreateFlow();

        FormState result = await flow.ValidatePasswordResetAsync(null);

        PortalAssertions.SubmitIs(result, false);
    }

    [SkippableTheory]
    [Trait("Category", "InFlightApplications-UI-Validation")]
    [Trait("Feature", "PasswordReset")]
    [InlineData("user@example.com")]
    [InlineData("1234567")]
    public async Task ValidIdentifier_EnablesSubmit(string identifier)
    {
        PortalValidationFlow flow = CreateFlow();

        FormState result = await flow.ValidatePasswordResetAsync(identifier);

        PortalAssertions.SubmitIs(result, true);
    }

    [SkippableTheory]
    [Trait("Category", "InFlightApplications-UI-Validation")]
    [Trait("Feature", "PasswordReset")]
    [InlineData("not-an-email")]
    [InlineData("user@@bad.com")]
    [InlineData("12345678901")]
    public async Task InvalidIdentifier_ShowsFieldErrorAndDisablesSubmit(string identifier)
    {
        PortalValidationFlow flow = CreateFlow();

        FormState result = await flow.ValidatePasswordResetAsync(identifier);

        PortalAssertions.ContainsValidation(
            result,
            "Email or Flying Club number contains invalid characters");
        PortalAssertions.SubmitIs(result, false);
    }

    private static PortalValidationFlow CreateFlow()
    {
        PlaywrightBrowserOptions options = TestOptions.FromEnvironment();
        return new PortalValidationFlow(options);
    }
}
