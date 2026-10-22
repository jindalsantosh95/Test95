using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Interactions;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System.Collections.Generic;

namespace VAA.EAPI.Automation.Core.UI.PageObjects
{
    public abstract class AppBasePage
    {
        protected readonly AppiumDriver Driver;
        protected readonly WebDriverWait Wait;

        protected AppBasePage(AppiumDriver driver, TimeSpan timeout)
        {
            Driver = driver ?? throw new ArgumentNullException(nameof(driver));
            Wait = new WebDriverWait(driver, timeout);
        }

        // -------------------------
        // Core wait helpers
        // -------------------------

        protected IWebElement WaitForElement(By locator, TimeSpan? timeoutOverride = null)
        {
            var wait = timeoutOverride.HasValue
                ? new WebDriverWait(Driver, timeoutOverride.Value)
                : Wait;

            return wait.Until(d => d.FindElement(locator));
        }

        protected IWebElement TryWaitForElement(By locator, TimeSpan? timeoutOverride = null)
        {
            var wait = timeoutOverride.HasValue
                ? new WebDriverWait(Driver, timeoutOverride.Value)
                : Wait;

            try
            {
                return wait.Until(d => d.FindElement(locator));
            }
            catch (WebDriverTimeoutException) { return null; }
            catch (NoSuchElementException) { return null; }
            catch (StaleElementReferenceException) { return null; }
        }

        protected bool IsVisible(By locator, TimeSpan? timeoutOverride = null)
        {
            var wait = timeoutOverride.HasValue
                ? new WebDriverWait(Driver, timeoutOverride.Value)
                : Wait;

            try
            {
                return wait.Until(d =>
                {
                    var el = d.FindElement(locator);
                    return el.Displayed;
                });
            }
            catch (WebDriverTimeoutException) { return false; }
            catch (NoSuchElementException) { return false; }
            catch (StaleElementReferenceException) { return false; }
        }

        // -------------------------
        // Default cross-platform XPath fallback
        // -------------------------

        protected virtual By DefaultFallbackXpathFor(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key must not be null/empty.", nameof(key));

            var literal = ToXPathLiteral(key);

            return By.XPath(
                $"//*[@text={literal} or @content-desc={literal} or @label={literal} or @name={literal}]"
            );
        }

        protected virtual By DefaultContainsFallbackXpathFor(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key must not be null/empty.", nameof(key));

            var literal = ToXPathLiteral(key);

            // Contains match fallback (LAST resort)
            return By.XPath($"//*[contains(@text, {literal})" + $" or contains(@content-desc, {literal})" + $" or contains(@label, {literal})" + $" or contains(@name, {literal})]");
        }

        protected static string ToXPathLiteral(string value)
        {
            if (!value.Contains("'"))
                return $"'{value}'";

            var parts = value.Split('\'');
            var sb = new System.Text.StringBuilder("concat(");

            for (int i = 0; i < parts.Length; i++)
            {
                if (i > 0)
                    sb.Append(", \"'\", ");
                sb.Append($"'{parts[i]}'");
            }

            sb.Append(")");
            return sb.ToString();
        }

        // -------------------------
        // Key-based finders (AccessibilityId → XPath fallback)
        // -------------------------

        protected IWebElement WaitForElementByKey(string key, TimeSpan? timeoutOverride = null)
        {
            // 1) Fast path: AccessibilityId
            var primary = TryWaitForElement(MobileBy.AccessibilityId(key), TimeSpan.FromSeconds(2));
            if (primary != null) return primary;

            // 2) Exact XPath fallback
            var exact = TryWaitForElement(DefaultFallbackXpathFor(key), TimeSpan.FromSeconds(2));
            if (exact != null) return exact;

            // 3) Contains XPath fallback (last resort)
            return WaitForElement(DefaultContainsFallbackXpathFor(key), timeoutOverride);
        }

        protected IWebElement TryWaitForElementByKey(string key, TimeSpan? timeoutOverride = null)
        {
            // 1) AccessibilityId
            var primary = TryWaitForElement(MobileBy.AccessibilityId(key), timeoutOverride);
            if (primary != null) return primary;

            // 2) Exact XPath
            var exact = TryWaitForElement(DefaultFallbackXpathFor(key), timeoutOverride);
            if (exact != null) return exact;

            // 3) Contains XPath
            return TryWaitForElement(DefaultContainsFallbackXpathFor(key), timeoutOverride);
        }

