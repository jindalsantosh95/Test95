using Microsoft.Extensions.Configuration;
using VAA.EAPI.Automation.Core.Helpers.FaultInjection;
using VAA.EAPI.Automation.MobileApp.UI.Flows;
using VAA.EAPI.Automation.MobileApp.UI.Handlers;
using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using VAA.EAPI.Automation.MobileApp.UI.Models;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

[Collection("Mobile fault injection")]
public abstract class ControlledSearchFailureTestBase(ITestOutputHelper output, EapiFaultCase fault) : SearchBookTestBase(output,
    new Dictionary<string, string?>
    {
        ["IOSLaunch:AutomationFeatureFlags"] = fault.Seat == SeatFaultOperation.None
            ? "850100-search-native-entry,850101-search-native-skip-seat-selection"
            : "850100-search-native-entry"
    })
{
    private FaultProxySession? _proxy;

    protected FaultProxySession ActiveProxy => _proxy
        ?? throw new InvalidOperationException("The controlled fault proxy has not been started.");

    public override async Task InitializeAsync()
    {
        IConfiguration configuration = MobileFaultFactAttribute.LoadConfiguration();
        if (!MobileFaultFactAttribute.CanRun(configuration, fault.PostHpp))
            throw new InvalidOperationException("Isolated fault routing must be explicitly configured.");
        try
        {
            if (configuration["RunMode"] == "Remote")
                _proxy = await StartProxyAsync(configuration);
            await base.InitializeAsync();
        }
        catch
        {
            if (_proxy is not null)
                await _proxy.DisposeAsync();
            throw;
        }
    }

    public override async Task DisposeAsync()
    {
        try
        { await base.DisposeAsync(); }
        finally
        {
            if (_proxy is not null)
                await _proxy.DisposeAsync();
        }
    }

    protected async Task VerifyControlledFailureAsync([System.Runtime.CompilerServices.CallerMemberName] string testName = "")
    {
        try
        {
            Assert.True(MobileFaultFactAttribute.CanRun(Fixture.Config, fault.PostHpp));
            new AppEntryHandler(Fixture.Driver).EnterAsGuest();
            NativeFlightSearchPage page = new(Fixture.Driver);
            page.Open();
            // Keep local and remote guards alive through evidence collection and driver teardown.
            // Disposing a local guard at method exit could release pending post-HPP requests.
            _proxy ??= await StartProxyAsync(Fixture.Config, Fixture.Config.GetValue<bool>("FaultInjection:UseProcessCapture")
                ? MobileFaultRouting.GetTestAppProcessId(Fixture.Driver) : null);
            FaultProxySession proxy = _proxy;
            try
            {
                page.SearchOneWay(FlightSearch.OneWay("LHR", "JFK", monthsFromToday: 3, PassengerCounts.OneAdult));
                string? selectedSeat = null;
                if (fault.Seat != SeatFaultOperation.None)
                {
                    page.WaitForResults();
                    page.SelectFirstFlightByCarrier(true, expectFlightSummary: true, requiredCabin: "Economy",
                        requiredFareTitle: "Economy Classic");
                    proxy.EnsureNoGuardViolation();
                    new NativeBookingCheckoutFlow(Fixture.Driver).OpenSeatSelectionForOneGuestAdult();
                    if (fault.Seat == SeatFaultOperation.SelectSeat)
                    {
                        NativeSeatSelectionPage seats = new(Fixture.Driver);
                        seats.WaitForActivePassenger(0);
                        selectedSeat = seats.OpenAvailableSeat();
                        seats.ConfirmSeat(selectedSeat);
                        proxy.EnsureNoGuardViolation();
                        seats.SubmitSelectionExpectingControlledFailure();
                    }
                }
                if (fault.PostHpp)
                {
                    page.WaitForResults();
                    page.SelectFirstFlightByCarrier(true, expectFlightSummary: true, requiredCabin: "Economy",
                        requiredFareTitle: "Economy Classic");
                    proxy.EnsureNoGuardViolation();
                    new NativeBookingCheckoutFlow(Fixture.Driver).SubmitTestPaymentForControlledPostHppFailure(proxy.EnsureNoGuardViolation);
                }
                if (fault.Basket)
                {
                    page.WaitForResults();
                    page.SelectFirstFlightByCarrier(true, expectFlightSummary: true, requiredCabin: "Economy");
                    new NativeBookingPage(Fixture.Driver).SubmitSummaryExpectingControlledBasketFailure(fault.ExpectedUiMessage);
                }
                if (fault.Timeout)
                    await proxy.WaitForWithheldRequestAsync(TimeSpan.FromSeconds(30));
                else
                    await proxy.WaitForInjectionAsync(TimeSpan.FromSeconds(30));
                Output.WriteLine(fault.Timeout
                    ? "Target request intercepted; 150-second response hold started. This marker alone does not prove the client timeout fired."
                    : $"Controlled fault intercepted: {fault.Operation}, EAPI code {fault.Code}, HTTP 200 with GraphQL errors. No traffic payloads retained.");
                if (fault.Seat != SeatFaultOperation.None)
                    Assert.True(new NativeSeatSelectionPage(Fixture.Driver).WaitForControlledError(
                        fault.ExpectedUiMessage, fault.Seat == SeatFaultOperation.SelectSeat, selectedSeat),
                        "Expected seat error, loading stopped and recovery enabled without payment navigation; no retry or skip clicked.");
                else if (fault.PostHpp)
                    Assert.True(new NativeBookingPage(Fixture.Driver).WaitForControlledPostHppError(fault.ExpectedUiMessage, fault.Timeout),
                        "Expected post-HPP error, no successful confirmation and an enabled recovery action; no retry clicked.");
                else if (!fault.Basket)
                    Assert.True(page.WaitForControlledSearchError(fault.ExpectedUiMessage, fault.Timeout), "Expected search error and enabled Search must be visible.");
                proxy.EnsureNoGuardViolation();
                Output.WriteLine($"Expected error state verified at {DateTimeOffset.UtcNow:O}. No retry action performed.");
                TrySetBrowserStackStatus("passed", $"Controlled {fault.Code} error shown; progression stopped safely.", testName);
            }
            finally
            {
                Output.WriteLine("Fault proxy evidence (UTC, no payloads): " + proxy.ReadEventSummary());
            }
        }
        catch (Exception exception)
        {
            TrySetBrowserStackStatus("failed", DescribeBrowserStackFailure(exception), testName);
            throw;
        }
    }

    protected async Task StartLocalProxyIfRequiredAsync()
        => _proxy ??= await StartProxyAsync(Fixture.Config, Fixture.Config.GetValue<bool>("FaultInjection:UseProcessCapture")
            ? MobileFaultRouting.GetTestAppProcessId(Fixture.Driver) : null);

    private Task<FaultProxySession> StartProxyAsync(IConfiguration configuration, int? processId = null)
    {
        string Require(string key) => configuration["FaultInjection:" + key]
            ?? throw new InvalidOperationException($"FaultInjection:{key} must be configured explicitly.");
        return FaultProxySession.StartAsync(Require("Executable"), Require("AddonPath"), fault.BuildRule(),
            certificateDirectory: Require("CertificateDirectory"), localCaptureProcessId: processId);
    }
}

public sealed class ControlledSearchFailureTests(ITestOutputHelper output) : ControlledSearchFailureTestBase(output, EapiFaultCase.PastDate)
{
    [MobileFaultFact]
    [Trait("Category", "MobileApp-SearchBook-FaultInjection")]
    [Trait("Suite", "FaultInjection")]
    [Trait("Area", "SearchAndBook")]
    [Trait("Feature", "Search-ErrorHandling")]
    [Trait("TestType", "Functional")]
    [Trait("Readiness", "Draft")]
    [Trait("Platform", "iOS")]
    public Task SearchBookFault_TC01_ControlledSearchError_ShowsErrorAndAllowsSearch() => VerifyControlledFailureAsync();
}
