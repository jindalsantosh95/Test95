using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web;
using AventStack.ExtentReports;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using Deque.AxeCore.Selenium;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.UI.Factory;
using VAA.EAPI.Automation.Core.Xunit;
using Deque.AxeCore.Commons;

namespace VAA.EAPI.Automation.Core.UI.Extensions
{
    public static class WebDriverExtensions
    {
        private static readonly ConditionalWeakTable<IWebDriver, DriverProcessRegistration> driverProcesses = new();

        private sealed class DriverProcessRegistration
        {
            public DriverProcessRegistration(int processId)
            {
                ProcessId = processId;
            }

            public int ProcessId { get; }
        }

        public static readonly Dictionary<string, IWebElement> Elements = null;

        private static IWebElement FindVisibleElement(IWebDriver driver, By locator)
        {
            try
            {
                IWebElement element = driver.FindElement(locator);
                return element.Displayed ? element : null;
            }
            catch (NoSuchElementException)
            {
                return null;
            }
            catch (StaleElementReferenceException)
            {
                return null;
            }
        }

        /// <summary>
        /// Injects custom HTTP headers into all outgoing requests for this browser session using
        /// Selenium's Network.AddRequestHandler. Headers are applied to every request because the
        /// HPP page calls multiple backend hosts (its own API, virginatlantic.com payment services, etc.)
        /// that all require them. Does not navigate — call Navigate().GoToUrl() separately.
        /// Throws <see cref="NotSupportedException"/> if the driver does not support Network interception.
        /// </summary>
        /// <param name="url">The target page URL (not used for request matching but retained for call-site clarity).</param>
        /// <param name="headers">Dictionary of header name/value pairs to inject.</param>
        public static void AddCustomHeaders(this IWebDriver driver, string url, Dictionary<string, string> headers)
        {
            if (driver.Manage().Network is not { } network)
                throw new NotSupportedException($"The driver ({driver.GetType().Name}) does not support Network interception. Use a browser that implements INetwork (e.g. ChromeDriver).");

            // The HPP page makes requests to multiple backend hosts (its own API, virginatlantic.com
            // payment services, etc.) that all require these headers. Since the set of hosts is not
            // predictable, apply to all requests without logging their credential values.
            network.AddRequestHandler(new NetworkRequestHandler()
            {
                RequestMatcher = _ => true,
                RequestTransformer = request =>
                {
                    foreach (KeyValuePair<string, string> header in headers)
                    {
                        if (request.Headers.ContainsKey(header.Key))
                            request.Headers[header.Key] = header.Value;
                        else
                            request.Headers.Add(header.Key, header.Value);
                    }
                    return request;
                }
            });

            network.StartMonitoring().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Generates a temporary Manifest V3 Chrome extension that injects custom headers using declarativeNetRequest.
        /// The extension only modifies main_frame requests matching the given URL pattern, so XHR/fetch calls
        /// made by the page are not affected — avoiding CORS preflight issues.
        ///
        /// Use this approach for non-headless Chrome where extensions can be loaded. For headless mode,
        /// use AddCustomHeaders(), as Chrome extensions cannot load in headless mode.
        ///
        /// Usage in BrowserFactory:
        ///   var extPath = WebDriverExtensions.CreateHeaderInjectorExtension(headers, "*enterprisepaymentshpp*");
        ///   options.AddArgument($"--disable-extensions-except={extPath}");
        ///   options.AddArgument($"--load-extension={extPath}");
        ///
        /// The extension directory is created in the system temp folder and can be cleaned up after the browser session.
        /// </summary>
        /// <param name="headers">Dictionary of header name/value pairs to inject.</param>
        /// <param name="urlPattern">URL pattern to match (e.g. "*enterprisepaymentshpp*"). Only main_frame requests matching this pattern will have headers injected.</param>
        /// <returns>Path to the generated extension directory.</returns>
        public static string CreateHeaderInjectorExtension(Dictionary<string, string> headers, string urlPattern)
        {
            string extDir = Path.Combine(Path.GetTempPath(), $"header_injector_{Guid.NewGuid():N}");
            Directory.CreateDirectory(extDir);

            List<object> rules = new List<object>();
            int ruleId = 1;
            foreach (KeyValuePair<string, string> header in headers)
            {
                rules.Add(new
                {
                    id = ruleId++,
                    priority = 1,
                    action = new
                    {
                        type = "modifyHeaders",
                        requestHeaders = new[] { new { header = header.Key, operation = "set", value = header.Value } }
                    },
                    condition = new
                    {
                        urlFilter = urlPattern,
                        resourceTypes = new[] { "main_frame" }
                    }
                });
            }

            File.WriteAllText(Path.Combine(extDir, "manifest.json"), Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                manifest_version = 3,
                name = "Test Header Injector",
                version = "1.0",
                permissions = new[] { "declarativeNetRequest" },
                declarative_net_request = new
                {
                    rule_resources = new[] { new { id = "headers", enabled = true, path = "rules.json" } }
                }
            }, Newtonsoft.Json.Formatting.Indented));

