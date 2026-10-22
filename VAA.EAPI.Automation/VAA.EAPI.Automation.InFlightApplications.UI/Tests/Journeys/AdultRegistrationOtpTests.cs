using VAA.EAPI.Automation.Core.Playwright.Browser;
using VAA.EAPI.Automation.InFlightApplications.UI.Assertions;
using VAA.EAPI.Automation.InFlightApplications.UI.Flows;
using VAA.EAPI.Automation.InFlightApplications.UI.Infrastructure;
using VAA.EAPI.Automation.InFlightApplications.UI.Pages;

namespace VAA.EAPI.Automation.InFlightApplications.UI.Tests.Journeys;

[Collection(StatefulTestCollection.Name)]
public sealed class AdultRegistrationOtpTests
{
    [SkippableFact]
    [Trait("Category", "InFlightApplications-UI-OtpJourney")]
    [Trait("Feature", "Signup")]
    [Trait("Applicant", "Adult")]
    public async Task AdultApplicant_CompletesFlyingClubEnrolmentAndConnects()
    {
        TestOptions.RequireOtpJourneys();
        PlaywrightBrowserOptions options = TestOptions.FromEnvironment();
        PortalValidationFlow flow = new(options);
        EapiOtpCodeProvider otpCodeProvider = new();

        JourneyResult result = await flow.CompleteAdultEnrolmentAsync(otpCodeProvider);

        PortalAssertions.Connected(result);
    }

    [SkippableFact]
    [Trait("Category", "InFlightApplications-UI-OtpJourney")]
    [Trait("Feature", "SignupOtpValidation")]
    public async Task AdultApplicant_SeesOtpLengthAndValidationErrors()
    {
        TestOptions.RequireOtpJourneys();
        PlaywrightBrowserOptions options = TestOptions.FromEnvironment();
        PortalValidationFlow flow = new(options);

        OtpValidationResult result = await flow.ValidateAdultRegistrationOtpAsync();

        PortalAssertions.ShortOtpIsRejected(result);
    }

    [SkippableFact]
    [Trait("Category", "InFlightApplications-UI-OtpJourney")]
    [Trait("Feature", "SignupDetailsValidation")]
    public async Task AdultApplicant_ValidatesAddressAndEntersNonLookupAddress()
    {
        TestOptions.RequireOtpJourneys();
        PlaywrightBrowserOptions options = TestOptions.FromEnvironment();
        PortalValidationFlow flow = new(options);
        EapiOtpCodeProvider otpCodeProvider = new();

        DetailedAddressJourneyResult result = await flow.ValidateDetailedAddressJourneyAsync(otpCodeProvider);

        PortalAssertions.DetailedAddressValidationReachedPreferences(result);
    }
}
