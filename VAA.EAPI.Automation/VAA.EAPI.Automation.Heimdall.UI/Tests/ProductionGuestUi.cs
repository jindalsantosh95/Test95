using VAA.EAPI.Automation.Core.Constants;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Playwright.Browser;
using VAA.EAPI.Automation.SearchAndBook.UI.Flows;
using VAA.EAPI.Automation.SearchAndBook.UI.Support;
using Xunit;

namespace VAA.EAPI.Automation.Heimdall.UI.Tests;

public sealed class ProductionGuestUi
{
    private const string HeimdallRunnerEnvironmentVariable = "VAA_HEIMDALL_RUNNER";

    [SkippableFact]
    [Trait("Category", GlobalTraits.HealthCheck)]
    [Trait("Tag", GlobalTraits.DotCom)]
    public async Task GuestUpperClass_SelectsIncludedSeatAndReachesHostedPaymentPage()
    {
        await RunToHostedPaymentPageAsync(
            "Production guest Upper Class included-seat monitor",
            "Upper Class",
            SeatSelectionMode.SelectAvailable,
            requireSeatMap: true);
    }

    [SkippableFact]
    [Trait("Category", GlobalTraits.HealthCheck)]
    [Trait("Tag", GlobalTraits.DotCom)]
    public async Task GuestEconomyLight_ContinuesWithoutSeatAndReachesHostedPaymentPage()
    {
        await RunToHostedPaymentPageAsync(
            "Production guest Economy Light no-seat monitor",
            "Economy Light",
            SeatSelectionMode.Skip,
            requireSeatMap: false);
    }

    private static async Task RunToHostedPaymentPageAsync(
        string name,
        string fareName,
        SeatSelectionMode seatSelection,
        bool requireSeatMap)
    {
        string? baseUrl = System.Environment.GetEnvironmentVariable("SAB_BASE_URL");
        if (baseUrl.IsNullOrWhiteSpace())
        {
            bool runningInHeimdall = System.Environment
                .GetEnvironmentVariable(HeimdallRunnerEnvironmentVariable)
                .Is("true");
            Skip.IfNot(
                runningInHeimdall,
                "Production Heimdall UI monitors require SAB_BASE_URL and are skipped during aggregate/local test runs.");

            throw new InvalidOperationException(
                "SAB_BASE_URL must be configured on the Heimdall agent for Search & Book UI health checks.");
        }

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out Uri? productionBaseUrl) ||
            productionBaseUrl.Scheme != Uri.UriSchemeHttps ||
            !ProductionHosts.Contains(productionBaseUrl.Host))
        {
            throw new InvalidOperationException(
                "SAB_BASE_URL must target the HTTPS production Virgin Atlantic website for Heimdall UI health checks.");
        }

        PlaywrightBrowserOptions options = PlaywrightBrowserOptions.FromEnvironment(productionBaseUrl.AbsoluteUri);
        options = options with
        {
            // HPP URLs contain payment reference identifiers. Keep tracing disabled until
            // trace URL redaction exists; retain the safer failure diagnostics only.
            TraceEnabled = false,
            RetainPassedArtifacts = false,
            DisableWebSecurity = false,
            MaskSensitiveControlsInFailureScreenshots = true
        };
        await new GuestJourneyFlow(options).RunAsync(new GuestJourneyScenario
        {
            Name = name,
            FareName = fareName,
            SeatSelection = seatSelection,
            RequireSeatMap = requireSeatMap,
            StopAtHostedPaymentPage = true
        });
    }

    private static readonly HashSet<string> ProductionHosts = new(StringComparer.OrdinalIgnoreCase)
    {
        "www.virginatlantic.com",
        "virginatlantic.com"
    };
}
