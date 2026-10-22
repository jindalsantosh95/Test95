using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.UI.Helpers;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Infrastructure;

/// <summary>
/// Writes safe BrowserStack correlation information to xUnit output when a native journey fails.
/// It never prints raw request/response bodies because those can contain test journey data.
/// BrowserStack capture is enabled when the session starts, but evidence is retrieved only here.
/// </summary>
public sealed class MobileFailureEvidence
{
    private static readonly HttpClient BrowserStackApiClient = new()
    {
        Timeout = TimeSpan.FromSeconds(30),
    };
    private readonly IConfiguration _config;
    private readonly AppiumDriver _driver;
    private readonly MobileHealingTelemetry _healingTelemetry;
    private readonly ITestOutputHelper _output;
    private string _basketId = string.Empty;
    private readonly DateTimeOffset _startedAt = DateTimeOffset.UtcNow;

    public MobileFailureEvidence(
        AppiumDriver driver,
        IConfiguration config,
        ITestOutputHelper output,
        MobileHealingTelemetry healingTelemetry)
    {
        _driver = driver;
        _config = config;
        _output = output;
        _healingTelemetry = healingTelemetry;
    }

    /// <summary>
    /// Records a non-PII basket correlation identifier supplied by a future API-assisted flow.
    /// Native Appium interaction alone cannot reliably read the app's internal basket state.
    /// </summary>
    public void RecordBasketId(string basketId)
    {
        _basketId = basketId?.Trim() ?? string.Empty;
    }

    public void ReportFailure()
    {
        if (_config.GetValue<bool>("Diagnostics:SuppressSensitiveEvidence"))
        {
            _output.WriteLine("Sensitive-provider scenario: screenshots, hierarchy, app/network logs and raw errors are suppressed.");
            return;
        }
        DateTimeOffset failedAt = DateTimeOffset.UtcNow;
        _output.WriteLine($"Failure investigation window UTC: {_startedAt:O} to {failedAt:O} (before evidence collection).");
        _output.WriteLine("Timestamp matches are candidates only; backend cause requires matching request correlation and telemetry.");
        TryCaptureFullScreenScreenshot();
        TryCaptureErrorScreenshots();
        TryCaptureIosLogs();
        TryCaptureNativeSnapshot();
        string sessionId = _driver.SessionId?.ToString() ?? "unavailable";
        bool remote = (_config["RunMode"] ?? "Local").Is("Remote");
        string basketId = _basketId.IsNullOrWhiteSpace()
            ? "not captured by the native UI journey"
            : _basketId;
        bool networkLogs = _config.GetValue("BrowserStack:NetworkLogs", false);
        bool captureNetworkContent = _config.GetValue("BrowserStack:CaptureNetworkContent", false);
        bool canCaptureNetworkContent = networkLogs && captureNetworkContent;

        _output.WriteLine(remote
            ? "=== Mobile BrowserStack failure evidence ==="
            : "=== Local Appium failure evidence ===");
        _output.WriteLine($"Session ID: {sessionId}");
        _output.WriteLine($"Bounded healing events before failure: {_healingTelemetry.Snapshot().Count}");
        if (!remote)
        {
            _output.WriteLine($"Platform: {ReadConfig("Platform")}");
            _output.WriteLine($"Device: {ReadConfig($"Appium:{_config["Platform"]}:DeviceName")}");
            _output.WriteLine($"Server: {ReadConfig($"Appium:{_config["Platform"]}:ServerUrl")}");
            _output.WriteLine($"Basket ID: {basketId}");
            _output.WriteLine(
                "iOS app logs are diagnostic signals, not raw HTTP capture or proof of backend cause. " +
                "BrowserStack network response capture is a separate opt-in diagnostic source.");
            return;
        }

        _output.WriteLine($"Project: {ReadConfig("BrowserStack:Project")}");
        _output.WriteLine($"Build: {ReadConfig("BrowserStack:Build")}");
        _output.WriteLine($"Session name: {ReadConfig("BrowserStack:Name")}");
        _output.WriteLine($"App: {ReadConfig("BrowserStack:App")}");
        _output.WriteLine($"Profile: {ReadConfig("BrowserStack:Profile")}");
        _output.WriteLine($"Basket ID: {basketId}");
        _output.WriteLine(
            canCaptureNetworkContent
                ? "GraphQL response capture: enabled; retrieving a sanitised failure summary."
                : "GraphQL response capture: disabled; enable BrowserStack network logs and content capture before the session starts.");
        _output.WriteLine(
            "Do not add raw GraphQL response bodies to xUnit output; use the BrowserStack session for targeted diagnosis.");

        if (canCaptureNetworkContent && !sessionId.Is("unavailable"))
        {
            TryReportSanitisedNetworkFailures(sessionId);
        }

        TryAnnotate(sessionId, basketId, canCaptureNetworkContent);
    }

    private void TryCaptureIosLogs()
    {
        if ((_config["RunMode"] ?? "Local").Is("Remote") || !(_config["Platform"] ?? "").Is("iOS") ||
            !_config.GetValue("Diagnostics:IosAppLogsOnFailure", true))
        {
            return;
        }
        try
        {
            IReadOnlyList<string> summaries = IosAppLogEvidence.CollectAsync(
                _config["Appium:iOS:ServerUrl"] ?? string.Empty,
                _driver.SessionId?.ToString() ?? string.Empty,
                _config["Diagnostics:IosAppProcessName"] ?? "vaa-mobile-ios", _startedAt).GetAwaiter().GetResult();
            string directory = Path.Combine(AppContext.BaseDirectory, "TestResults", "ios-app-logs", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, "app-log-summary.txt");
            string header = "Local iOS app log signals only; raw messages/bodies omitted. " +
                "Last 5 minutes within this test; at most 2000 records inspected and 40 summaries retained.";
            string[] lines = new[] { header }.Concat(summaries.Count == 0
                ? new[] { "No matching safe app-log metadata found. This does not prove absence of app/backend errors." }
                : summaries).ToArray();
            File.WriteAllLines(path, lines);
            _output.WriteLine($"Sanitised iOS app logs before teardown: {path}");
            foreach (string line in lines)
            {
                _output.WriteLine(line);
            }
        }
        catch (Exception exception)
        {
            _output.WriteLine($"iOS app logs unavailable ({exception.GetType().Name}); original failure retained. No raw logs saved.");
        }
    }

