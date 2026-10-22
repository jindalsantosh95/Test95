using AventStack.ExtentReports;
using OpenQA.Selenium;
using OpenQA.Selenium.Chromium;

namespace VAA.EAPI.Automation.Core.UI.Extensions
{
    /// <summary>
    /// Helper for capturing screenshots and embedding them in ExtentReports logs.
    /// Use at key checkpoints in UI tests to provide visual evidence of test progress.
    ///
    /// Usage:
    ///   driver.LogScreenshot(logger, "After entering email");
    ///   driver.LogScreenshot(logger, "Connected page", Status.Pass);
    ///   driver.LogScreenshotOnFailure(logger, "Payment page did not load");
    /// </summary>
    public static class ExtentScreenshotHelper
    {
        private static readonly string ScreenshotDir = Path.Combine(Directory.GetCurrentDirectory(), "Screenshots");

        /// <summary>
        /// Captures a screenshot and attaches it to the ExtentReports test log.
        /// </summary>
        /// <param name="driver">The WebDriver instance.</param>
        /// <param name="logger">The ExtentTest logger to attach the screenshot to.</param>
        /// <param name="stepDescription">Description of what the screenshot captures.</param>
        /// <param name="status">Log status level (default: Info).</param>
        public static void LogScreenshot(this IWebDriver driver, ExtentTest logger, string stepDescription, Status status = Status.Info)
        {
            if (driver == null)
            {
                logger.Log(Status.Warning, $"Cannot capture screenshot for '{stepDescription}': WebDriver is null");
                return;
            }

            try
            {
                Directory.CreateDirectory(ScreenshotDir);

                var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                var fileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.png";
                var filePath = Path.Combine(ScreenshotDir, fileName);

                screenshot.SaveAsFile(filePath);

                logger.Log(status, stepDescription,
                    MediaEntityBuilder.CreateScreenCaptureFromPath(filePath).Build());
            }
            catch (Exception ex)
            {
                logger.Log(Status.Warning, $"Failed to capture screenshot for '{stepDescription}': {ex.Message}");
            }
        }

        /// <summary>
        /// Captures a screenshot and logs it as a failure in the ExtentReports test log.
        /// Use in catch blocks or assertion failures.
        /// </summary>
        public static void LogScreenshotOnFailure(this IWebDriver driver, ExtentTest logger, string failureDescription)
        {
            driver.LogScreenshot(logger, failureDescription, Status.Fail);
        }

        /// <summary>
        /// Captures a screenshot and logs it as a pass in the ExtentReports test log.
        /// Use after successful assertions or key milestones.
        /// </summary>
        public static void LogScreenshotOnPass(this IWebDriver driver, ExtentTest logger, string passDescription)
        {
            driver.LogScreenshot(logger, passDescription, Status.Pass);
        }

        /// <summary>
        /// Captures console errors from the browser log and appends them to the ExtentReports test log.
        /// Only captures Severe-level entries (JS errors, failed resources).
        /// </summary>
        public static void LogConsoleErrors(this IWebDriver driver, ExtentTest logger)
        {
            try
            {
                var logs = driver.Manage().Logs.GetLog(LogType.Browser);
                var errors = logs.Where(l => l.Level >= LogLevel.Severe).ToList();

                if (errors.Any())
                {
                    logger.Log(Status.Warning, $"<b>Console Errors ({errors.Count}):</b>");
                    foreach (var error in errors)
                        logger.Log(Status.Warning, $"[{error.Timestamp:HH:mm:ss}] {error.Message}");
                }
                else
                {
                    logger.Log(Status.Info, "No console errors detected");
                }
            }
            catch (Exception ex)
            {
                logger.Log(Status.Warning, $"Failed to retrieve console logs: {ex.Message}");
            }
        }

        /// <summary>
        /// Captures network performance logs (requests, responses, failures) using Chrome's performance logging.
        /// Filters for failed network requests and logs them to the ExtentReports test log.
        /// Requires LogType.Performance to be enabled on ChromeOptions (SetLoggingPreference).
        /// </summary>
        public static void LogNetworkErrors(this IWebDriver driver, ExtentTest logger)
        {
            try
            {
                if (driver is not ChromiumDriver)
                {
                    logger.Log(Status.Info, "Network logging only supported on Chromium browsers");
                    return;
                }

                var perfLogs = driver.Manage().Logs.GetLog("performance");
                var failedRequests = new List<string>();

                foreach (var entry in perfLogs)
                {
                    var message = entry.Message;
                    if (message.Contains("Network.responseReceived") && (message.Contains("\"status\":4") || message.Contains("\"status\":5")))
                    {
                        failedRequests.Add(message);
                    }
                    else if (message.Contains("Network.loadingFailed"))
                    {
                        failedRequests.Add(message);
                    }
                }

                if (failedRequests.Any())
                {
                    logger.Log(Status.Warning, $"<b>Failed Network Requests ({failedRequests.Count}):</b>");
                    foreach (var req in failedRequests.Take(20))
                    {
                        // Extract just the URL and error from the full JSON log entry
                        try
                        {
                            var json = Newtonsoft.Json.Linq.JObject.Parse(req);
                            var url = json.SelectToken("$..request.url")?.ToString()
                                   ?? json.SelectToken("$..response.url")?.ToString()
                                   ?? "unknown URL";
                            var error = json.SelectToken("$..errorText")?.ToString()
                                     ?? json.SelectToken("$..response.status")?.ToString()
                                     ?? "unknown error";
                            logger.Log(Status.Warning, $"{url} - {error}");
                        }
                        catch
                        {
                            logger.Log(Status.Warning, req.Length > 500 ? req[..500] + "..." : req);
                        }
                    }
                }
                else
                {
                    logger.Log(Status.Info, "No failed network requests detected");
                }
            }
            catch (Exception ex)
            {
                logger.Log(Status.Warning, $"Failed to retrieve network logs: {ex.Message}");
            }
        }

        /// <summary>
        /// Captures screenshot, console errors, and network failures — use on test failure for full diagnostics.
        /// </summary>
        public static void LogFailureDiagnostics(this IWebDriver driver, ExtentTest logger, string failureDescription)
        {
            driver.LogScreenshotOnFailure(logger, failureDescription);
            driver.LogConsoleErrors(logger);
            driver.LogNetworkErrors(logger);
        }
    }
}
