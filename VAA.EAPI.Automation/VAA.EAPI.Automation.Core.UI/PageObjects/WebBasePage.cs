using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using VAA.EAPI.Automation.Core.UI.Helpers;

namespace VAA.EAPI.Automation.Core.UI.PageObjects
{
    public abstract class WebBasePage
    {
        protected readonly IWebDriver Driver;
        protected readonly SmartElementFinder Finder;

        protected WebBasePage(IWebDriver driver)
        {
            Driver = driver;
            Finder = new SmartElementFinder(driver);
        }

        // Optional: predefined locators for critical elements
        protected abstract Dictionary<string, By[]> ElementLocators { get; }


        public IWebElement Find(string key)
        {
            // 1️⃣ Try predefined locators first
            if (ElementLocators.TryGetValue(key, out var locators))
            {
                try
                {
                    var el = Finder.Find(locators);
                    if (el != null) return el;
                }
                catch (NoSuchElementException)
                {
                    // Ignore and try fallback
                }
            }

            // 2️⃣ Fallback: try common locator strategies
            var autoLocatorCandidates = new List<By>();

            // If key looks like a full XPath, use it directly
            if (key.TrimStart().StartsWith("//") || key.TrimStart().StartsWith("(/"))
            {
                try
                {
                    autoLocatorCandidates.Add(By.XPath(key));
                }
                catch (InvalidSelectorException)
                {
                    // Skip invalid XPath
                }
            }
            else
            {
                // 3️⃣ Add common locator strategies
                autoLocatorCandidates.AddRange(new[]
                {
                    By.Id(key),
                    By.Name(key),
                    By.CssSelector($"[data-cy='{key}']"),
                    By.CssSelector($"[data-testid='{key}']"),
                    By.CssSelector($"input[placeholder*='{key}']"),
                    By.XPath($"//*[@id='{key}']"),
                    By.XPath($"//input[contains(@aria-label,'{key}')]"),
                    By.XPath($"//input[@role='combobox' and contains(@placeholder,'{key}')]"),
                    By.XPath($"//button[contains(text(),'{key}')]"),
                    By.XPath($"//button[span[contains(text(),'{key}')]]"),
                    By.XPath($"//button[normalize-space(text())='{key}']"),
                    By.XPath($"//a[span[contains(text(),'{key}')]]"),
                    By.XPath($"//span[contains(text(),'{key}')]")
                });
            }

            try
            {
                return Finder.Find(autoLocatorCandidates.ToArray());
            }
            catch (NoSuchElementException)
            {
                // Final fallback: fuzzy search by text
                return Finder.FindByFuzzyText(key);
            }
        }
        public IWebElement WaitForVisible(string key, int timeoutSeconds = 60)
        {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
            
            //  Try predefined locators first
            if (ElementLocators.TryGetValue(key, out var locators))
            {
                try
                {
                    return wait.Until(driver => Finder.Find(locators));
                }
                catch (WebDriverTimeoutException)
                {
                    // Ignore and try fallback
                }
            }

            //  Fallback: try common locator strategies
            var autoLocatorCandidates = new List<By>();

            if (key.TrimStart().StartsWith("//") || key.TrimStart().StartsWith("(/"))
            {
                try
                {
                    autoLocatorCandidates.Add(By.XPath(key));
                }
                catch (InvalidSelectorException)
                {
                    // Skip invalid XPath
                }
            }
            else
            {
                autoLocatorCandidates.AddRange(new[]
                {
                    By.Id(key),
                    By.Name(key),
                    By.CssSelector($"[data-cy='{key}']"),
                    By.CssSelector($"[data-testid='{key}']"),
                    By.CssSelector($"input[placeholder*='{key}']"),
                    By.XPath($"//*[@id='{key}']"),
                    By.XPath($"//input[contains(@aria-label,'{key}')]"),
                    By.XPath($"//input[@role='combobox' and contains(@placeholder,'{key}')]"),
                    By.XPath($"//button[contains(text(),'{key}')]"),
                    By.XPath($"//button[span[contains(text(),'{key}')]]"),
                    By.XPath($"//button[normalize-space(text())='{key}']"),
                    By.XPath($"//a[span[contains(text(),'{key}')]]"),
                    By.XPath($"//span[contains(text(),'{key}')]")
                });
            }

            try
            {
                return wait.Until(driver => Finder.Find(autoLocatorCandidates.ToArray()));
            }
            catch (WebDriverTimeoutException)
            {
                //  Final fallback: fuzzy search by text with wait
                return wait.Until(driver => Finder.FindByFuzzyText(key));
            }
        }

        public void Click(string key)
        {
            WaitForVisible(key);
            Find(key).Click();
            MimicUserSleep();
        }

        public void EnterText(string key, string text)
        {
            var el = Find(key);
            el.Clear();
            el.SendKeys(text);
            MimicUserSleep();
        }

        public static void MimicUserSleep(int minMilliseconds = 200, int maxMilliseconds = 3000)
        {
            if (minMilliseconds > maxMilliseconds)
                throw new ArgumentException("Minimum sleep time cannot be greater than maximum sleep time.");

            var random = new Random();
            int sleepTime = random.Next(minMilliseconds, maxMilliseconds + 1);
            Thread.Sleep(sleepTime);
        }
    }
}
