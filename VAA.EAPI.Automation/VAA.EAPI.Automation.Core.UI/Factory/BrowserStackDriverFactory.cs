using Microsoft.Extensions.Configuration;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.Core.UI.Factory;

public static class BrowserStackDriverFactory
{
    private const int DefaultCommandTimeoutSeconds = 240;
    private const int DefaultIdleTimeoutSeconds = 300;
    private const int DefaultSessionCreationAttempts = 3;
    private const int DefaultSessionCreationRetryDelaySeconds = 5;

    public static AndroidDriver CreateAndroid(IConfiguration config)
        => new AndroidDriver(
            GetServerUri(config),
            CreateOptions(config, platform: "Android"),
            ResolveCommandTimeout(config));

    public static IOSDriver CreateIos(IConfiguration config)
        => new IOSDriver(
            GetServerUri(config),
            CreateOptions(config, platform: "iOS"),
            ResolveCommandTimeout(config));

    /// <summary>
    /// Requires: BrowserStack:Platform = "Android" or "iOS"
    /// </summary>
    public static AppiumDriver Create(IConfiguration config)
        => CreateAsync(config).GetAwaiter().GetResult();

    public static async Task<AppiumDriver> CreateAsync(IConfiguration config)
    {
        int attempts = ResolvePositiveSetting(
            config, "BrowserStack:SessionCreationAttempts", DefaultSessionCreationAttempts);
        int retryDelaySeconds = ResolvePositiveSetting(
            config,
            "BrowserStack:SessionCreationRetryDelaySeconds",
            DefaultSessionCreationRetryDelaySeconds);
        OpenQA.Selenium.WebDriverException lastTransientException = null;
        IReadOnlyList<DeviceCandidate> devices = ResolveDeviceCandidates(config);

        for (int attempt = 1; attempt <= attempts; attempt++)
        {
            DeviceCandidate device = devices[(attempt - 1) % devices.Count];
            try
            {
                return CreateOnce(config, device);
            }
            catch (OpenQA.Selenium.WebDriverException exception)
                when (BrowserStackSessionRetryPolicy.IsTransient(exception))
            {
                lastTransientException = exception;
                if (attempt == attempts)
                    break;

                Console.WriteLine(
                    $"BrowserStack session creation attempt {attempt} of {attempts} on " +
                    $"{device.DeviceName}/{config["BrowserStack:Platform"]} " +
                    $"{device.PlatformVersion} failed transiently: " +
                    $"{exception.Message}. Retrying in {retryDelaySeconds} seconds.");
                await Task.Delay(TimeSpan.FromSeconds(retryDelaySeconds));
            }
        }

        throw new InvalidOperationException(
            $"BrowserStack session creation failed after {attempts} attempts.",
            lastTransientException);
    }

    private static AppiumDriver CreateOnce(IConfiguration config, DeviceCandidate device)
    {
        string platform = (config["BrowserStack:Platform"] ?? string.Empty).Trim();

        if (platform.Is("iOS"))
            return new IOSDriver(
                GetServerUri(config),
                CreateOptions(config, platform: "iOS", device),
                ResolveCommandTimeout(config));

        if (platform.Is("Android"))
            return new AndroidDriver(
                GetServerUri(config),
                CreateOptions(config, platform: "Android", device),
                ResolveCommandTimeout(config));

        throw new InvalidOperationException("BrowserStack:Platform must be 'iOS' or 'Android'.");
    }

    private static Uri GetServerUri(IConfiguration config)
        => new Uri(config["BrowserStack:ServerUrl"]
            ?? throw new InvalidOperationException("Missing BrowserStack:ServerUrl"));

