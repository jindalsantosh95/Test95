using OpenQA.Selenium.Appium;
using OpenQA.Selenium;

namespace VAA.EAPI.Automation.Core.UI.Helpers
{
    public static class BrowserStackLogHelper
    {
        public static void Annotate(AppiumDriver driver, string message)
        {
            if (driver == null) throw new ArgumentNullException(nameof(driver));

            var script =
                "browserstack_executor: { " +
                "\"action\": \"annotate\", " +
                "\"arguments\": { " +
                    $"\"data\": \"{EscapeJson(message)}\" " +
                "} " +
                "}";

            ((IJavaScriptExecutor)driver).ExecuteScript(script);
        }

        private static string EscapeJson(string value) =>
            (value ?? string.Empty)
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n");
    }
}