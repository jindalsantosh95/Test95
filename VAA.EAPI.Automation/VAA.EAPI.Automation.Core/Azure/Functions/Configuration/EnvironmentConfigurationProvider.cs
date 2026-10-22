using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using VAA.EAPI.Automation.Core.Diagnostics;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;

namespace VAA.EAPI.Automation.Core.Azure.Functions.Configuration
{
    public sealed class EnvironmentConfigurationProvider : ConfigurationProvider
    {
        public static readonly string Uri = "Configuration:Kudo:Uri";
        public static readonly string[] ExcludedConfig =
        [
            "deployment_branch",
            "SCM_TRACE_LEVEL",
            "SCM_COMMAND_IDLE_TIMEOUT",
            "SCM_LOGSTREAM_TIMEOUT",
            "SCM_BUILD_ARGS",
            "aspnet:PortableCompilationOutput",
            "aspnet:PortableCompilationOutputSnapshotType",
            "aspnet:DisableFcnDaclRead",
            "SCM_GIT_USERNAME",
            "SCM_GIT_EMAIL",
            "webpages:Version",
            "webpages:Enabled",
            "webactivator:assembliesToScan",
            "FUNCTIONS_EXTENSION_VERSION",
            "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING",
            "WEBSITE_AUTH_AUTO_AAD",
            "WEBSITE_AUTH_LOGOUT_PATH",
            "WEBSITE_SITE_NAME",
            "WEBSITE_RUN_FROM_PACKAGE",
            "ScmType",
            "FUNCTIONS_WORKER_RUNTIME",
            "WEBSITE_CONTENTSHARE",
            "WEBSITE_SLOT_NAME",
            "REMOTEDEBUGGINGVERSION",
            "WEBSITE_AUTH_ENABLED"
        ];

        public const string MsEdge = @"Microsoft\Edge\Application\msedge.exe";
        public const string Chrome = @"Google\Chrome\Application\chrome.exe";
        public const string ChromiumDataDirectory = @"c:\temp\chromium";

