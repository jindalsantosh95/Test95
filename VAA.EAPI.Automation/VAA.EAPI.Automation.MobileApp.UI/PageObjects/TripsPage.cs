using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Support.UI;

namespace VAA.EAPI.Automation.MobileApp.UI.PageObjects;

public sealed class TripsPage : NativeAppPage
{
    private const string TripsTabAccessibilityId = "My Trips";
    private static readonly TimeSpan TripsTimeout = TimeSpan.FromSeconds(45);
    private static readonly By TripsTabPostBookingFallback = new ByIosNSPredicate(
        "type == 'XCUIElementTypeButton' AND (name == 'My Trips' OR label == 'My Trips' " +
        "OR name == 'suitcase' OR label == 'suitcase' OR name == 'tripsTab' OR label == 'tripsTab')");

    public TripsPage(AppiumDriver driver)
        : base(driver, TripsTimeout)
    {
    }

    public void Open()
    {
        IWebElement? tripsTab = TryWaitForElementByKey(TripsTabAccessibilityId, TimeSpan.FromSeconds(2));
        if (tripsTab is not null && tripsTab.Displayed)
        {
            tripsTab.Click();
            return;
        }

        // iOS can expose the post-booking tab using its icon/container accessibility
        // metadata rather than its visible label. This mirrors the established mobile
        // framework predicate while retaining the app-owned ID as the primary route.
        try
        {
            WaitForElement(TripsTabPostBookingFallback, TripsTimeout).Click();
        }
        catch (WebDriverTimeoutException exception)
        {
            throw new WebDriverTimeoutException(
                "The My Trips tab did not become available after returning from booking confirmation. " +
                $"Tried accessibility ID '{TripsTabAccessibilityId}' first, then the documented " +
                "iOS navigation fallback. Inspect the BrowserStack screenshot/source before changing " +
                "the app selector contract.",
                exception);
        }
    }

    public bool ContainsBookingReference(string bookingReference)
    {
        string pattern = BookingReferencePattern(bookingReference);
        By reference = new ByIosNSPredicate(
            $"label MATCHES[c] '{pattern}' OR value MATCHES[c] '{pattern}' OR name MATCHES[c] '{pattern}'");
        WebDriverWait wait = new(Driver, TripsTimeout);
        try
        {
            return wait.Until(_ =>
            {
                if (Driver.FindElements(reference).Any(element => element.Displayed))
                {
                    return true;
                }

                Driver.ExecuteScript("mobile: scroll", new Dictionary<string, object> { ["direction"] = "down" });
                return false;
            });
        }
        catch (WebDriverTimeoutException)
        {
            return false;
        }
    }

    internal static string BookingReferencePattern(string bookingReference)
    {
        if (!System.Text.RegularExpressions.Regex.IsMatch(bookingReference ?? string.Empty, "^[A-Za-z0-9]{6}$"))
            throw new ArgumentException("Expected exactly six alphanumeric booking-reference characters.", nameof(bookingReference));

        // BookingCardInfoHeader spells the reference with spaces for VoiceOver.
        // Preserve character order and token boundaries; never remove all spaces from a card label.
        return "(?s).*(?<![A-Z0-9])" +
            string.Join("[ ]*", bookingReference!.ToUpperInvariant().Select(character => character.ToString())) +
            "(?![A-Z0-9]).*";
    }
}
