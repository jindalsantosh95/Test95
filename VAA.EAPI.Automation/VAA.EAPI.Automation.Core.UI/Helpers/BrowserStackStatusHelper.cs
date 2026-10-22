using OpenQA.Selenium.Appium;
using OpenQA.Selenium;

namespace VAA.EAPI.Automation.Core.UI.Helpers
{
    public static class BrowserStackStatusHelper
    {
        /// <summary>
        /// Marks the current BrowserStack session as passed or failed using the JS executor API.
        /// </summary>
        /// <param name="driver">The AppiumDriver instance for this session.</param>
        /// <param name="status">"passed" or "failed".</param>
        /// <param name="reason">A short human-readable reason that appears in the BrowserStack UI.</param>
        public static void SetSessionStatus(AppiumDriver driver, string status, string reason)
        {
            if (driver == null) throw new ArgumentNullException(nameof(driver));
            if (string.IsNullOrWhiteSpace(status)) throw new ArgumentException("Status must be provided.", nameof(status));

            // Just guard to avoid accidental typos
            status = status.ToLowerInvariant();
            if (status != "passed" && status != "failed")
                throw new ArgumentException("Status must be either \"passed\" or \"failed\".", nameof(status));

            // BrowserStack JS executor API
            var script =
                "browserstack_executor: { " +
                "\"action\": \"setSessionStatus\", " +
                "\"arguments\": { " +
                    $"\"status\": \"{EscapeJson(status)}\", " +
                    $"\"reason\": \"{EscapeJson(reason ?? string.Empty)}\" " +
                "} " +
                "}";

            ((IJavaScriptExecutor)driver).ExecuteScript(script);
        }

        // Very small JSON string escaper to keep the reason safe in the script
        private static string EscapeJson(string value)
        {
            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n");
        }
    }
}
