using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Safari;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.UI.Extensions;
using Application = VAA.EAPI.Automation.Core.UI.Constants.Application;
using Process = System.Diagnostics.Process;

namespace VAA.EAPI.Automation.Core.UI.Factory
{
    public static class BrowserFactory
    {
        public static IReadOnlyDictionary<string, string> HostedPaymentRequestHeaders
        {
            get
            {
                string vaaDigitalFirstToken = ConfigReader.Get("HppVaaDigitalFirstToken");
                string deltaEnterprisePaymentsToken = ConfigReader.Get("HppDeltaEnterprisePaymentsToken");

                if (vaaDigitalFirstToken.IsNullOrWhiteSpace() || deltaEnterprisePaymentsToken.IsNullOrWhiteSpace())
                {
                    throw new InvalidOperationException(
                        "HPP request-header credentials are missing. Configure HppVaaDigitalFirstToken "
                        + "and HppDeltaEnterprisePaymentsToken in the runtime environment.");
                }

                return new Dictionary<string, string>
                {
                    { "X-Vaa_digitalfirst", vaaDigitalFirstToken },
                    { "X-Delta_enterprisepaymentsHPP_Test", deltaEnterprisePaymentsToken }
                };
            }
        }

        public enum BrowserType
        {
            Chrome,
            FireFox,
            Edge,
            BrowserStack
        }