        public override void Load()
        {
            Uri uri = ConfigReader.Try<Uri>(Uri);

            if (uri is null)
                return;

            Trace<EnvironmentConfigurationProvider> trace = new();

            string chromePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), Chrome);
            string edgePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), MsEdge);

            if (!File.Exists(chromePath))
                chromePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), Chrome);

            if (!File.Exists(edgePath))
                edgePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), MsEdge);

            bool isChromeInstalled = File.Exists(chromePath);
            bool isEdgeInstalled = File.Exists(edgePath);

            trace.Debug.WriteLine($"isEdgeInstalled: {isEdgeInstalled}, isChromeInstalled: {isChromeInstalled}");

            if (!isEdgeInstalled && !isChromeInstalled)
                return;

            Process[] currentProcesses = Process.GetProcesses();

            bool isChromeRunning = currentProcesses.Any(p =>
            {
                try
                {
                    return p.StartInfo.FileName.EndsWith("chrome.exe");
                }
                catch { }

                return false;
            });
            bool isEdgeRunning = currentProcesses.Any(p =>
            {
                try
                {
                    return p.StartInfo.FileName.EndsWith("msedge.exe");
                }
                catch { }

                return false;
            });

            bool canUseChrome = isChromeInstalled && !isChromeRunning;
            bool canUseEdge = isEdgeInstalled && !isEdgeRunning;

            if (!canUseChrome && !canUseEdge)
            {
                trace.Debug.WriteLine($"canUseEdge: {canUseEdge}, canUseChrome: {canUseChrome}. ");
                trace.Debug.WriteLine("If both are installed and open then please close one version of the browser, If one browser is installed and open then please close all instance(s). If you are unable to close your browser instance(s) then your local settings will be loaded and must be complete.");

                return;
            }

            string filePath = canUseChrome ? chromePath : canUseEdge ? edgePath : string.Empty;

            if (filePath.IsNullOrWhiteSpace())
                return;

            bool chromiumDataDirectoryExists = Directory.Exists(ChromiumDataDirectory);

            if (!chromiumDataDirectoryExists)
            {
                try
                {
                    Directory.CreateDirectory(ChromiumDataDirectory);
                }
                catch { }

                trace.Diagnostics.LogInformation($"{{0}}> Data directory, {ChromiumDataDirectory}, initialized in {{2}}ms, 30 second delay has been added to allow for the sign in process into azure.");
            }

            int debuggingPort = new Random().Next(2048, 63532);
            Process process = new Process();

            process.StartInfo.FileName = filePath;
            process.StartInfo.Arguments = $"--remote-debugging-port={debuggingPort} --user-data-dir={ChromiumDataDirectory} --disable-dev-shm-usage --disable-gpu";
            process.StartInfo.UseShellExecute = true;

            process.Start();

            trace.Diagnostics.LogInformation($"{{0}}> Launching chrome browser {filePath} {process.StartInfo.Arguments} executed in {{2}}ms");

            IWebDriver webDriver = default;

            if (canUseChrome)
            {
                ChromeOptions chromeOptions = new ChromeOptions()
                {
                    DebuggerAddress = $"127.0.0.1:{debuggingPort}"
                };

                chromeOptions.AddArgument("--no-sandbox");
                //chromeOptions.AddArgument("--headless");
                chromeOptions.AddArgument("--disable-dev-shm-usage");

                webDriver = new ChromeDriver(ChromeDriverService.CreateDefaultService(), chromeOptions);
            }
            else if (canUseEdge)
            {
                EdgeOptions edgeOptions = new EdgeOptions()
                {
                    DebuggerAddress = $"127.0.0.1:{debuggingPort}",
                };

                edgeOptions.AddArgument("--no-sandbox");
                //edgeOptions.AddArgument("--headless");
                edgeOptions.AddArgument("--disable-dev-shm-usage");

                webDriver = new EdgeDriver(EdgeDriverService.CreateDefaultService(), edgeOptions);
            }

            webDriver.Navigate().GoToUrl(uri);

            trace.Diagnostics.LogInformation("{0}> Initiating chrome driver executed in {2}ms");

            if (!chromiumDataDirectoryExists)
                Thread.Sleep(30_000);

            WebDriverWait webDriverWait = new WebDriverWait(webDriver, TimeSpan.FromSeconds(60));

            webDriverWait.Until((driver) =>
            {
                bool? found = null;

                while (found != true)
                {
                    try
                    {
                        found = driver.FindElement(canUseChrome ? By.TagName("pre") : By.ClassName("cm-content"))?.Displayed;
                    }
                    catch (NoSuchElementException) { }
                    finally
                    {
                        Thread.Sleep(100);
                    }
                }

                return found.GetValueOrDefault();
            });

            string json = webDriver.FindElement(canUseChrome ? By.TagName("pre") : By.ClassName("cm-content"))?.Text;

            webDriver.Close();
            webDriver.Quit();

            trace.Diagnostics.LogInformation("{0}> Loading json config executed in {2}ms");

            if (!json.IsNullOrWhiteSpace() && json.TryParse(out JObject config))
            {
                string[] currentVariables = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process).Keys.OfType<string>().ToArray();

                foreach (KeyValuePair<string, JToken?> setting in config)
                {
                    if (setting.Key.IsAny(ExcludedConfig)) continue;
                    if (setting.Key.IsAny(currentVariables)) continue;

                    string current = Environment.GetEnvironmentVariable(setting.Key, EnvironmentVariableTarget.Process);

                    ConfigReader.Set(setting.Key, setting.Value.ToString());
                }

                trace.Diagnostics.LogInformation("{0}> Setting process environment variables executed in {2}ms");
            }

            trace.Diagnostics.LogInformation("{0}> EnvironmentConfigurationProvider executed in {7}ms");
        }
    }
}
