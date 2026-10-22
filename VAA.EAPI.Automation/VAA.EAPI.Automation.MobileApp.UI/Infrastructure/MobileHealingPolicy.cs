using System.Diagnostics;

using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.MobileApp.UI.Infrastructure;

public sealed class MobileHealingPolicy
{
    private readonly MobileHealingTelemetry _telemetry;

    public MobileHealingPolicy(MobileHealingTelemetry telemetry)
    {
        _telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
    }

    public void ExecuteRecovery(
        MobileHealingType type,
        string operation,
        int attempt,
        int maximumAttempts,
        string detail,
        Action recoveryAction)
    {
        if (operation.IsNullOrWhiteSpace())
        {
            throw new ArgumentException("A healing operation name is required.", nameof(operation));
        }

        if (attempt <= 0 || maximumAttempts <= 0 || attempt > maximumAttempts)
        {
            throw new ArgumentOutOfRangeException(
                nameof(attempt),
                attempt,
                $"Healing attempt must be between 1 and {maximumAttempts}.");
        }

        ArgumentNullException.ThrowIfNull(recoveryAction);

        Stopwatch duration = Stopwatch.StartNew();
        try
        {
            recoveryAction();
            duration.Stop();
            Record(type, operation, attempt, maximumAttempts, duration.Elapsed, true, detail);
        }
        catch
        {
            duration.Stop();
            Record(type, operation, attempt, maximumAttempts, duration.Elapsed, false, detail);
            throw;
        }
    }

    private void Record(
        MobileHealingType type,
        string operation,
        int attempt,
        int maximumAttempts,
        TimeSpan duration,
        bool succeeded,
        string detail)
        => _telemetry.Record(new MobileHealingEvent(
            DateTimeOffset.UtcNow,
            type,
            operation,
            attempt,
            maximumAttempts,
            duration,
            succeeded,
            detail));
}
