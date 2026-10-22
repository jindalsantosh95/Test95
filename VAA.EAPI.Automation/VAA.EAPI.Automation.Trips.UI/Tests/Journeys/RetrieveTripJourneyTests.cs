using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Playwright.Browser;
using VAA.EAPI.Automation.Trips.UI.Flows;
using VAA.EAPI.Automation.Trips.UI.Support;
using Xunit;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.Trips.UI.Tests.Journeys;

[Trait("Category", "Trips-UI-Regression")]
public sealed class RetrieveTripJourneyTests(ITestOutputHelper output)
{
    [SkippableFact]
    [Trait("Feature", "Trip retrieval")]
    [Trait("Platform", "Web")]
    public async Task AddTrip_WithStoragePnr_DisplaysTheMatchingTrip()
    {
        string baseUrl = GetBaseUrlOrSkip();

        // Arrange: select a valid current-week booking created for the mobile/My Trips flow.
        TripsUiTestData data = TripsUiTestData.FromStorage();
        PlaywrightBrowserOptions options = PlaywrightBrowserOptions.FromEnvironment(baseUrl);
        RetrievedTrip result = await new RetrieveTripFlow(options).RunAsync(data);

        Assert.Equal(data.Pnr, result.BookingReference);
        output.WriteLine($"Validated PNR: {data.Pnr}");
    }

    private static string GetBaseUrlOrSkip()
    {
        string? baseUrl = Environment.GetEnvironmentVariable("TRIPS_UI_BASE_URL");
        Skip.If(baseUrl.IsNullOrWhiteSpace(), "Set TRIPS_UI_BASE_URL to run the Trips UI journey.");
        return baseUrl!;
    }
}
