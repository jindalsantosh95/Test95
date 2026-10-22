using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.UI.Helpers;
using VAA.EAPI.Automation.MobileApp.UI.Flows;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Infrastructure;

public abstract class SearchBookTestBase : IAsyncLifetime
{
    private MobileFailureEvidence? _failureEvidence;
    private MobileHealingTelemetry? _healingTelemetry;
    private int _reportedHealingEventCount;
    private (string SessionId, string Status, string Reason)? _pendingStatus;

    protected SearchBookTestBase(
        ITestOutputHelper output,
        IReadOnlyDictionary<string, string?>? configurationOverrides = null)
    {
        Output = output;
        Fixture = new SearchBookMobileFixture(configurationOverrides);
    }

    protected SearchBookMobileFixture Fixture { get; }

    protected ITestOutputHelper Output { get; }

    protected void ReportSuccessfulBooking(
        CompletedNativeBooking booking,
        string reason,
        [CallerMemberName] string testName = "")
    {
        Output.WriteLine(ConfirmedBookingReport.Format(booking.BookingReference, testName));
        TrySetBrowserStackStatus("passed", reason, testName);
    }

    public virtual async Task InitializeAsync()
    {
        await Fixture.InitializeAsync();
        _healingTelemetry = MobileHealingTelemetryRegistry.Attach(Fixture.Driver);
        _failureEvidence = new MobileFailureEvidence(
            Fixture.Driver,
            Fixture.Config,
            Output,
            _healingTelemetry);
    }

    public virtual async Task DisposeAsync()
    {
        ReportNewHealingEvents();
        if (_healingTelemetry is not null)
        {
            MobileHealingTelemetryRegistry.Detach(Fixture.Driver);
        }

        try
        {
            await Fixture.DisposeAsync();
        }
        finally
        {
            if (_pendingStatus is { } pending)
            {
                try
                {
                    using HttpClient client = new(new HttpClientHandler { AllowAutoRedirect = false });
                    bool updated = await new BrowserStackSessionStatusClient(client, Fixture.Config)
                        .UpdateAsync(pending.SessionId, pending.Status, pending.Reason);
                    Output.WriteLine(updated
                        ? $"BrowserStack REST fallback marked session {pending.Status}."
                        : "BrowserStack REST status fallback unavailable; use the original TRX result.");
                }
                catch (Exception)
                {
                    // Reporting cannot replace the original test/teardown exception.
                    Output.WriteLine("BrowserStack REST status fallback unavailable; details suppressed. Use the original TRX result.");
                }
            }
        }
    }

    protected void TrySetBrowserStackStatus(
        string status,
        string reason,
        [CallerMemberName] string testName = "")
    {
        if (status.Is("failed"))
        {
            _failureEvidence?.ReportFailure();
        }

        ReportNewHealingEvents();

        if (!(Fixture.Config["RunMode"] ?? "Local").Is("Remote"))
        {
            Output.WriteLine($"Local Appium test {status}: {reason}");
            return;
        }

        try
        {
            string sessionName = $"{GetType().Name} | {testName}";
            BrowserStackStatusHelper.SetSessionStatus(Fixture.Driver, status, reason);
            _pendingStatus = null;
            Output.WriteLine($"BrowserStack session '{sessionName}' marked {status}: {reason}");
        }
        catch (Exception)
        {
            _pendingStatus = (Fixture.Driver.SessionId.ToString(), status, reason);
            Output.WriteLine("BrowserStack executor status update unavailable; one bounded REST fallback will run after teardown.");
        }
        // A naming error must never prevent reporting pass/fail.
        TrySetBrowserStackSessionName(testName);
    }

    protected void TrySetBrowserStackSessionName([CallerMemberName] string testName = "")
    {
        if (!(Fixture.Config["RunMode"] ?? "Local").Is("Remote"))
        {
            return;
        }

        try
        {
            SetBrowserStackSessionName($"{GetType().Name} | {testName}");
        }
        catch (Exception nameException)
        {
            Output.WriteLine($"BrowserStack session naming was unavailable: {nameException.Message}");
        }
    }

    protected static string DescribeBrowserStackFailure(Exception exception)
    {
        Exception rootCause = exception;
        while (rootCause.InnerException is not null)
        {
            rootCause = rootCause.InnerException;
        }

        string reason = rootCause == exception
            ? $"{exception.GetType().Name}: {exception.Message}"
            : $"{exception.GetType().Name}: {exception.Message} Root cause: " +
              $"{rootCause.GetType().Name}: {rootCause.Message}";
        const int browserStackReasonLimit = 255;
        return reason.Length <= browserStackReasonLimit
            ? reason
            : reason[..(browserStackReasonLimit - 3)] + "...";
    }

    private void SetBrowserStackSessionName(string sessionName)
        => ((IJavaScriptExecutor)Fixture.Driver).ExecuteScript(
            "browserstack_executor: { " +
            "\"action\": \"setSessionName\", " +
            "\"arguments\": { " +
            $"\"name\": \"{EscapeJson(sessionName)}\" " +
            "} " +
            "}");

    private void ReportNewHealingEvents()
    {
        if (_healingTelemetry is null)
        {
            return;
        }

        IReadOnlyList<MobileHealingEvent> events = _healingTelemetry.Snapshot();
        if (events.Count <= _reportedHealingEventCount)
        {
            return;
        }

        IReadOnlyList<MobileHealingEvent> newEvents = events
            .Skip(_reportedHealingEventCount)
            .ToArray();
        Output.WriteLine("=== Bounded mobile self-healing events ===");
        foreach (MobileHealingEvent healingEvent in newEvents)
        {
            string outcome = healingEvent.RecoveryActionSucceeded ? "recovered" : "recovery failed";
            Output.WriteLine(
                $"- {healingEvent.Type}: {healingEvent.Operation}; " +
                $"attempt {healingEvent.Attempt}/{healingEvent.MaximumAttempts}; {outcome}; " +
                $"duration {healingEvent.Duration.TotalMilliseconds:F0} ms. {healingEvent.Detail}");
        }

        _reportedHealingEventCount = events.Count;
        int warningThreshold = Math.Max(1, Fixture.Config.GetValue("SelfHealing:WarningThreshold", 2));
        if (events.Count >= warningThreshold)
        {
            Output.WriteLine(
                $"Self-healing warning: this session used {events.Count} bounded recoveries " +
                $"(configured threshold: {warningThreshold}). Investigate recurring instability.");
        }

        if ((Fixture.Config["RunMode"] ?? "Local").Is("Remote"))
        {
            try
            {
                BrowserStackLogHelper.Annotate(
                    Fixture.Driver,
                    $"Bounded self-healing: {newEvents.Count} new event(s), {events.Count} total.");
            }
            catch (Exception annotationException)
            {
                Output.WriteLine(
                    $"BrowserStack self-healing annotation was unavailable: {annotationException.Message}");
            }
        }
    }

    private static string EscapeJson(string value) => value
        .Replace("\\", "\\\\")
        .Replace("\"", "\\\"")
        .Replace("\r", "\\r")
        .Replace("\n", "\\n");
}
