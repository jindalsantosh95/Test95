using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Playwright.Browser;
using VAA.EAPI.Automation.Trips.UI.Flows;
using VAA.EAPI.Automation.Trips.UI.Support;
using Xunit;

namespace VAA.EAPI.Automation.Trips.UI.Tests.Journeys;

[Trait("Category", "Trips-UI-Regression")]
public sealed class RequestAccessibleTravelJourneyTests
{
    [SkippableFact]
    [Trait("Feature", "Request accessible travel")]
    public async Task RequestAccessibleTravel_UsesAccessibleRequestControls()
    {
        string? baseUrl = Environment.GetEnvironmentVariable("TRIPS_UI_BASE_URL");
        Skip.If(baseUrl.IsNullOrWhiteSpace(), "Set TRIPS_UI_BASE_URL to run the Trips UI journey.");
        TripsUiTestData data = TripsUiTestData.FromStorage();
        await new RequestAccessibleTravelFlow(PlaywrightBrowserOptions.FromEnvironment(baseUrl!)).RunAsync(data, "Alex Example");
    }
}
