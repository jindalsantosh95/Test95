using Microsoft.Extensions.Configuration;
using OpenQA.Selenium.Appium;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.UI.Factory;
using VAA.EAPI.Automation.Core.UI.Helpers;
using Xunit;

namespace VAA.EAPI.Automation.Core.UI.Infrastructure
{
    public sealed class MobileFixture : IAsyncLifetime
    {
        private static readonly SemaphoreSlim LocalSessionGate = new(1, 1);
        private readonly IReadOnlyDictionary<string, string?>? _configurationOverrides;
        private string _sessionId = string.Empty;
        private bool _ownsLocalSessionGate;

        public MobileFixture(IReadOnlyDictionary<string, string?>? configurationOverrides = null)
        {
            _configurationOverrides = configurationOverrides;
        }

        public AppiumDriver Driver { get; private set; } = null!;
        public IConfiguration Config { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            IConfigurationBuilder configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("local.settings.local.json", optional: true)
                .AddEnvironmentVariables();

            if (_configurationOverrides is not null)
            {
                configurationBuilder.AddInMemoryCollection(_configurationOverrides);
            }

            Config = configurationBuilder.Build();

            string configuredRunMode = Config["RunMode"] ?? "Local";
            if (!configuredRunMode.Is("Remote"))
            {
                await LocalSessionGate.WaitAsync();
                _ownsLocalSessionGate = true;
            }

            try
            {
                string runMode = Config["RunMode"] ?? "Local"; // Default to Local if not specified

                if (runMode.Is("Remote"))
                {
                    Driver = await BrowserStackDriverFactory.CreateAsync(Config);
                }
                else
                {
                    Driver = LocalDriverFactory.Create(Config);
                }

                try
                {
                    if (Driver.SessionId is null || Driver.SessionId.ToString().IsNullOrWhiteSpace())
                    {
                        throw new InvalidOperationException("The Appium driver did not create a remote session ID.");
                    }

                    string pageSource = Driver.PageSource;
                    if (pageSource.IsNullOrWhiteSpace())
                    {
                        throw new InvalidOperationException("The Appium session returned an empty page source.");
                    }

                    _sessionId = Driver.SessionId.ToString();
                    Console.WriteLine($"Appium session started: {_sessionId}");
                }
                catch (Exception ex)
                {
                    string mode = runMode.Is("Remote") ? "BrowserStack" : "local Appium";
                    throw new InvalidOperationException(
                        $"The {mode} session did not start correctly. " +
                        "The PageSource command failed; see inner exception for the real reason.",
                        ex);
                }

                Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(0);
            }
            catch (Exception ex)
            {
                string runMode = Config["RunMode"] ?? "Local";
                await DisposeAsync();

                if (runMode.Is("Remote"))
                {
                    throw new InvalidOperationException(
                        "Failed to start BrowserStack Appium session. Check capabilities, app id, and credentials. " +
                        $"Mode=Remote, Platform={Config["BrowserStack:Platform"]}, Device={Config["BrowserStack:DeviceName"]}, " +
                        $"OS={Config["BrowserStack:PlatformVersion"]}, App={Config["BrowserStack:App"]}, Hub={Config["BrowserStack:ServerUrl"]}",
                        ex);
                }
                else
                {
                    throw new InvalidOperationException(
                        "Failed to start local Appium session. Check that Appium server is running and configuration is correct. " +
                        $"Mode=Local, Platform={Config["Platform"]}, Device={Config[$"Appium:{Config["Platform"]}:DeviceName"]}, " +
                        $"App={Config[$"Appium:{Config["Platform"]}:App"]}, ServerUrl={Config[$"Appium:{Config["Platform"]}:ServerUrl"]}",
                        ex);
                }
            }
        }

        public async Task DisposeAsync()
        {
            try
            {
                if (Driver is not null)
                {
                    Driver.Quit();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(
                    $"Appium driver cleanup failed for session " +
                    $"{(_sessionId.IsNullOrWhiteSpace() ? "unknown" : _sessionId)}: {exception.Message}");

                bool remote = (Config["RunMode"] ?? "Local").Is("Remote");
                if (remote && !_sessionId.IsNullOrWhiteSpace())
                {
                    bool released = await BrowserStackSessionCleanup.TryTerminateAsync(Config, _sessionId);
                    Console.WriteLine(released
                        ? $"BrowserStack fallback cleanup released session {_sessionId}."
                        : $"BrowserStack fallback cleanup could not confirm release of session {_sessionId}.");
                }
            }
            finally
            {
                if (_ownsLocalSessionGate)
                {
                    _ownsLocalSessionGate = false;
                    LocalSessionGate.Release();
                }
            }
        }
    }
}