            File.WriteAllText(Path.Combine(extDir, "rules.json"), Newtonsoft.Json.JsonConvert.SerializeObject(rules, Newtonsoft.Json.Formatting.Indented));

            return extDir;
        }

        public static string GetElementText(this IWebDriver driver, string key, int timeout = 10)
        {
            IWebElement element = FindElement(driver, key, key, timeout);
            if (element.Text.Length > 0)
            {
                return element.Text;
            }
            else
            {
                throw new Exception($"Failed as located the {key}.");
            }
        }

        public static string GetAttributeTextValue(this IWebDriver driver, string key, string attributeName, int timeout = 10)
        {
            IWebElement element = FindElement(driver, key, key, timeout);
            if (element.GetAttribute(attributeName) != null)
            {
                int i = 0;
                while (true)
                {
                    i++;
                    try
                    {
                        if (element.GetAttribute(attributeName) != string.Empty)
                        {
                            return element.GetAttribute(attributeName);
                        }
                    }
                    catch (Exception) when (i < 10) { Thread.Sleep(1000); }
                    Thread.Sleep(1000);
                    if (i >= 10)
                        return element.GetAttribute(attributeName);
                }
            }
            else
            {
                // This is needed as checked only exists when checked
                if (attributeName == "checked")
                { return false.ToString(); }

                throw new Exception($"Couldn't find the attribute {attributeName} on the element {key}.");
            }
        }

        public static void MoveToElement(this IWebDriver driver, IWebElement webElement)
        {
            try
            {
                Actions actions = new Actions(driver);
                actions.MoveToElement(webElement);
                actions.Perform();
            }
            catch (Exception)
            {

            }
        }

        public static IWebElement FindElement(this IWebDriver driver, string key, string elementDescription, int timeout = 10)
        {
            IWebElement element = TryFindElement(driver, key, timeout);

            if (element == null)
                throw new NoSuchElementException($"Failed to locate the {elementDescription}.");
            return element;
        }

        public static IWebElement TryFindElement(this IWebDriver driver, string key, int timeout = 10)
        {
            IWebElement element = WaitForElementToBeVisible(driver, key, timeout);

            if (element != null)
            {
                MoveToElement(driver, element);
            }
            return element;
        }

