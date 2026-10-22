using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Playwright.Browser;
using VAA.EAPI.Automation.Trips.UI.Flows;
using VAA.EAPI.Automation.Trips.UI.Support;
using Xunit;

namespace VAA.EAPI.Automation.Trips.UI.Tests.Journeys;

[Trait("Category", "Trips-UI-Regression")]
public sealed class LoyaltyJourneyTests
{
    [SkippableFact]
    [Trait("Feature", "Loyalty number")]
    public async Task ExistingLoyaltyNumber_IsDisplayedOnPassengerCard()
    {
        string? baseUrl = Environment.GetEnvironmentVariable("TRIPS_UI_BASE_URL");
        Skip.If(baseUrl.IsNullOrWhiteSpace(), "Set TRIPS_UI_BASE_URL to run the Trips UI journey.");
        TripsUiTestData data = TripsUiTestData.FromStorage();
        await new LoyaltyJourneyFlow(PlaywrightBrowserOptions.FromEnvironment(baseUrl!)).VerifyExistingNumberAsync(data, "Alex Example");
    }
}
