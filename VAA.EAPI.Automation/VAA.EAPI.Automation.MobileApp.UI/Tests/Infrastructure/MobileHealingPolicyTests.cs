using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

public sealed class MobileHealingPolicyTests
{
    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void ExecuteRecovery_SuccessfulBoundedAction_RecordsObservableHealingEvent()
    {
        MobileHealingTelemetry telemetry = new();
        MobileHealingPolicy policy = new(telemetry);
        bool recoveryExecuted = false;

        policy.ExecuteRecovery(
            MobileHealingType.RerenderRetry,
            "Re-query filter option",
            attempt: 1,
            maximumAttempts: 1,
            "SwiftUI rebuilt the filter row.",
            () => recoveryExecuted = true);

        MobileHealingEvent healingEvent = Assert.Single(telemetry.Snapshot());
        Assert.True(recoveryExecuted);
        Assert.True(healingEvent.RecoveryActionSucceeded);
        Assert.Equal(MobileHealingType.RerenderRetry, healingEvent.Type);
        Assert.Equal("Re-query filter option", healingEvent.Operation);
        Assert.Equal(1, healingEvent.Attempt);
        Assert.Equal(1, healingEvent.MaximumAttempts);
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void ExecuteRecovery_FailedRecoveryAction_RecordsFailureAndPreservesException()
    {
        MobileHealingTelemetry telemetry = new();
        MobileHealingPolicy policy = new(telemetry);

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
            policy.ExecuteRecovery(
                MobileHealingType.TransientStateRetry,
                "Recover transient login",
                attempt: 1,
                maximumAttempts: 2,
                "The known transient state remained visible.",
                () => throw new InvalidOperationException("Recovery did not restore the login form.")));

        MobileHealingEvent healingEvent = Assert.Single(telemetry.Snapshot());
        Assert.Equal("Recovery did not restore the login form.", exception.Message);
        Assert.False(healingEvent.RecoveryActionSucceeded);
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData(0, 1)]
    [InlineData(2, 1)]
    [InlineData(1, 0)]
    public void ExecuteRecovery_AttemptOutsideBound_FailsBeforeExecutingAction(
        int attempt,
        int maximumAttempts)
    {
        MobileHealingTelemetry telemetry = new();
        MobileHealingPolicy policy = new(telemetry);
        bool recoveryExecuted = false;

        Assert.Throws<ArgumentOutOfRangeException>(() => policy.ExecuteRecovery(
            MobileHealingType.StaleElementRequery,
            "Re-query stale element",
            attempt,
            maximumAttempts,
            "A stale reference was observed.",
            () => recoveryExecuted = true));

        Assert.False(recoveryExecuted);
        Assert.Empty(telemetry.Snapshot());
    }
}
