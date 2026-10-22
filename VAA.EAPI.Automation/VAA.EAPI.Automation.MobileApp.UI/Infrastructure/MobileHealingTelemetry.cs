using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using OpenQA.Selenium.Appium;

namespace VAA.EAPI.Automation.MobileApp.UI.Infrastructure;

public enum MobileHealingType
{
    StaleElementRequery,
    TransientStateRetry,
    RerenderRetry,
    SessionRecovery,
    TestDataReplacement,
}

public sealed record MobileHealingEvent(
    DateTimeOffset Timestamp,
    MobileHealingType Type,
    string Operation,
    int Attempt,
    int MaximumAttempts,
    TimeSpan Duration,
    bool RecoveryActionSucceeded,
    string Detail);

public sealed class MobileHealingTelemetry
{
    private readonly List<MobileHealingEvent> _events = [];
    private readonly object _sync = new();
    private readonly bool _enabled;

    public MobileHealingTelemetry()
        : this(true)
    {
    }

    private MobileHealingTelemetry(bool enabled)
    {
        _enabled = enabled;
    }

    internal static MobileHealingTelemetry Disabled { get; } = new(false);

    public IReadOnlyList<MobileHealingEvent> Snapshot()
    {
        lock (_sync)
        {
            return new ReadOnlyCollection<MobileHealingEvent>([.. _events]);
        }
    }

    internal void Record(MobileHealingEvent healingEvent)
    {
        if (!_enabled)
        {
            return;
        }

        lock (_sync)
        {
            _events.Add(healingEvent);
        }
    }
}

public static class MobileHealingTelemetryRegistry
{
    private static readonly ConditionalWeakTable<AppiumDriver, MobileHealingTelemetry> TelemetryByDriver = new();

    public static MobileHealingTelemetry Attach(AppiumDriver driver)
    {
        ArgumentNullException.ThrowIfNull(driver);
        TelemetryByDriver.Remove(driver);
        MobileHealingTelemetry telemetry = new();
        TelemetryByDriver.Add(driver, telemetry);
        return telemetry;
    }

    public static void Detach(AppiumDriver driver)
    {
        ArgumentNullException.ThrowIfNull(driver);
        TelemetryByDriver.Remove(driver);
    }

    internal static MobileHealingTelemetry Get(AppiumDriver driver)
    {
        ArgumentNullException.ThrowIfNull(driver);
        return TelemetryByDriver.TryGetValue(driver, out MobileHealingTelemetry? telemetry)
            ? telemetry
            : MobileHealingTelemetry.Disabled;
    }
}
