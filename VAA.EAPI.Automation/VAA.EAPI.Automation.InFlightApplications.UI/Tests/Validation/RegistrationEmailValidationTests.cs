using VAA.EAPI.Automation.Core.Playwright.Browser;
using VAA.EAPI.Automation.InFlightApplications.UI.Assertions;
using VAA.EAPI.Automation.InFlightApplications.UI.Flows;
using VAA.EAPI.Automation.InFlightApplications.UI.Pages;

namespace VAA.EAPI.Automation.InFlightApplications.UI.Tests.Validation;

public sealed class RegistrationEmailValidationTests
{
    [SkippableFact]
    [Trait("Category", "InFlightApplications-UI-Validation")]
    [Trait("Feature", "Signup")]
    public async Task EmptyEmail_DisablesSubmit()
    {
        PortalValidationFlow flow = CreateFlow();

        FormState result = await flow.ValidateRegistrationEmailAsync(null);

        PortalAssertions.RegistrationEmailIsOpen(result);
        PortalAssertions.SubmitIs(result, false);
    }

    [SkippableFact]
    [Trait("Category", "InFlightApplications-UI-Validation")]
    [Trait("Feature", "Signup")]
    public async Task ValidEmail_EnablesSubmit()
    {
        PortalValidationFlow flow = CreateFlow();

        FormState result = await flow.ValidateRegistrationEmailAsync("user@example.com");

        PortalAssertions.RegistrationEmailIsOpen(result);
        PortalAssertions.SubmitIs(result, true);
    }

    [SkippableTheory]
    [Trait("Category", "InFlightApplications-UI-Validation")]
    [Trait("Feature", "Signup")]
    [InlineData("not-an-email")]
    [InlineData("user@@bad.com")]
    [InlineData("@nodomain.com")]
    public async Task InvalidEmail_ShowsFieldErrorAndDisablesSubmit(string email)
    {
        PortalValidationFlow flow = CreateFlow();

        FormState result = await flow.ValidateRegistrationEmailAsync(email);

        PortalAssertions.RegistrationEmailIsOpen(result);
        PortalAssertions.ContainsValidation(result, "Enter a valid email address");
        PortalAssertions.SubmitIs(result, false);
    }

    private static PortalValidationFlow CreateFlow()
    {
        PlaywrightBrowserOptions options = TestOptions.FromEnvironment();
        return new PortalValidationFlow(options);
    }
}