    private static AppiumOptions CreateOptions(
        IConfiguration config,
        string platform,
        DeviceCandidate device = null)
    {
        (string user, string key) = GetBrowserStackCredentials(config);

        string deviceName = device is null
            ? Require(config, "BrowserStack:DeviceName")
            : device.DeviceName;
        string platformVersion = device is null
            ? Require(config, "BrowserStack:PlatformVersion")
            : device.PlatformVersion;
        string app = Require(config, "BrowserStack:App");
        bool fastProfile = (config["BrowserStack:Profile"] ?? "Diagnostic").Is("Fast");
        int idleTimeoutSeconds = ResolveBoundedSetting(
            config, "BrowserStack:IdleTimeoutSeconds", DefaultIdleTimeoutSeconds, minimum: 0, maximum: 300);

        AppiumOptions options = new()
        {
            PlatformName = platform,
            DeviceName = deviceName,
            PlatformVersion = platformVersion,
            AutomationName = platform.Is("iOS") ? "XCUITest" : "UIAutomator2",
            App = app,
        };

        options.AddAdditionalAppiumOption("newCommandTimeout", idleTimeoutSeconds);

        if (platform.Is("iOS"))
        {
            options.AddAdditionalAppiumOption("autoAcceptAlerts", true);
            // BrowserStack can preserve the installed app and XCUITest 11 then defaults
            // forceAppLaunch to false. Force a fresh process launch so processArguments,
            // including automation feature flags, are applied to every session.
            options.AddAdditionalAppiumOption(
                "forceAppLaunch",
                config.GetValue("BrowserStack:ForceAppLaunch", true));
            options.AddAdditionalAppiumOption("processArguments", IosProcessArguments.Resolve(config));
        }

        bool networkLogs = ResolveProfileSetting(config, "BrowserStack:NetworkLogs", !fastProfile);
        Dictionary<string, object> browserStackOptions = new()
        {
            ["userName"] = user,
            ["accessKey"] = key,
            ["projectName"] = Require(config, "BrowserStack:Project"),
            ["buildName"] = Require(config, "BrowserStack:Build"),
            ["sessionName"] = Require(config, "BrowserStack:Name"),
            ["debug"] = ResolveProfileSetting(config, "BrowserStack:Debug", !fastProfile),
            ["video"] = ResolveProfileSetting(config, "BrowserStack:Video", !fastProfile),
            ["networkLogs"] = networkLogs,
            ["disableAnimations"] = config.GetValue("BrowserStack:DisableAnimations", true),
            ["idleTimeout"] = idleTimeoutSeconds,
        };
        string appiumVersion = config["BrowserStack:AppiumVersion"]?.Trim();
        if (config.GetValue("BrowserStack:Local", false))
        {
            browserStackOptions["local"] = true;
            browserStackOptions["localIdentifier"] = Require(config, "BrowserStack:LocalIdentifier");
        }
        if (!appiumVersion.IsNullOrWhiteSpace())
        {
            browserStackOptions["appiumVersion"] = appiumVersion;
        }
        if (networkLogs && config.GetValue("BrowserStack:CaptureNetworkContent", false))
        {
            browserStackOptions["networkLogsOptions"] = new Dictionary<string, object>
            {
                ["captureContent"] = true,
            };
        }

        // AddAdditionalAppiumOption preserves an explicitly supplied vendor prefix, so this
        // is emitted as the W3C-standard top-level bstack:options capability.
        options.AddAdditionalAppiumOption("bstack:options", browserStackOptions);
        return options;
    }

    private static bool ResolveProfileSetting(IConfiguration config, string key, bool profileDefault)
        => config.GetValue<bool?>(key) ?? profileDefault;

    private static IReadOnlyList<DeviceCandidate> ResolveDeviceCandidates(IConfiguration config)
    {
        List<DeviceCandidate> candidates =
        [
            new(
                Require(config, "BrowserStack:DeviceName"),
                Require(config, "BrowserStack:PlatformVersion")),
        ];

        if (!config.GetValue("BrowserStack:DeviceFallbacksEnabled", true))
        {
            return candidates;
        }

        foreach (IConfigurationSection fallback in
                 config.GetSection("BrowserStack:DeviceFallbacks").GetChildren())
        {
            string deviceName = fallback["DeviceName"]?.Trim() ?? string.Empty;
            string platformVersion = fallback["PlatformVersion"]?.Trim() ?? string.Empty;
            if (deviceName.IsNullOrWhiteSpace() || platformVersion.IsNullOrWhiteSpace())
            {
                throw new InvalidOperationException(
                    "Each BrowserStack:DeviceFallbacks entry requires DeviceName and PlatformVersion.");
            }

            DeviceCandidate candidate = new(deviceName, platformVersion);
            if (!candidates.Contains(candidate))
            {
                candidates.Add(candidate);
            }
        }

        return candidates;
    }

    private static TimeSpan ResolveCommandTimeout(IConfiguration config) =>
        TimeSpan.FromSeconds(ResolvePositiveSetting(
            config, "BrowserStack:CommandTimeoutSeconds", DefaultCommandTimeoutSeconds));

    private static int ResolvePositiveSetting(IConfiguration config, string key, int defaultValue)
    {
        int value = config.GetValue<int?>(key) ?? defaultValue;
        if (value <= 0)
            throw new InvalidOperationException($"{key} must be greater than zero.");

        return value;
    }

    private static int ResolveBoundedSetting(
        IConfiguration config,
        string key,
        int defaultValue,
        int minimum,
        int maximum)
    {
        int value = config.GetValue<int?>(key) ?? defaultValue;
        if (value < minimum || value > maximum)
            throw new InvalidOperationException($"{key} must be between {minimum} and {maximum} seconds.");

        return value;
    }

    private static (string user, string key) GetBrowserStackCredentials(IConfiguration config)
    {
        string user = Require(config, "BrowserStack:Username");
        string key = Require(config, "BrowserStack:AccessKey");

        return (user, key);
    }

    private static string Require(IConfiguration config, string key)
        => !string.IsNullOrWhiteSpace(config[key])
            ? config[key]!
            : throw new InvalidOperationException($"Missing or empty config value: {key}");

    private sealed record DeviceCandidate(string DeviceName, string PlatformVersion);

}
