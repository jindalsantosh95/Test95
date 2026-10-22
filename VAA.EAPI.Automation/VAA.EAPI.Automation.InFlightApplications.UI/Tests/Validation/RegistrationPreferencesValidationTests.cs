using VAA.EAPI.Automation.Core.Playwright.Browser;
using VAA.EAPI.Automation.InFlightApplications.UI.Assertions;
using VAA.EAPI.Automation.InFlightApplications.UI.Flows;
using VAA.EAPI.Automation.InFlightApplications.UI.Infrastructure;
using VAA.EAPI.Automation.InFlightApplications.UI.Pages;

namespace VAA.EAPI.Automation.InFlightApplications.UI.Tests.Validation;

[Collection(StatefulTestCollection.Name)]
public sealed class RegistrationPreferencesValidationTests
{
    [SkippableFact]
    [Trait("Category", "InFlightApplications-UI-OtpJourney")]
    public async Task Preferences_DefaultToFlyingClub()
    {
        (PortalValidationFlow flow, EapiOtpCodeProvider provider) = CreateFlow();
        PreferencesState state = await flow.ValidateRegistrationPreferencesAsync(provider, choosePartner: false);
        PortalAssertions.DefaultsToFlyingClub(state);
    }

    [SkippableFact]
    [Trait("Category", "InFlightApplications-UI-OtpJourney")]
    public async Task PartnerPreference_RequiresAirlineProgramme()
    {
        (PortalValidationFlow flow, EapiOtpCodeProvider provider) = CreateFlow();
        PartnerPreferenceResult result = await flow.SelectPartnerProgrammeAsync(provider);
        PortalAssertions.PartnerProgrammeIsRequired(result.BeforeSelection, result.AfterSelection);
    }

    private static (PortalValidationFlow Flow, EapiOtpCodeProvider Provider) CreateFlow()
    {
        TestOptions.RequireOtpJourneys();
        PlaywrightBrowserOptions options = TestOptions.FromEnvironment();
        return (new PortalValidationFlow(options), new EapiOtpCodeProvider());
    }
}
