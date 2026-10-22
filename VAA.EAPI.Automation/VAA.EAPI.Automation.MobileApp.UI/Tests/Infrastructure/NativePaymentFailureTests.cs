using VAA.EAPI.Automation.MobileApp.UI.PageObjects;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

public sealed class NativePaymentFailureTests
{
    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData(true, false, 4, "NativeFlightPaymentRetryButton")]
    [InlineData(false, true, 5, "We could not complete your booking")]
    public void PaymentError_FailsOnFirstObservationWithoutRetry(
        bool retryVisible, bool inlineVisible, int expectedReads, string expectedEvidence)
    {
        int reads = 0;
        InvalidOperationException failure = Assert.Throws<InvalidOperationException>(() =>
            NativePaymentFailure.ThrowIfDisplayed(locator =>
            {
                reads++;
                if (locator.ToString().Contains(NativeBookingAccessibilityIds.PaymentRetryButton))
                    return retryVisible;
                return locator.ToString().Contains(NativePaymentFailure.InlineErrorPredicate) && inlineVisible;
            }, "while waiting for booking confirmation"));

        Assert.Equal(expectedReads, reads);
        Assert.Contains(expectedEvidence, failure.Message);
        Assert.Contains("while waiting for booking confirmation", failure.Message);
        Assert.Contains("No payment retry was attempted", failure.Message);
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void NoVisiblePaymentError_AllowsWaitToContinue()
    {
        int reads = 0;
        NativePaymentFailure.ThrowIfDisplayed(_ => { reads++; return false; }, "before HPP");
        Assert.Equal(5, reads);
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData(NativeBookingAccessibilityIds.PaymentErrorTitle, 1)]
    [InlineData(NativeBookingAccessibilityIds.PaymentErrorMessage, 2)]
    [InlineData(NativeBookingAccessibilityIds.PaymentErrorPage, 3)]
    public void DedicatedPaymentError_FailsImmediatelyForEachOwnedMarker(string identifier, int expectedReads)
    {
        int reads = 0;
        InvalidOperationException failure = Assert.Throws<InvalidOperationException>(() =>
            NativePaymentFailure.ThrowIfDisplayed(locator =>
            {
                reads++;
                return locator.ToString().Contains(identifier, StringComparison.Ordinal);
            }, "while waiting for booking confirmation"));
        Assert.Equal(expectedReads, reads);
        Assert.Contains(identifier, failure.Message);
        Assert.Contains("while waiting for booking confirmation", failure.Message);
        Assert.Contains("No payment retry was attempted", failure.Message);
        Assert.Contains("whether payment was captured", failure.Message);
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("name")]
    [InlineData("label")]
    [InlineData("value")]
    public void InlineErrorLocator_CoversStaticTextAndLegacyButton(string attribute)
    {
        Assert.Contains("type == 'XCUIElementTypeStaticText' OR type == 'XCUIElementTypeButton'",
            NativePaymentFailure.InlineErrorPredicate);
        Assert.Contains($"{attribute} CONTAINS[c] 'We could not complete your booking'",
            NativePaymentFailure.InlineErrorPredicate);
        Assert.Contains($"{attribute} CONTAINS[c] 'Try secure payment again'",
            NativePaymentFailure.InlineErrorPredicate);
    }
}