        public static IWebElement WaitForElementToBeVisible(this IWebDriver driver, string key, int timeout = 10)
        {
            WebDriverWait wait = new WebDriverWait(driver, new TimeSpan(0, 0, timeout));
            try
            {
                if (key.Has("/"))
                {
                    return wait.Until(d => FindVisibleElement(d, By.XPath(key)));
                }
                else
                {
                    return wait.Until(d => FindVisibleElement(d, By.CssSelector(key)));
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static IEnumerable<IWebElement> FindElements(this IWebDriver driver, string key, string elementDescription, int timeout = 10)
        {
            IEnumerable<IWebElement> elements = WaitForElementsToBeVisible(driver, key, timeout);

            return elements.Any() ? elements : throw new NoSuchElementException($"Failed to locate the {elementDescription}.");
        }

        public static IEnumerable<IWebElement> WaitForElementsToBeVisible(this IWebDriver driver, string key, int timeout = 10)
        {
            WebDriverWait wait = new WebDriverWait(driver, new TimeSpan(0, 0, timeout));
            try
            {
                By locator = key.Has("/") ? By.XPath(key) : By.CssSelector(key);
                return wait.Until(d =>
                {
                    IReadOnlyCollection<IWebElement> elements = d.FindElements(locator);
                    return elements.Count > 0 && elements.All(element => element.Displayed) ? elements : null;
                });
            }
            catch (Exception)
            {
                return Enumerable.Empty<IWebElement>();
            }
        }

        public static IWebElement FindElementAndClick(this IWebDriver driver, string key, string elementDescription, int timeout = 10)
        {
            IWebElement element = TryFindElementAndClick(driver, key, elementDescription, timeout);

            if (element == null)
                throw new NoSuchElementException($"Failed to locate the {elementDescription}.  Browser Errors logs: {BrowserFactory.ReturnBrowserLogs(driver)}");
            return element;
        }

        public static IWebElement TryFindElementAndClick(this IWebDriver driver, string key, string elementDescription, int timeout = 10)
        {
            IWebElement element = TryFindElement(driver, key, timeout);
            if (element == null)
                return null;

            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            const int maxAttempts = 10;

            for (int i = 1; i <= maxAttempts; i++)
            {
                try
                {
                    element.Click();
                    return element;
                }
                catch (StaleElementReferenceException) when (i < maxAttempts)
                {
                    element = TryFindElement(driver, key, timeout);
                }
                catch (ElementClickInterceptedException) when (i < maxAttempts)
                {
                    // Try to bring it into view / hover etc.
                    MoveToElement(driver, element);

                    // Optional small pause for animations/overlays to clear
                    Thread.Sleep(150);

                    // If we’re late in retries, try JS click as fallback
                    if (i >= 7)
                    {
                        try
                        {
                            element = TryFindElement(driver, key, timeout); // re-find before JS click
                            js.ExecuteScript("arguments[0].click();", element);
                            return element;
                        }
                        catch (Exception) when (i < maxAttempts) { /* keep looping */ }
                    }
                }
                catch (ElementNotInteractableException) when (i < maxAttempts)
                {
                    Thread.Sleep(250);

                    // Try JS click earlier for "not interactable" cases (often offscreen/hidden overlay)
                    if (i >= 5)
                    {
                        try
                        {
                            element = TryFindElement(driver, key, timeout);
                            js.ExecuteScript("arguments[0].scrollIntoView({block:'center'});", element);
                            js.ExecuteScript("arguments[0].click();", element);
                            return element;
                        }
                        catch (Exception) when (i < maxAttempts) { /* keep looping */ }
                    }
                }
            }

            // If we get here, we failed all attempts
            throw new WebDriverException($"Failed to click element '{elementDescription}' (key: {key}) after {maxAttempts} attempts.");
        }

        public static IWebElement FindElementAndDoubleClick(this IWebDriver driver, string key, string elementDescription, int timeout = 10)
        {
            IWebElement element = FindElement(driver, key, elementDescription);
            if (element != null)
            {
                int i = 0;
                while (true)
                {
                    i++;
                    try
                    {
                        new Actions(driver).DoubleClick(element).Perform();
                        break;
                    }
                    catch (ElementClickInterceptedException) when (i < 10)
                    {
                        Thread.Sleep(1000);
                    }
                }

                return element;
            }
            else
            {
                throw new NoSuchElementException($"Failed to click on the {elementDescription}.");
            }
        }

        /// <summary>
        /// This only works with html 'select' elements.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="elementDescription"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static IWebElement FindElementAndSelectFromDropdown(this IWebDriver driver, string key, string data, string elementDescription, int timeout = 10)
        {
            SelectElement element = new SelectElement(FindElement(driver, key, elementDescription, timeout));
            if (element != null)
            {
                int i = 0;
                while (true)
                {
                    i++;
                    try
                    {
                        element.SelectByText(data);
                        break;
                    }
                    catch (NoSuchElementException) when (i < 10)
                    {
                        Thread.Sleep(1000);
                    }
                }
                return element.WrappedElement;
            }
            else
            {
                throw new Exception($"Failed to select the value '{data}' from the {elementDescription}.");
            }
        }

        public static IWebElement FindElementAndEnterText(this IWebDriver driver, string key, string data, string elementDescription, int timeout = 10, bool disableClearStep = false)
        {
            IWebElement element = FindElement(driver, key, elementDescription, timeout);
            int i = 0;
            if (element != null)
            {
                while (i < 5)
                {
                    i++;
                    try
                    {
                        element.Click();
                        break;
                    }
                    catch (ElementClickInterceptedException)
                    {
                        Thread.Sleep(1000);
                    }
                    catch (StaleElementReferenceException)
                    {
                        Thread.Sleep(1000);
                    }
                }

                if (!disableClearStep)
                {
                    element.Clear();
                }

                element.SendKeys(data);
                return element;
            }
            else
            {
                throw new Exception($"Failed to enter the value '{data}' into the {elementDescription}.");
            }
        }

        public static string[] TryFindAllElements(this IWebDriver driver, string key, string elementDescription, int timeout = 10)
        {
            ReadOnlyCollection<IWebElement> elements = driver.FindElements(By.XPath(key));

            return elements?.Select(x => x.Text).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray() ?? Array.Empty<string>();
        }

        public static bool WaitForElementToDissapear(this IWebDriver driver, string key, int timeout = 10)
        {
            WebDriverWait wait = new WebDriverWait(driver, new TimeSpan(0, 0, timeout));
            try
            {
                By path;

                if (key.Has("/"))
                    path = By.XPath(key);
                else
                    path = By.CssSelector(key);

                return wait.Until((IWebDriver driver) =>
                {
                    try
                    {
                        IWebElement element = driver.FindElement(By.XPath(key));

                        return !(element?.Displayed ?? false);
                    }
                    catch (StaleElementReferenceException)
                    {
                        return true;
                    }
                });
            }
            catch (Exception)
            {
                return true;
            }
        }

        public static void CheckAccessibility(this IWebDriver driver, ExtentTest logger, IWebElement webElement = null)
        {
            // Ensure async script timeout is sufficient for axe-core analysis (RemoteWebDriver defaults can be very low)
            try
            { driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(30); }
            catch (WebDriverException) { /* some remote drivers don't support timeout configuration */ }

            AxeResult axeResult = webElement == null ? driver.Analyze() : driver.Analyze(webElement);

            foreach (AxeResultItem violation in axeResult.Violations)
            {
                string help = HttpUtility.HtmlEncode(violation.Help);
                string html = HttpUtility.HtmlEncode(violation.Nodes.FirstOrDefault().Html);
                string url = $"<a target=\"_blank\" href=\"{violation.HelpUrl}\">{violation.HelpUrl}</a>";

                logger.Log(Status.Info, $"[Accessibility] {driver.Url} Violation:- {help}.   Html:- {html},   Help Url:- {url}");
            }

            if (axeResult.Violations.Length == 0)
                logger.Log(Status.Pass, $"No accessibility violations found on: {driver.Url}");
            else
                logger.Log(Status.Pass, $"Accessibility scan complete on: {driver.Url} — {axeResult.Violations.Length} violation(s) logged above as info");

            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript($"window.scrollTo(0, 0)");
        }

        public static void HumanWait(int waitTime)
        {
            Thread.Sleep(waitTime);
        }

        public static IWebElement SwitchToAFrame(this IWebDriver driver, string key, int timeout = 10)
        {
            WebDriverWait wait = new WebDriverWait(driver, new TimeSpan(0, 0, timeout));

            try
            {
                return wait.Until(d =>
                {
                    IWebElement frame = FindVisibleElement(d, By.Id(key));
                    if (frame == null)
                        return null;

                    d.SwitchTo().Frame(frame);
                    return frame;
                });
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static ReadOnlyCollection<IWebElement> WaitForElementToBePresent(this IWebDriver driver, string key, int timeout = 10)
        {
            WebDriverWait wait = new WebDriverWait(driver, new TimeSpan(0, 0, timeout));
            try
            {
                return wait.Until(d =>
                {
                    ReadOnlyCollection<IWebElement> elements = d.FindElements(By.XPath(key));
                    return elements.Count > 0 ? elements : null;
                });
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static IWebElement WaitForElementToBeClickable(this IWebDriver driver, string key, int timeout = 30)
        {
            WebDriverWait wait = new WebDriverWait(driver, new TimeSpan(0, 0, timeout));
            try
            {
                return wait.Until(d =>
                {
                    IWebElement element = FindVisibleElement(d, By.XPath(key));
                    return element?.Enabled == true ? element : null;
                });
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static void WaitForRedirectUrl(this IWebDriver driver, string expectedUrl, int timeoutInSeconds)
        {
            try
            {
                // Define the WebDriverWait
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));

                // Wait for the URL to be the expected one
                wait.Until(driver =>
                {
                    // Check if the current URL matches the expected one
                    return driver.Url.Has(expectedUrl);
                });
            }
            catch (WebDriverTimeoutException)
            {
                // Timeout occurred, throw an error
                throw new Exception($"Expected URL '{expectedUrl}' was not loaded within {timeoutInSeconds} seconds. URL found was {driver.Url}");
            }
        }

        public static bool GetScreenshot(this IWebDriver driver, string fileName)
        {
            if (driver is null)
                return false;

            Screenshot screenShot = ((ITakesScreenshot)driver).GetScreenshot();

            return TestContext.Current.Attach(screenShot.AsByteArray, fileName);
        }

        public static void RegisterDriverProcess(this IWebDriver driver, int processId)
        {
            if (driver is null)
                throw new ArgumentNullException(nameof(driver));
            if (processId <= 0)
                throw new ArgumentOutOfRangeException(nameof(processId));

            driverProcesses.Remove(driver);
            driverProcesses.Add(driver, new DriverProcessRegistration(processId));
            Console.WriteLine($"Registered Selenium ChromeDriver process {processId} for owned cleanup.");
        }

        public static void ForgetDriverProcess(this IWebDriver driver)
        {
            if (driver is not null)
                driverProcesses.Remove(driver);
        }

        public static int GetDriverPid(this IWebDriver driver)
        {
            if (driverProcesses.TryGetValue(driver, out DriverProcessRegistration registration))
                return registration.ProcessId;

            Console.WriteLine(
                "WARNING: Selenium driver has no explicit process registration; "
                + "attempting legacy PID discovery.");
            ICommandExecutor executor = ((WebDriver)driver).CommandExecutor;
            System.Reflection.FieldInfo serviceField = executor.GetType().GetField(
                "service",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (serviceField is null || serviceField.GetValue(executor) is not DriverService service)
            {
                throw new InvalidOperationException(
                    $"Unable to discover the Selenium driver service from {executor.GetType().FullName}.");
            }

            return service.ProcessId;
        }

        public static int[] GetChildProcessIds(this IWebDriver driver)
        {
            try
            {
                int processId = driver.GetDriverPid();

                if (processId <= 0)
                {
                    return Array.Empty<int>();
                }

                // Recursively collect all descendant process IDs
                List<int> descendants = new List<int>();
                try
                {
                    CollectDescendantPids(processId, descendants);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(
                        $"WARNING: Unable to enumerate descendants for registered Selenium process "
                        + $"{processId}: {exception.GetType().Name}: {exception.Message}. "
                        + "The registered ChromeDriver process will still be cleaned up.");
                }

                return descendants
                    .Prepend(processId)
                    .Distinct()
                    .ToArray();
            }
            catch (Exception exception)
            {
                Console.WriteLine(
                    $"WARNING: Unable to capture Selenium-owned browser processes: "
                    + $"{exception.GetType().Name}: {exception.Message}");
                return Array.Empty<int>();
            }
        }

        private static void CollectDescendantPids(int parentPid, List<int> collected)
        {
            if (parentPid <= 0)
            {
                return;
            }

            StringBuilder stringBuilder = new StringBuilder();

            Process process = new()
            {
                StartInfo = new()
                {
                    FileName = "wmic",
                    Arguments = $"process where (ParentProcessId={parentPid}) get ProcessId",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                },
            };

            process.OutputDataReceived += (sender, args) => stringBuilder.AppendLineIf(x => !args.Data.IsNullOrWhiteSpace(), args.Data);
            process.ErrorDataReceived += (sender, args) => { };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            int[] childPids = stringBuilder.ToString()
                                           .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                                           .Skip(1)
                                           .Select(x => x.TryParse(out int pid) ? pid : -1)
                                           .Where(x => x > 4)
                                           .ToArray();

            foreach (int pid in childPids)
            {
                if (!collected.Contains(pid))
                {
                    collected.Add(pid);
                    CollectDescendantPids(pid, collected);
                }
            }
        }

        public static void TerminateChildProcesses(this IWebDriver driver)
        {
            int[] childPids = driver.GetChildProcessIds()
                                    .ToArray();

            TerminateOwnedProcesses(childPids);
        }

        public static void TerminateOwnedProcessTree(int processId)
        {
            if (processId <= 4 || processId == Environment.ProcessId)
                return;

            List<int> processIds = [processId];
            try
            {
                CollectDescendantPids(processId, processIds);
            }
            catch (Exception exception)
            {
                Console.WriteLine(
                    $"WARNING: Unable to enumerate descendants for failed Selenium process "
                    + $"{processId}: {exception.GetType().Name}: {exception.Message}.");
            }
            TerminateOwnedProcesses(processIds);
        }

        public static void TerminateOwnedProcesses(IEnumerable<int> processIds)
        {
            int currentProcessId = Environment.ProcessId;
            int[] ownedProcessIds = processIds
                .Where(processId => processId > 4 && processId != currentProcessId)
                .Distinct()
                .ToArray();

            if (ownedProcessIds.Length == 0)
            {
                Console.WriteLine("No registered Selenium browser processes required forced cleanup.");
                return;
            }

            Process process = new()
            {
                StartInfo = new()
                {
                    FileName = "taskkill",
                    Arguments = $"/f /t {string.Join(' ', ownedProcessIds.Select(x => $"/pid {x}"))}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                },
            };

            process.OutputDataReceived += (sender, args) => { };
            process.ErrorDataReceived += (sender, args) => { };

            if (process.Start())
            {
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();
                Console.WriteLine(
                    $"Forced cleanup completed for {ownedProcessIds.Length} registered Selenium "
                    + $"browser process(es) with exit code {process.ExitCode}.");
            }
        }

        public static void WaitForDomToBeQuiet(this IWebDriver driver, int timeoutSeconds = 10, int quietMs = 300)
        {
            driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(timeoutSeconds);

            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

            js.ExecuteAsyncScript(@"
        const done = arguments[arguments.length - 1];
        const quietMs = arguments[0];

        let timer = null;
        const observer = new MutationObserver(() => {
            if (timer) clearTimeout(timer);
            timer = setTimeout(() => {
                observer.disconnect();
                done(true);
            }, quietMs);
        });

        observer.observe(document.body, { childList: true, subtree: true });

        // In case nothing changes, resolve after quietMs anyway
        timer = setTimeout(() => {
            observer.disconnect();
            done(true);
        }, quietMs);
    ", quietMs);
        }
    }
}
