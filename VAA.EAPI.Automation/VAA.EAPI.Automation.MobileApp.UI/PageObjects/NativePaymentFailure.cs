using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace VAA.EAPI.Automation.MobileApp.UI.PageObjects;

internal static class NativePaymentFailure
{
    // Temporary English-copy fallback for the inline error, which has no owned ID.
    // Replace with the payment error ID when the app contract exposes it (see README).
    internal const string InlineErrorPredicate =
        "(type == 'XCUIElementTypeStaticText' OR type == 'XCUIElementTypeButton') AND " +
        "(name CONTAINS[c] 'Try secure payment again' OR " +
        "label CONTAINS[c] 'Try secure payment again' OR " +
        "value CONTAINS[c] 'Try secure payment again' OR " +
        "name CONTAINS[c] 'We could not complete your booking' OR " +
        "label CONTAINS[c] 'We could not complete your booking' OR " +
        "value CONTAINS[c] 'We could not complete your booking')";

    internal static void ThrowIfDisplayed(Func<By, bool> isDisplayed, string stage)
    {
        // Native post-HPP errors may expose only their accessible children, not the
        // containing page. Observe any owned error marker; never click a recovery action.
        foreach (string identifier in new[]
        {
            NativeBookingAccessibilityIds.PaymentErrorTitle,
            NativeBookingAccessibilityIds.PaymentErrorMessage,
            NativeBookingAccessibilityIds.PaymentErrorPage,
        })
        {
            if (isDisplayed(MobileBy.AccessibilityId(identifier)))
            {
                throw new InvalidOperationException(
                    $"Payment/booking status could not be confirmed {stage}: the app displayed {identifier}. " +
                    "No payment retry was attempted. Inspect the captured failure evidence; " +
                    "this UI error does not establish the backend cause or whether payment was captured.");
            }
        }

        string? failure = isDisplayed(MobileBy.AccessibilityId(NativeBookingAccessibilityIds.PaymentRetryButton))
            ? "the app displayed NativeFlightPaymentRetryButton"
            : isDisplayed(new ByIosNSPredicate(InlineErrorPredicate))
                ? "the app displayed 'We could not complete your booking' / 'Try secure payment again'"
                : null;
        if (failure is not null)
        {
            throw new InvalidOperationException(
                $"Secure payment failed {stage}: {failure}. " +
                "No payment retry was attempted. Review the captured mobile failure evidence; " +
                "the underlying backend cause is not established by this UI error.");
        }
    }
}
