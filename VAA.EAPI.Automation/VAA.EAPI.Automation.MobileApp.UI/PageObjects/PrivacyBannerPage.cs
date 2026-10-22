using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace VAA.EAPI.Automation.MobileApp.UI.PageObjects;

/// <summary>Dismisses the non-login privacy banner, preferring Reject All as in the existing mobile harness.</summary>
public sealed class PrivacyBannerPage : NativeAppPage
{
    private static readonly By RejectAll = new ByIosNSPredicate(
        "type == 'XCUIElementTypeButton' AND " +
        "(name CONTAINS[c] 'Reject All' OR label CONTAINS[c] 'Reject All' OR value CONTAINS[c] 'Reject All')");

    private static readonly By Agree = new ByIosNSPredicate(
        "type == 'XCUIElementTypeButton' AND " +
        "(name CONTAINS[c] 'I Agree' OR label CONTAINS[c] 'I Agree' OR value CONTAINS[c] 'I Agree')");

    public PrivacyBannerPage(AppiumDriver driver) : base(driver, TimeSpan.FromSeconds(10)) { }

    public bool DismissIfPresent()
    {
        BannerState? state;
        try
        {
            state = Wait.Until(_ =>
            {
                IWebElement? action = FindVisible(RejectAll) ?? FindVisible(Agree);
                if (action is not null)
                    return new BannerState(action);
                bool primaryNavigationVisible = IsVisibleByKey("Book", TimeSpan.FromMilliseconds(250)) ||
                    IsVisibleByKey("Explore", TimeSpan.FromMilliseconds(250));
                return primaryNavigationVisible ? BannerState.NoBanner : null;
            });
        }
        catch (WebDriverTimeoutException)
        {
            // The banner is optional and is not always exposed in the native accessibility
            // tree. Primary-navigation validation below remains the authoritative readiness wait.
            return false;
        }

        if (state.Action is null)
        {
            return false;
        }

        IWebElement action = state.Action;
        action.Click();
        Wait.Until(_ => IsVisibleByKey("Book", TimeSpan.FromMilliseconds(500)) || !IsDisplayed(action));
        return true;
    }

    public bool DismissVisibleActionIfPresent()
    {
        IWebElement? action = FindVisible(RejectAll) ?? FindVisible(Agree);
        if (action is null)
        {
            return false;
        }

        action.Click();
        return true;
    }

    private IWebElement? FindVisible(By locator)
    {
        try
        {
            IWebElement element = Driver.FindElement(locator);
            return element.Displayed && element.Enabled ? element : null;
        }
        catch (NoSuchElementException) { return null; }
        catch (StaleElementReferenceException) { return null; }
    }

    private static bool IsDisplayed(IWebElement element)
    {
        try
        { return element.Displayed; }
        catch (StaleElementReferenceException) { return false; }
    }

    private sealed record BannerState(IWebElement? Action)
    {
        public static readonly BannerState NoBanner = new((IWebElement?)null);
    }
}
