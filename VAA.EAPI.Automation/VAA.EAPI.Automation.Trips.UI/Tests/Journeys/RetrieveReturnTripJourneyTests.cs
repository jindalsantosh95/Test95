using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Playwright.Browser;
using VAA.EAPI.Automation.Trips.UI.Flows;
using VAA.EAPI.Automation.Trips.UI.Support;
using Xunit;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.Trips.UI.Tests.Journeys;

[Trait("Category", "Trips-UI-Regression")]
public sealed class RetrieveReturnTripJourneyTests(ITestOutputHelper output)
{
    [SkippableFact]
    [Trait("Feature", "Return trip retrieval")]
    [Trait("Platform", "Web")]
    public async Task AddTrip_WithStorageReturnPnr_DisplaysBothJourneys()
    {
        string? baseUrl = Environment.GetEnvironmentVariable("TRIPS_UI_BASE_URL");
        Skip.If(baseUrl.IsNullOrWhiteSpace(), "Set TRIPS_UI_BASE_URL to run the Trips UI journey.");

        TripsUiTestData data = TripsUiTestData.FromStorage("Return");
        PlaywrightBrowserOptions options = PlaywrightBrowserOptions.FromEnvironment(baseUrl!);
        RetrievedTrip result = await new RetrieveTripFlow(options).RunReturnAsync(data);

        Assert.Equal(data.Pnr, result.BookingReference);
        output.WriteLine($"Validated return PNR: {data.Pnr}");
    }
}