        protected bool IsVisibleByKey(string key, TimeSpan? timeoutOverride = null)
        {
            // 1) AccessibilityId quick check
            if (IsVisible(MobileBy.AccessibilityId(key), TimeSpan.FromSeconds(1)))
                return true;

            // 2) Exact XPath check
            if (IsVisible(DefaultFallbackXpathFor(key), TimeSpan.FromSeconds(1)))
                return true;

            // 3) Contains XPath check
            return IsVisible(DefaultContainsFallbackXpathFor(key), timeoutOverride);
        }

        // -------------------------
        // Actions (key-based only)
        // -------------------------

        protected void TapByKey(string key)
            => WaitForElementByKey(key).Click();

        protected void TapByKeyIfFound(string key)
        {
            var el = TryWaitForElementByKey(key, TimeSpan.FromSeconds(5));
            el?.Click();
        }

        protected void EnterTextByKey(string key, string text, bool clearFirst = true)
        {
            var el = WaitForElementByKey(key);
            if (clearFirst) el.Clear();
            el.SendKeys(text);
        }

        protected string GetTextByKey(string key)
            => WaitForElementByKey(key).Text;

        protected string GetAttributeValueByKey(string key, string attributeName, TimeSpan? timeoutOverride = null)
        {
            var element = TryWaitForElementByKey(key, timeoutOverride ?? TimeSpan.FromSeconds(2));

            if (element == null)
                return string.Empty;

            return element.GetAttribute(attributeName) ?? string.Empty;
        }

        // -------------------------
        // Actions (W3C Actions)
        // -------------------------

        protected void SwipeVertical(double startYPercent, double endYPercent, double xPercent = 0.5, int durationMs = 600)
        {
            var size = Driver.Manage().Window.Size;

            int startX = (int)(size.Width * xPercent);
            int startY = (int)(size.Height * startYPercent);
            int endY = (int)(size.Height * endYPercent);

            var finger = new OpenQA.Selenium.Appium.Interactions.PointerInputDevice(PointerKind.Touch);
            var sequence = new ActionSequence(finger);

            sequence.AddAction(finger.CreatePointerMove(CoordinateOrigin.Viewport, startX, startY, TimeSpan.Zero));
            sequence.AddAction(finger.CreatePointerDown(PointerButton.TouchContact));
            sequence.AddAction(finger.CreatePause(TimeSpan.FromMilliseconds(100)));
            sequence.AddAction(finger.CreatePointerMove(CoordinateOrigin.Viewport, startX, endY, TimeSpan.FromMilliseconds(durationMs)));
            sequence.AddAction(finger.CreatePointerUp(PointerButton.TouchContact));

            Driver.PerformActions(new List<ActionSequence> { sequence });
        }

        protected void ScrollDownOneSwipe(int durationMs = 600)
        {
            // finger moves UP -> content scrolls DOWN
            SwipeVertical(startYPercent: 0.80, endYPercent: 0.20, xPercent: 0.50, durationMs: durationMs);
        }

        protected void ScrollUpOneSwipe(int durationMs = 600)
        {
            // finger moves DOWN -> content scrolls UP
            SwipeVertical(startYPercent: 0.20, endYPercent: 0.80, xPercent: 0.50, durationMs: durationMs);
        }

        protected IWebElement ScrollToKey(string key, int maxSwipes = 6, bool scrollDown = true, bool throwIfNotFound = true, int perSwipePauseMs = 250)
        {
            // 1) Try immediately
            var el = TryWaitForElementByKey(key, TimeSpan.FromSeconds(1));
            if (el != null) return el;

            // 2) Swipe + retry
            for (int i = 0; i < maxSwipes; i++)
            {
                if (scrollDown) ScrollDownOneSwipe();
                else ScrollUpOneSwipe();

                if (perSwipePauseMs > 0)
                    Thread.Sleep(perSwipePauseMs);

                el = TryWaitForElementByKey(key, TimeSpan.FromSeconds(1));
                if (el != null) return el;
            }

            if (throwIfNotFound)
                throw new NoSuchElementException($"Could not find '{key}' after {maxSwipes} swipe(s).");

            return null!;
        }

        protected void ScrollToKeyAndTap(string key, int maxSwipes = 6, bool scrollDown = true)
        {
            var el = ScrollToKey(key, maxSwipes, scrollDown, throwIfNotFound: true);
            el.Click();
        }

    }
}