using VAA.EAPI.Automation.Core.Playwright.Browser;
using VAA.EAPI.Automation.InFlightApplications.UI.Assertions;
using VAA.EAPI.Automation.InFlightApplications.UI.Flows;
using VAA.EAPI.Automation.InFlightApplications.UI.Pages;

namespace VAA.EAPI.Automation.InFlightApplications.UI.Tests.Validation;

public sealed class RegistrationProfileValidationTests
{
    [SkippableFact]
    [Trait("Category", "InFlightApplications-UI-Validation")]
    [Trait("Feature", "Signup")]
    public async Task EmptyForm_DisablesSubmit()
    {
        PortalValidationFlow flow = CreateFlow();

        FormState result = await flow.ValidateRegistrationProfileAsync();

        PortalAssertions.SubmitIs(result, false);
    }

    [SkippableTheory]
    [Trait("Category", "InFlightApplications-UI-Validation")]
    [Trait("Feature", "Signup")]
    [InlineData(30, "M", "Male")]
    [InlineData(30, "F", "Female")]
    [InlineData(10, "X", "Unspecified (x)")]
    [InlineData(10, "U", "Undisclosed (u)")]
    public async Task CompleteProfile_EnablesSubmit(int age, string gender, string expectedGenderLabel)
    {
        PortalValidationFlow flow = CreateFlow();

        FormState result = await flow.ValidateRegistrationProfileAsync(
            "Alex",
            "Brown",
            DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-age),
            gender);

        PortalAssertions.SubmitIs(result, true);
        PortalAssertions.SelectedGenderIs(result, expectedGenderLabel);
    }

    [SkippableTheory]
    [Trait("Category", "InFlightApplications-UI-Validation")]
    [Trait("Feature", "Signup")]
    [InlineData("A", "First name must be at least 2 characters")]
    [InlineData("John1", "First name contains invalid characters")]
    [InlineData("ThisIsTooLongName", "First name must be less than 15 characters")]
    public async Task InvalidFirstName_ShowsFieldError(string firstName, string expectedError)
    {
        PortalValidationFlow flow = CreateFlow();

        FormState result = await flow.ValidateRegistrationProfileAsync(
            firstName,
            "Smith",
            DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-30));

        PortalAssertions.ContainsValidation(result, expectedError);
        PortalAssertions.SubmitIs(result, false);
    }

    [SkippableTheory]
    [Trait("Category", "InFlightApplications-UI-Validation")]
    [Trait("Feature", "Signup")]
    [InlineData("A", "Last name must be at least 2 characters")]
    [InlineData("Smith9", "Last name contains invalid characters")]
    [InlineData("ThisIsAVeryLongLastName", "Last name must be less than 20 characters")]
    public async Task InvalidLastName_ShowsFieldError(string lastName, string expectedError)
    {
        PortalValidationFlow flow = CreateFlow();

        FormState result = await flow.ValidateRegistrationProfileAsync(
            "John",
            lastName,
            DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-30));

        PortalAssertions.ContainsValidation(result, expectedError);
        PortalAssertions.SubmitIs(result, false);
    }

    [SkippableFact]
    [Trait("Category", "InFlightApplications-UI-Validation")]
    [Trait("Feature", "Signup")]
    public async Task FutureDateOfBirth_ShowsValidationError()
    {
        PortalValidationFlow flow = CreateFlow();

        FormState result = await flow.ValidateRegistrationProfileAsync(
            "John",
            "Smith",
            DateOnly.FromDateTime(DateTime.UtcNow).AddYears(70));

        PortalAssertions.ContainsValidation(result, "Enter a valid date");
        PortalAssertions.SubmitIs(result, false);
    }

    private static PortalValidationFlow CreateFlow()
    {
        PlaywrightBrowserOptions options = TestOptions.FromEnvironment();
        return new PortalValidationFlow(options);
    }
}
