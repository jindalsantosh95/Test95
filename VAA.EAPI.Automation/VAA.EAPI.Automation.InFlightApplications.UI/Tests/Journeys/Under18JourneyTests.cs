using VAA.EAPI.Automation.Core.Playwright.Browser;
using VAA.EAPI.Automation.InFlightApplications.UI.Assertions;
using VAA.EAPI.Automation.InFlightApplications.UI.Flows;
using VAA.EAPI.Automation.InFlightApplications.UI.Pages;

namespace VAA.EAPI.Automation.InFlightApplications.UI.Tests.Journeys;

public sealed class Under18JourneyTests
{
    [SkippableFact]
    [Trait("Category", "InFlightApplications-UI-Journey")]
    [Trait("Feature", "Signup")]
    [Trait("Applicant", "Under18")]
    public async Task Under18Applicant_ReachesConnectionPage()
    {
        PlaywrightBrowserOptions options = TestOptions.FromEnvironment();
        PortalValidationFlow flow = new(options);

        JourneyResult result = await flow.CompleteUnder18JourneyAsync();

        PortalAssertions.Connected(result);
    }
}