    private void TryCaptureFullScreenScreenshot()
    {
        try
        {
            if (!_config.GetValue("Diagnostics:FullScreenScreenshotOnFailure", false))
                return;

            // Explicit opt-in: this image is not redacted and may contain sensitive test data.
            string directory = Path.Combine(AppContext.BaseDirectory, "TestResults", "mobile-failure-screenshots", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, "screen.png");
            _driver.GetScreenshot().SaveAsFile(path);
            _output.WriteLine($"Failure full-screen screenshot (unredacted; review before sharing): {path}");
        }
        catch (Exception exception)
        {
            _output.WriteLine($"Full-screen failure screenshot unavailable ({exception.GetType().Name}); original failure retained.");
        }
    }

    private void TryCaptureErrorScreenshots()
    {
        if (!_config.GetValue("Diagnostics:ErrorControlScreenshotsOnFailure", false))
            return;
        try
        {
            if (!(_config["Platform"] ?? "").Is("iOS") || !_driver.Context.Is("NATIVE_APP"))
            {
                _output.WriteLine("Failure screenshots excluded: only approved native iOS error controls are supported.");
                return;
            }
            string[] identifiers = [NativeBookingAccessibilityIds.SeatSelectionError,
                NativeBookingAccessibilityIds.PaymentRetryButton];
            int captured = 0;
            foreach (string identifier in identifiers)
            {
                foreach (IWebElement element in _driver.FindElements(MobileBy.AccessibilityId(identifier)).Take(8))
                {
                    // Crop only leaf error controls. Never fall back to a full-screen image.
                    if (!element.Displayed || !CanCaptureErrorControl(identifier, element.GetAttribute("type")) ||
                        element is not ITakesScreenshot screenshot)
                        continue;
                    string directory = Path.Combine(AppContext.BaseDirectory, "TestResults", "mobile-error-screenshots", Guid.NewGuid().ToString("N"));
                    Directory.CreateDirectory(directory);
                    string path = Path.Combine(directory, "error.png");
                    screenshot.GetScreenshot().SaveAsFile(path);
                    captured++;
                    _output.WriteLine($"Failure error-control screenshot ({identifier}): {path}");
                }
            }
            if (captured == 0)
                _output.WriteLine("Failure screenshots excluded: no approved visible error control. No full-screen fallback.");
        }
        catch (Exception exception)
        {
            _output.WriteLine($"Failure screenshot unavailable ({exception.GetType().Name}); original failure retained.");
        }
    }

    internal static bool CanCaptureErrorControl(string identifier, string? type)
        => (identifier == NativeBookingAccessibilityIds.SeatSelectionError ||
            identifier == NativeBookingAccessibilityIds.PaymentRetryButton) &&
            (type == "XCUIElementTypeStaticText" || type == "XCUIElementTypeButton");

    private void TryCaptureNativeSnapshot()
    {
        try
        {
            System.Xml.Linq.XElement snapshot = NativeUiFailureSnapshot.Sanitise(_driver.PageSource);
            string directory = Path.Combine(AppContext.BaseDirectory, "TestResults", "native-ui-failures", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, "ui-state.xml");
            snapshot.Save(path);
            _output.WriteLine($"Sanitised native UI state before teardown: {path}. Labels, values and screenshots omitted.");
        }
        catch (Exception exception)
        {
            // Evidence must never replace the original failure or leak a driver's raw payload.
            _output.WriteLine($"Native UI state capture unavailable ({exception.GetType().Name}); original failure retained.");
        }
    }

    private void TryReportSanitisedNetworkFailures(string sessionId)
    {
        try
        {
            BrowserStackNetworkLogClient client = new(BrowserStackApiClient, _config);
            IReadOnlyList<string> summaries = client
                .GetSafeFailureSummariesAsync(sessionId)
                .GetAwaiter()
                .GetResult();
            if (summaries.Count == 0)
            {
                _output.WriteLine("Sanitised BrowserStack network summary: no failing GraphQL entries were available.");
                return;
            }

            _output.WriteLine("Sanitised BrowserStack network failures:");
            foreach (string summary in summaries)
            {
                _output.WriteLine($"- {summary}");
            }
        }
        catch (Exception)
        {
            _output.WriteLine("Sanitised BrowserStack network summary could not be retrieved.");
        }
    }

    private string ReadConfig(string key)
    {
        string? value = _config[key];
        return value.IsNullOrWhiteSpace() ? "unavailable" : value ?? "unavailable";
    }

    private void TryAnnotate(string sessionId, string basketId, bool captureNetworkContent)
    {
        try
        {
            BrowserStackLogHelper.Annotate(
                _driver,
                $"Failure evidence. Session={sessionId}; Basket={basketId}; " +
                $"GraphQLContentCapture={captureNetworkContent}; " +
                $"HealingEvents={_healingTelemetry.Snapshot().Count}.");
        }
        catch (Exception exception)
        {
            _output.WriteLine($"BrowserStack failure annotation was unavailable: {exception.Message}");
        }
    }
}
