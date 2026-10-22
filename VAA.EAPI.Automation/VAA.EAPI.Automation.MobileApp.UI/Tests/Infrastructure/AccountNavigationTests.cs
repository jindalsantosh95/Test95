using OpenQA.Selenium;
using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

public sealed class AccountNavigationTests
{
    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void AccountEntry_StopsWhenDestinationAppearsAndRecordsOnlyRecoveryTaps(int successfulAttempt)
    {
        int taps = 0;
        int waits = 0;
        MobileHealingTelemetry telemetry = new();
        LoyaltyLoginPage.NavigateToAccount(
            () => taps++,
            () =>
            {
                if (++waits < successfulAttempt)
                {
                    throw new AccountEntryReadinessTimeoutException("No Account state appeared.");
                }
            },
            new MobileHealingPolicy(telemetry));

        Assert.Equal(successfulAttempt, taps);
        Assert.Equal(successfulAttempt, waits);
        Assert.Equal(successfulAttempt - 1, telemetry.Snapshot().Count);
        Assert.All(telemetry.Snapshot(), recovery =>
        {
            Assert.Equal(2, recovery.MaximumAttempts);
            Assert.True(recovery.RecoveryActionSucceeded);
            Assert.Equal(MobileHealingType.TransientStateRetry, recovery.Type);
        });
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void AccountEntry_NeverSettles_FailsAfterThreeTapsWithOriginalException()
    {
        int taps = 0;
        MobileHealingTelemetry telemetry = new();
        AccountEntryReadinessTimeoutException failure = new("No Account state appeared.");
        Assert.Same(failure, Assert.Throws<AccountEntryReadinessTimeoutException>(() =>
            LoyaltyLoginPage.NavigateToAccount(() => taps++, () => throw failure, new MobileHealingPolicy(telemetry))));
        Assert.Equal(3, taps);
        Assert.Equal(2, telemetry.Snapshot().Count);
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void AccountEntry_TransportFailure_IsNotRetried()
    {
        int taps = 0;
        MobileHealingTelemetry telemetry = new();
        WebDriverException failure = new("socket hang up");
        Assert.Same(failure, Assert.Throws<WebDriverException>(() =>
            LoyaltyLoginPage.NavigateToAccount(() => taps++, () => throw failure, new MobileHealingPolicy(telemetry))));
        Assert.Equal(1, taps);
        Assert.Empty(telemetry.Snapshot());
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData(1)]
    [InlineData(2)]
    public void AccountEntry_TapTimeout_IsNotRetried(int failedTap)
    {
        int taps = 0;
        int waits = 0;
        MobileHealingTelemetry telemetry = new();
        WebDriverTimeoutException failure = new("Account tap command timed out.");
        Assert.Same(failure, Assert.Throws<WebDriverTimeoutException>(() =>
            LoyaltyLoginPage.NavigateToAccount(
                () =>
                {
                    if (++taps == failedTap)
                    {
                        throw failure;
                    }
                },
                () =>
                {
                    waits++;
                    throw new AccountEntryReadinessTimeoutException("No Account state appeared.");
                },
                new MobileHealingPolicy(telemetry))));
        Assert.Equal(failedTap, taps);
        Assert.Equal(failedTap - 1, waits);
        if (failedTap > 1)
        {
            Assert.False(Assert.Single(telemetry.Snapshot()).RecoveryActionSucceeded);
        }
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void AccountEntry_VisibleAppError_IsNotRetried()
    {
        int taps = 0;
        MobileHealingTelemetry telemetry = new();
        InvalidOperationException failure = new("Account displayed its load-error state.");
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() =>
            LoyaltyLoginPage.NavigateToAccount(() => taps++, () => throw failure, new MobileHealingPolicy(telemetry))));
        Assert.Equal(1, taps);
        Assert.Empty(telemetry.Snapshot());
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void AccountEntry_CommandTimeoutInsideReadinessPoll_PropagatesWithoutRetapping()
    {
        int taps = 0;
        int polls = 0;
        MobileHealingTelemetry telemetry = new();
        WebDriverTimeoutException failure = new("Privacy command timed out.");
        Assert.Same(failure, Assert.Throws<WebDriverTimeoutException>(() =>
            LoyaltyLoginPage.NavigateToAccount(
                () => taps++,
                () => LoyaltyLoginPage.WaitForAccountReadiness<object>(() =>
                {
                    polls++;
                    throw failure;
                }, TimeSpan.Zero),
                new MobileHealingPolicy(telemetry))));
        Assert.Equal(1, taps);
        Assert.Equal(1, polls);
        Assert.Empty(telemetry.Snapshot());
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void AccountEntry_ReadinessExpiryWithoutCommandFailure_AllowsOnlyBoundedRetapping()
    {
        int taps = 0;
        MobileHealingTelemetry telemetry = new();
        Assert.Throws<AccountEntryReadinessTimeoutException>(() =>
            LoyaltyLoginPage.NavigateToAccount(
                () => taps++,
                () => LoyaltyLoginPage.WaitForAccountReadiness<object>(() => null, TimeSpan.Zero),
                new MobileHealingPolicy(telemetry)));
        Assert.Equal(3, taps);
        Assert.Equal(2, telemetry.Snapshot().Count);
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void AccountReadiness_ObservedDestination_ReturnsWithoutWaiting()
    {
        object destination = new();
        Assert.Same(destination, LoyaltyLoginPage.WaitForAccountReadiness(() => destination, TimeSpan.Zero));
    }
}