        public static IWebDriver GetBrowser(BrowserType browser, string url, bool headless = false, bool customHeaders = false,
            string bsBrowser = "Chrome", string bsBrowserVersion = "latest", string bsOs = "Windows", string bsOsVersion = "11", string bsDevice = null, string bsSessionName = null)
        {
            IWebDriver driver = null;
            int maxRetries = 5;
            int attempt = 0;

            while (driver == null)
            {
                attempt++;
                try
                {
                    switch (browser)
                    {
                        case BrowserType.Chrome:

                            ChromeOptions options = new();
                            options.AddArgument("--incognito");
                            options.AddArgument("--disable-gpu"); // Helps in headless mode
                            options.AddArgument("--window-size=1920,1080"); // Ensures consistent viewport
                            options.AddExcludedArgument("enable-automation"); // Removes automation banner
                            options.AddAdditionalOption("useAutomationExtension", false); // Disables automation extension
                            options.SetLoggingPreference(LogType.Browser, LogLevel.All);
                            options.SetLoggingPreference("performance", LogLevel.All);

                            if (headless)
                            {
                                options.AddArgument("--headless=new");
                            }

                            if (url.Has("powerapps"))
                            {
                                options.DebuggerAddress = Application.DebuggingPort;

                                Process process = new()
                                {
                                    StartInfo = new()
                                    {
                                        FileName = ConfigReader.Get("PathForChrome"),
                                        Arguments = Application.DebuggingArguments
                                    }
                                };

                                process.Start();
                            }

                            // Create the service explicitly so cleanup owns the exact ChromeDriver
                            // process without reflecting over Selenium's private implementation.
                            ChromeDriverService chromeDriverService = ChromeDriverService.CreateDefaultService();
                            chromeDriverService.HideCommandPromptWindow = true;
                            try
                            {
                                ChromeDriver chromeDriver = new(chromeDriverService, options, TimeSpan.FromMinutes(2));
                                chromeDriver.RegisterDriverProcess(chromeDriverService.ProcessId);
                                driver = chromeDriver;
                            }
                            catch
                            {
                                WebDriverExtensions.TerminateOwnedProcessTree(chromeDriverService.ProcessId);
                                chromeDriverService.Dispose();
                                throw;
                            }

                            break;
                        case BrowserType.FireFox:

                            FirefoxDriverService firefoxDriverService = FirefoxDriverService.CreateDefaultService();

                            driver = new FirefoxDriver(firefoxDriverService, new FirefoxOptions(), TimeSpan.FromMinutes(2));

                            break;
                        case BrowserType.Edge:

                            EdgeOptions edgeOptions = new EdgeOptions();

                            if (headless)
                            {
                                edgeOptions.AddArgument("--headless=new");
                            }

                            driver = new EdgeDriver(edgeOptions);

                            break;

                        case BrowserType.BrowserStack:

                            string bsUser = ConfigReader.Get("BROWSERSTACK_USERNAME");
                            string bsKey = ConfigReader.Get("BROWSERSTACK_ACCESS_KEY");

                            if (string.IsNullOrWhiteSpace(bsUser) || string.IsNullOrWhiteSpace(bsKey))
                                throw new InvalidOperationException("Missing BrowserStack credentials. Set BROWSERSTACK_USERNAME and BROWSERSTACK_ACCESS_KEY in config.");

                            Dictionary<string, object> bsCaps = new Dictionary<string, object>
                            {
                                ["userName"] = bsUser,
                                ["accessKey"] = bsKey,
                                ["projectName"] = ConfigReader.Get("BROWSERSTACK_PROJECT") ?? "EAPI Automation",
                                ["buildName"] = ConfigReader.Get("BROWSERSTACK_BUILD") ?? $"Build-{DateTime.Now:yyyyMMdd-HHmmss}",
                                ["sessionName"] = bsSessionName ?? "WiFi Portal Tests",
                                ["os"] = bsOs,
                                ["osVersion"] = bsOsVersion
                            };

                            // For mobile devices (e.g. iPhone, Samsung Galaxy)
                            if (!string.IsNullOrWhiteSpace(bsDevice))
                                bsCaps["deviceName"] = bsDevice;

                            DriverOptions bsDriverOptions = bsBrowser.ToLowerInvariant() switch
                            {
                                "firefox" => new FirefoxOptions(),
                                "edge" => new EdgeOptions(),
                                "safari" => new SafariOptions(),
                                _ => new ChromeOptions()
                            };

                            bsDriverOptions.BrowserVersion = bsBrowserVersion;
                            bsDriverOptions.AddAdditionalOption("bstack:options", bsCaps);

                            string hubUrl = ConfigReader.Get("BROWSERSTACK_HUB_URL") ?? "https://hub-cloud.browserstack.com/wd/hub";
                            driver = new RemoteWebDriver(new Uri(hubUrl), bsDriverOptions);

                            break;
                    }
                }
                catch (Exception ex) when (ex is System.Net.Sockets.SocketException or WebDriverException or InvalidOperationException)
                {
                    driver = null;
                    Thread.Sleep(1000 * attempt); // Back off between retries
                    if (attempt >= maxRetries)
                        throw new WebDriverException($"Failed to start browser after {maxRetries} attempts. Last error: {ex.Message}", ex);
                }
            }

            try
            {
                driver.Manage().Window.Maximize();
            }
            catch (Exception) { }

            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(120);

            if (customHeaders)
            {
                try
                {
                    driver.AddCustomHeaders(url, HostedPaymentRequestHeaders.ToDictionary(header => header.Key, header => header.Value));
                    Console.WriteLine("Custom headers interceptor started successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"WARNING: Custom headers injection failed — HPP requests will be rejected. {ex.GetType().Name}: {ex.Message}");
                    throw;
                }
            }

            try
            {
                driver.Navigate().GoToUrl(url);
            }
            catch (Exception) { }

            // call to colate any child process Ids
            _ = driver.GetChildProcessIds();

            return driver;
        }

        [Obsolete("Marked obsolete in favour of DriverCleanUp")]
        public static void CloseBrowser(this IWebDriver driver)
        {
            using (driver)
            {
                try
                {
                    driver.Manage().Network.StopMonitoring();
                    driver.Manage().Cookies.DeleteAllCookies();
                    driver.Close();
                    driver.Quit();
                }
                catch { }
            }
        }

        public static void DriverCleanUp(this IWebDriver driver)
        {
            if (driver == null)
                return; // Avoid null reference exceptions.

            int[] ownedProcessIds = driver.GetChildProcessIds();
            Console.WriteLine(
                $"Closing Selenium session with {ownedProcessIds.Length} owned browser process(es): "
                + $"{string.Join(", ", ownedProcessIds)}");

            try
            {
                IOptions manage = driver.Manage();

                // Stop network monitoring (if supported and started).
                try
                {
                    manage.Network?.StopMonitoring();
                }
                catch (NotImplementedException)
                {
                    // Ignore if Network API is not implemented.
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error stopping network monitoring: {ex.Message}");
                }

                // Clear cookies (optional, for cleanup).
                try
                {
                    manage.Cookies.DeleteAllCookies();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting all cookies: {ex.Message}");
                }

                // Quit WebDriver (ensures all processes are terminated).
                driver.Quit();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while closing browser: {ex.Message}");
            }
            finally
            {
                WebDriverExtensions.TerminateOwnedProcesses(ownedProcessIds);
                driver.ForgetDriverProcess();

                try
                {
                    driver.Quit();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Final Selenium Quit failed: {ex.Message}");
                }

                try
                {
                    driver.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Final Selenium Dispose failed: {ex.Message}");
                }
            }
        }

        public static string ReturnBrowserLogs(IWebDriver driver)
        {
            // Retrieve browser logs (console errors)
            System.Collections.ObjectModel.ReadOnlyCollection<LogEntry> logs = driver.Manage().Logs.GetLog(LogType.Browser);
            List<string> errorMessages = new List<string>();

            // Loop through the logs and extract the severe errors
            foreach (LogEntry log in logs)
            {
                if (log.Level == LogLevel.Severe)
                {
                    errorMessages.Add($"Console Error: {log.Message}");
                }
            }

            // Join the errors into a single string
            return string.Join("\n", errorMessages);
        }
    }
}
