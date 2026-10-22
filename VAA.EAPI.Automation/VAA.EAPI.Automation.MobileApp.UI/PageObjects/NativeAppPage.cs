using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using VAA.EAPI.Automation.Core.UI.PageObjects;

using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;

namespace VAA.EAPI.Automation.MobileApp.UI.PageObjects;

/// <summary>
/// Native-app page base for the mobile automation contract.
///
/// New native controls must be located by their app-owned accessibility identifier.
/// A text, predicate, or XPath locator is allowed only when the app contract explicitly
/// records the missing identifier; keep that fallback in the owning page object with a
/// removal condition. This intentionally does not inherit AppBasePage's generic key-to-XPath
/// fallbacks, which were created for legacy cross-platform UI suites.
/// </summary>
public abstract class NativeAppPage : AppBasePage
{
    protected NativeAppPage(AppiumDriver driver, TimeSpan timeout)
        : base(driver, timeout)
    {
        HealingPolicy = new MobileHealingPolicy(MobileHealingTelemetryRegistry.Get(driver));
    }

    protected MobileHealingPolicy HealingPolicy { get; }

    protected new IWebElement WaitForElementByKey(string accessibilityId, TimeSpan? timeoutOverride = null)
        => WaitForElement(MobileBy.AccessibilityId(accessibilityId), timeoutOverride);

    protected new IWebElement? TryWaitForElementByKey(string accessibilityId, TimeSpan? timeoutOverride = null)
        => TryWaitForElement(MobileBy.AccessibilityId(accessibilityId), timeoutOverride);

    protected new bool IsVisibleByKey(string accessibilityId, TimeSpan? timeoutOverride = null)
        => IsVisible(MobileBy.AccessibilityId(accessibilityId), timeoutOverride);

    protected new void TapByKey(string accessibilityId)
    {
        try
        {
            WaitForElementByKey(accessibilityId).Click();
        }
        catch (WebDriverTimeoutException exception)
        {
            throw new WebDriverTimeoutException(
                $"Native control '{accessibilityId}' was not available for tap. " +
                "Locator strategy: Appium accessibility ID. Inspect the BrowserStack screenshot/source " +
                "to distinguish a missing app contract from an unsettled screen transition.",
                exception);
        }
    }

    protected IWebElement ScrollWithinContainerToKey(
        string containerAccessibilityId,
        string targetAccessibilityId,
        int maximumScrolls,
        double distance = 0.7,
        bool useNativeSwipe = false)
    {
        for (int attempt = 0; attempt <= maximumScrolls; attempt++)
        {
            IWebElement container = WaitForElementByKey(containerAccessibilityId);
            if (useNativeSwipe && !container.Displayed)
            {
                throw new InvalidOperationException(
                    $"Cannot swipe '{containerAccessibilityId}' towards '{targetAccessibilityId}': the container is hidden by navigation or an overlay.");
            }
            IWebElement? target = container
                .FindElements(MobileBy.AccessibilityId(targetAccessibilityId))
                .FirstOrDefault(IsDisplayedWithoutThrowing);
            if (target is not null)
            {
                return target;
            }

            if (attempt < maximumScrolls)
            {
                Driver.ExecuteScript(useNativeSwipe ? "mobile: swipe" : "mobile: scroll",
                    ContainerNavigationArguments(GetElementReference(container), distance, useNativeSwipe));
            }
        }

        throw new NoSuchElementException(
            $"Could not find '{targetAccessibilityId}' within '{containerAccessibilityId}' " +
            $"after {maximumScrolls} bounded scrolls.");
    }

    internal static Dictionary<string, object> ContainerNavigationArguments(
        string elementId, double distance, bool useNativeSwipe)
    {
        // Swipe direction describes finger movement; scroll direction describes content traversal.
        // XCTest swipe avoids mobile:scroll's long press, which can activate SwiftUI menus.
        Dictionary<string, object> arguments = new()
        {
            ["elementId"] = elementId,
            ["direction"] = useNativeSwipe ? "up" : "down",
        };
        if (!useNativeSwipe)
            arguments["distance"] = distance;
        return arguments;
    }

    private static bool IsDisplayedWithoutThrowing(IWebElement element)
    {
        try
        {
            return element.Displayed;
        }
        catch (StaleElementReferenceException)
        {
            return false;
        }
    }

    protected static string GetElementReference(IWebElement element)
    {
        Type? elementType = element.GetType();
        while (elementType is not null)
        {
            System.Reflection.PropertyInfo? idProperty = elementType.GetProperty(
                "Id",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (idProperty?.GetValue(element) is string elementId && elementId.Length > 0)
            {
                return elementId;
            }

            elementType = elementType.BaseType;
        }

        throw new InvalidOperationException("Appium did not expose the scroll-container element reference.");
    }
}
