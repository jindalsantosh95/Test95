using VAA.EAPI.Automation.Core.Playwright.Accessibility;
using VAA.EAPI.Automation.Core.Playwright.Browser;
using VAA.EAPI.Automation.InFlightApplications.UI.Assertions;
using VAA.EAPI.Automation.InFlightApplications.UI.Flows;
using VAA.EAPI.Automation.InFlightApplications.UI.Infrastructure;

namespace VAA.EAPI.Automation.InFlightApplications.UI.Tests.Accessibility;

[Collection(StatefulTestCollection.Name)]
public sealed class PortalAccessibilityTests
{
    public static TheoryData<string> ReachablePortalPages => new()
    {
        "/",
        "/register",
        "/register/under-18s",
        "/forgotten-password",
        "/login/code",
        "/starlink-captive-portal/",
    };

    public static TheoryData<AdultRegistrationStep> GuardedRegistrationPages => new()
    {
        AdultRegistrationStep.Email,
        AdultRegistrationStep.Details,
        AdultRegistrationStep.Preferences,
        AdultRegistrationStep.Password,
    };

    [SkippableTheory]
    [MemberData(nameof(ReachablePortalPages))]
    [Trait("Category", "InFlightApplications-UI-Accessibility")]
    [Trait("Standard", "WCAG22AA")]
    public async Task ReachablePage_HasNoSeriousOrCriticalAccessibilityViolations(string relativePath)
    {
        PlaywrightBrowserOptions options = TestOptions.FromEnvironment();
        PortalValidationFlow flow = new(options);

        AccessibilityScanResult result = await flow.ScanAccessibilityAsync(relativePath);

        PortalAssertions.HasNoSeriousOrCriticalAccessibilityViolations(result);
    }

    [SkippableTheory]
    [MemberData(nameof(GuardedRegistrationPages))]
    [Trait("Category", "InFlightApplications-UI-OtpJourney")]
    [Trait("Feature", "Accessibility")]
    [Trait("Standard", "WCAG22AA")]
    public async Task GuardedRegistrationPage_HasNoSeriousOrCriticalAccessibilityViolations(
        AdultRegistrationStep step)
    {
        TestOptions.RequireOtpJourneys();
        PlaywrightBrowserOptions options = TestOptions.FromEnvironment();
        PortalValidationFlow flow = new(options);
        EapiOtpCodeProvider provider = new();

        AccessibilityScanResult result = await flow.ScanAdultRegistrationStepAsync(provider, step);

        PortalAssertions.HasNoSeriousOrCriticalAccessibilityViolations(result);
    }
}
