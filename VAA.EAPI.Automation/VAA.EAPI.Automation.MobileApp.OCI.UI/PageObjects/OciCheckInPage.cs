using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Support.UI;
using VAA.EAPI.Automation.MobileApp.OCI.UI.Models;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;

namespace VAA.EAPI.Automation.MobileApp.OCI.UI.PageObjects;

public sealed class OciCheckInPage : NativeAppPage
{
    public const string AddTrip = "AddTripButton";
    public const string HeaderAddTrip = "HomeAddTripButton";
    public const string BookingReference = "ConfirmationNumberText";
    public const string AddTripNext = "Next";
    public const string FirstName = "FirstNameText";
    public const string LastName = "LastNameText";
    public const string FindMyTrip = "FindMyTripButton";
    public const string CheckInNow = "Check in now";
    public const string ContinueToCheckIn = "ContinueToCheckInButton";
    public const string SubmitPassport = "SubmitPassportDataButton";
    public const string EmergencyContact = "EmergencyContactScreen";
    public const string SkipEmergencyContact = "SkipButton";
    public const string BaggageGuide = "Your baggage guide";
    public const string Continue = "Continue";
    public const string AcknowledgeHazardousMaterials = "AcknowledgeButton";
    public const string ViewBoardingPass = "ViewBoardingPassButton";
    public const string BoardingPass = "Boarding Pass";
    public const string AppleWallet = "Add to Apple Wallet";

    private static readonly TimeSpan JourneyTimeout = TimeSpan.FromSeconds(60);

    public OciCheckInPage(AppiumDriver driver)
        : base(driver, JourneyTimeout)
    {
    }

    public void AddPreparedTrip(OciCandidate candidate)
    {
        TapEither(AddTrip, HeaderAddTrip, JourneyTimeout);
        EnterText(BookingReference, candidate.Pnr);
        TapAddTripNext();
        EnterText(FirstName, candidate.FirstName);
        EnterText(LastName, candidate.LastName);
        DismissKeyboardIfPresent();
        TapByKey(FindMyTrip);
        By preparedTrip = new ByIosNSPredicate(
            $"name == 'FindMyTripButtonValue' AND (label == '{candidate.Pnr}' OR value == '{candidate.Pnr}')");
        WaitForElement(preparedTrip, JourneyTimeout);
    }

    public void CompleteCheckIn()
    {
        TapByKey(CheckInNow);
        TapContinueToCheckIn();
        TapWhenAvailable(SubmitPassport, JourneyTimeout);

        IWebElement nextScreen = WaitForEither(EmergencyContact, BaggageGuide, JourneyTimeout);
        if (nextScreen.GetAttribute("name") == EmergencyContact)
        {
            TapByKey(SkipEmergencyContact);
        }

        WaitForElementByKey(BaggageGuide, JourneyTimeout);
        ScrollToAndTap(Continue, 5);
        TapWhenAvailable(AcknowledgeHazardousMaterials, JourneyTimeout);
        TapWhenAvailable(ViewBoardingPass, JourneyTimeout);
    }

    public void AssertBoardingPassGenerated()
    {
        WaitForElementByKey(BoardingPass, JourneyTimeout);
        WaitForElementByKey(AppleWallet, JourneyTimeout);
    }

    private void EnterText(string accessibilityId, string value)
    {
        IWebElement element = WaitForElementByKey(accessibilityId);
        element.Clear();
        element.SendKeys(value);
    }

    private void TapAddTripNext()
    {
        IWebElement? stableControl = TryWaitForElementByKey(AddTripNext, TimeSpan.FromSeconds(2));
        if (stableControl is not null)
        {
            stableControl.Click();
            return;
        }

        // Removal condition: the iOS app supplies a dedicated accessibility identifier for
        // Add Trip Next. WDIO currently requires this same screen-scoped predicate fallback.
        By fallback = new ByIosNSPredicate(
            "type == 'XCUIElementTypeButton' AND " +
            "(name CONTAINS[c] 'Next' OR label CONTAINS[c] 'Next' OR value CONTAINS[c] 'Next')");
        WaitForElement(fallback, JourneyTimeout).Click();
    }

    private void TapContinueToCheckIn()
    {
        IWebElement? stableControl = TryWaitForElementByKey(ContinueToCheckIn, TimeSpan.FromSeconds(2));
        if (stableControl is not null)
        {
            stableControl.Click();
            return;
        }

        // Removal condition: Continue to Check In consistently exposes its app-owned
        // accessibility identifier. The existing WDIO journey needs this same fallback.
        By fallback = By.XPath(
            "//XCUIElementTypeButton[contains(@name,'Continue to check in') " +
            "or contains(@label,'Continue to check in')]");
        WaitForElement(fallback, JourneyTimeout).Click();
    }

    private void TapWhenAvailable(string accessibilityId, TimeSpan timeout)
    {
        WebDriverWait wait = new(Driver, timeout)
        {
            PollingInterval = TimeSpan.FromMilliseconds(250),
        };

        IWebElement element = wait.Until(_ =>
        {
            IWebElement? candidate = TryWaitForElementByKey(accessibilityId, TimeSpan.FromSeconds(1));
            if (candidate is null)
            {
                return null;
            }

            try
            {
                return candidate.Displayed && candidate.Enabled ? candidate : null;
            }
            catch (StaleElementReferenceException)
            {
                return null;
            }
        })!;

        element.Click();
    }

    private void TapEither(string primaryAccessibilityId, string alternateAccessibilityId, TimeSpan timeout)
    {
        WaitForEither(primaryAccessibilityId, alternateAccessibilityId, timeout).Click();
    }

    private IWebElement WaitForEither(string firstAccessibilityId, string secondAccessibilityId, TimeSpan timeout)
    {
        WebDriverWait wait = new(Driver, timeout);
        return wait.Until(_ =>
            Driver.FindElements(MobileBy.AccessibilityId(firstAccessibilityId))
                .Concat(Driver.FindElements(MobileBy.AccessibilityId(secondAccessibilityId)))
                .FirstOrDefault(element => element.Displayed))!;
    }

    private void ScrollToAndTap(string accessibilityId, int maximumScrolls)
    {
        for (int attempt = 0; attempt <= maximumScrolls; attempt++)
        {
            IWebElement? element = Driver.FindElements(MobileBy.AccessibilityId(accessibilityId))
                .FirstOrDefault(candidate => candidate.Displayed && candidate.Enabled);
            if (element is not null)
            {
                element.Click();
                return;
            }

            if (attempt < maximumScrolls)
            {
                Driver.ExecuteScript("mobile: scroll", new Dictionary<string, object>
                {
                    ["direction"] = "down",
                });
            }
        }

        throw new NoSuchElementException(
            $"Native control '{accessibilityId}' was not available after {maximumScrolls} bounded scrolls.");
    }

    private void DismissKeyboardIfPresent()
    {
        try
        {
            Driver.HideKeyboard();
        }
        catch (WebDriverException)
        {
            // iOS reports an error when the keyboard has already disappeared; the next
            // accessibility-ID lookup remains the authoritative readiness check.
        }
    }
}
