using Microsoft.Azure.Cosmos.Linq;
using OpenQA.Selenium;

namespace VAA.EAPI.Automation.Core.UI.Helpers
{
    public class SmartElementFinder
    {
        private readonly IWebDriver _driver;

        public SmartElementFinder(IWebDriver driver)
        {
            _driver = driver;
        }

        // Try multiple locators in order
        public IWebElement Find(params By[] locators)
        {
            foreach (var locator in locators)
            {
                var elements = _driver.FindElements(locator);
                var el = elements.FirstOrDefault(e => e.Displayed && e.Enabled);
                if (el != null) return el;
            }
            throw new NoSuchElementException("Element not found using provided locators.");
        }

        // Fuzzy search for text in buttons or spans
        public IWebElement FindByFuzzyText(string text, double threshold = 0.6)
        {
            var elements = _driver.FindElements(By.XPath("//input //button | //span | //a")).ToList();

            IWebElement bestMatch = null;
            double bestScore = 0;

            foreach (var el in elements)
            {
                try
                {
                    // Access Displayed, Enabled, and Text inside try
                    if (!el.Displayed) continue;

                    var elementText = el.Text?.Trim();
                    if (string.IsNullOrEmpty(elementText)) continue;

                    double score = FuzzyMatchScore(text, elementText);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMatch = el;
                    }
                }
                catch (StaleElementReferenceException)
                {
                    // Element was removed, skip it
                    continue;
                }
            }

            if (bestMatch != null && bestScore >= threshold)
            {
                // Re-find the element to avoid stale reference
                var locator = By.XPath($"//button[normalize-space(text())='{bestMatch.Text}'] | " +
                                       $"//span[normalize-space(text())='{bestMatch.Text}'] | " +
                                       $"//a[normalize-space(text())='{bestMatch.Text}']");
                return _driver.FindElement(locator);
            }

            throw new NoSuchElementException($"No element found with fuzzy text matching '{text}'.");
        }

        private double FuzzyMatchScore(string a, string b)
        {
            // Simple Levenshtein similarity
            int distance = LevenshteinDistance(a.ToLower(), b.ToLower());
            return 1.0 - (double)distance / Math.Max(a.Length, b.Length);
        }

        private int LevenshteinDistance(string s, string t)
        {
            var n = s.Length;
            var m = t.Length;
            var d = new int[n + 1, m + 1];

            for (int i = 0; i <= n; i++) d[i, 0] = i;
            for (int j = 0; j <= m; j++) d[0, j] = j;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1,
                                                d[i, j - 1] + 1),
                                                d[i - 1, j - 1] + cost);
                }
            }
            return d[n, m];
        }
    }
}