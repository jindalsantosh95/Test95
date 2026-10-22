using System.Xml.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Support.UI;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.MobileApp.UI.PageObjects;

public sealed class WelcomePage : NativeAppPage
{
    public const string GetStarted = "Get started";
    private const string ContinueWithoutSignIn = "Continue without sign in";

    private static readonly string[] RecognisedEntryPoints =
    {
        GetStarted,
        ContinueWithoutSignIn,
        "Book",
        "Explore",
        NativeSearchAccessibilityIds.FlyingFromButton,
    };

    public WelcomePage(AppiumDriver driver)
        : base(driver, TimeSpan.FromSeconds(30))
    {
    }

    public string WaitForRecognisedEntryPoint()
    {
        return Wait.Until(_ =>
        {
            foreach (string entryPoint in RecognisedEntryPoints)
            {
                IWebElement? element = Driver.FindElements(MobileBy.AccessibilityId(entryPoint))
                    .FirstOrDefault(IsDisplayed);
                if (element is not null)
                {
                    return entryPoint;
                }
            }

            return null;
        });
    }

    private static bool IsDisplayed(IWebElement element)
    {
        try
        {
            return element.Displayed;
        }
        catch (StaleElementReferenceException)
        {
            // SwiftUI rebuilds the tab/onboarding hierarchy during initial navigation.
            // The enclosing explicit wait will re-query the current accessibility tree.
            return false;
        }
    }

    public AppEntryState WaitForNextEntryState(
        TimeSpan timeout,
        Func<bool> dismissPrivacyIfPresent)
    {
        WebDriverWait entryWait = new(Driver, timeout)
        {
            PollingInterval = TimeSpan.FromMilliseconds(200),
        };
        int consecutiveNavigationObservations = 0;

        try
        {
            AppEntryResolution resolution = entryWait.Until(_ =>
            {
                if (DismissExpiredAuthenticationIfPresent())
                {
                    consecutiveNavigationObservations = 0;
                    return new AppEntryResolution(AppEntryState.GuestActionCompleted);
                }

                if (dismissPrivacyIfPresent())
                {
                    consecutiveNavigationObservations = 0;
                    return new AppEntryResolution(AppEntryState.PrivacyActionCompleted);
                }

                if (IsVisibleByKey(
                    NativeSearchAccessibilityIds.FlyingFromButton,
                    TimeSpan.FromMilliseconds(100)))
                {
                    return new AppEntryResolution(AppEntryState.NativeSearch);
                }

                IWebElement? getStarted = FindVisibleEnabled(GetStarted);
                if (getStarted is not null)
                {
                    return new AppEntryResolution(AppEntryState.Onboarding);
                }

                IWebElement? guestAction = FindVisibleEnabled(ContinueWithoutSignIn);
                if (guestAction is not null)
                {
                    guestAction.Click();
                    return new AppEntryResolution(AppEntryState.GuestActionCompleted);
                }

                consecutiveNavigationObservations = IsPrimaryNavigationVisible()
                    ? consecutiveNavigationObservations + 1
                    : 0;
                return consecutiveNavigationObservations >= 2
                    ? new AppEntryResolution(AppEntryState.PrimaryNavigation)
                    : null;
            });
            return resolution.State;
        }
        catch (WebDriverTimeoutException exception)
        {
            throw new WebDriverTimeoutException(
                $"The app entry state did not settle. Visible native controls: {DescribeVisibleControls()}",
                exception);
        }
    }

    private IWebElement? FindVisibleEnabled(string accessibilityId)
        => Driver.FindElements(MobileBy.AccessibilityId(accessibilityId))
            .FirstOrDefault(element => IsDisplayed(element) && element.Enabled);

    private bool DismissExpiredAuthenticationIfPresent()
    {
        IWebElement? expiryMessage = Driver.FindElements(new ByIosNSPredicate(
                "name == 'Authentication expired' OR label == 'Authentication expired' " +
                "OR name CONTAINS 'sign in again' OR label CONTAINS 'sign in again'"))
            .FirstOrDefault(IsDisplayed);
        if (expiryMessage is null)
        {
            return false;
        }

        IWebElement? confirmation = FindVisibleEnabled("OK");
        if (confirmation is null)
        {
            return false;
        }

        confirmation.Click();
        return true;
    }

    private bool IsPrimaryNavigationVisible()
        => IsVisibleByKey("Book", TimeSpan.FromMilliseconds(250)) ||
            IsVisibleByKey("Explore", TimeSpan.FromMilliseconds(250));

    private string DescribeVisibleControls()
    {
        try
        {
            XDocument source = XDocument.Parse(Driver.PageSource);
            string[] controls = source.Descendants()
                .Where(element =>
                    element.Attribute("visible")?.Value.Equals("true", StringComparison.OrdinalIgnoreCase) == true)
                .SelectMany(element => new[]
                {
                    element.Attribute("name")?.Value,
                    element.Attribute("label")?.Value,
                    element.Attribute("value")?.Value,
                })
                .Where(value => !value.IsNullOrWhiteSpace())
                .Distinct(StringComparer.Ordinal)
                .Take(30)
                .ToArray()!;
            return controls.Length == 0 ? "none exposed" : string.Join(" | ", controls);
        }
        catch (Exception sourceException)
        {
            return $"unavailable ({sourceException.GetType().Name})";
        }
    }
}

public enum AppEntryState
{
    NativeSearch,
    Onboarding,
    GuestActionCompleted,
    PrivacyActionCompleted,
    PrimaryNavigation,
}

internal sealed record AppEntryResolution(AppEntryState State);
