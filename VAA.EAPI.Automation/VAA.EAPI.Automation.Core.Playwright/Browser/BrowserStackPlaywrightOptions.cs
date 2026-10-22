using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.Core.Playwright.Browser;

public sealed record BrowserStackPlaywrightOptions
{
    public required string Username { get; init; }

    public required string AccessKey { get; init; }

    public string Endpoint { get; init; } = "wss://cdp.browserstack.com/playwright";

    public string OperatingSystem { get; init; } = "Windows";

    public string OperatingSystemVersion { get; init; } = "11";

    public string Browser { get; init; } = "chrome";

    public string BrowserVersion { get; init; } = "latest";

    public string? DeviceName { get; init; }

    public string ProjectName { get; init; } = "Search and Book UI";

    public string BuildName { get; init; } = "Search and Book UI - Local";

    public string SessionName { get; init; } = "Search and Book UI";

    public bool UseLocal { get; init; }

    public string? LocalIdentifier { get; init; }

    public bool Debug { get; init; } = true;

    public bool Video { get; init; } = true;

    public bool NetworkLogs { get; init; } = true;

    public bool CaptureNetworkContent { get; init; }

    public bool PreflightEnabled { get; init; }

    public static BrowserStackPlaywrightOptions FromEnvironment()
    {
        bool fastProfile = ReadString("BROWSERSTACK_PROFILE", "Diagnostic").Is("Fast");

        return new BrowserStackPlaywrightOptions
        {
            Username = Require("BROWSERSTACK_USERNAME"),
            AccessKey = Require("BROWSERSTACK_ACCESS_KEY"),
            Endpoint = ReadString("SAB_BS_ENDPOINT", "wss://cdp.browserstack.com/playwright"),
            OperatingSystem = ReadString("SAB_BS_OS", "Windows"),
            OperatingSystemVersion = ReadString("SAB_BS_OS_VERSION", "11"),
            Browser = ReadString("SAB_BS_BROWSER", "chrome"),
            BrowserVersion = ReadString("SAB_BS_BROWSER_VERSION", "latest"),
            DeviceName = ReadOptionalString("SAB_BS_DEVICE_NAME"),
            ProjectName = ReadString("BROWSERSTACK_PROJECT", "Virgin Atlantic Web - Search and Book"),
            BuildName = ReadString("BROWSERSTACK_BUILD", "Search and Book UI - Local"),
            SessionName = ReadString("SAB_BS_SESSION_NAME", "Search and Book UI"),
            UseLocal = ReadBool("BROWSERSTACK_LOCAL", false),
            LocalIdentifier = ReadOptionalString("BROWSERSTACK_LOCAL_IDENTIFIER"),
            Debug = ReadBool("BROWSERSTACK_DEBUG", !fastProfile),
            Video = ReadBool("BROWSERSTACK_VIDEO", !fastProfile),
            NetworkLogs = ReadBool("BROWSERSTACK_NETWORK_LOGS", !fastProfile),
            CaptureNetworkContent = ReadBool("BROWSERSTACK_CAPTURE_NETWORK_CONTENT", false),
            PreflightEnabled = ReadBool("BROWSERSTACK_PREFLIGHT", false)
        };
    }

    private static string Require(string name)
    {
        string? value = Environment.GetEnvironmentVariable(name);
        if (value.IsNullOrWhiteSpace())
        {
            throw new InvalidOperationException($"{name} is required for BrowserStack Playwright execution.");
        }

        return value;
    }

    private static string ReadString(string name, string defaultValue)
    {
        string? value = Environment.GetEnvironmentVariable(name);
        return value.IsNullOrWhiteSpace() ? defaultValue : value;
    }

    private static string? ReadOptionalString(string name)
    {
        string? value = Environment.GetEnvironmentVariable(name);
        return value.IsNullOrWhiteSpace() ? null : value;
    }

    private static bool ReadBool(string name, bool defaultValue)
    {
        string? value = Environment.GetEnvironmentVariable(name);
        return bool.TryParse(value, out bool parsed) ? parsed : defaultValue;
    }
}
