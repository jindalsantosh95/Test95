using System.Text.Json;
using Microsoft.Playwright;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.Core.Playwright.Browser;

internal static class BrowserStackPlaywrightConnection
{
    public static async Task<IBrowser> ConnectBrowserAsync(
        IPlaywright playwright,
        BrowserStackPlaywrightOptions options)
    {
        string endpoint = BuildEndpoint(options);
        return await playwright.Chromium.ConnectAsync(endpoint);
    }

    public static async Task<IAndroidDevice> ConnectAndroidAsync(
        IPlaywright playwright,
        BrowserStackPlaywrightOptions options)
    {
        string endpoint = BuildEndpoint(options);
        return await playwright.Android.ConnectAsync(endpoint);
    }

    private static string BuildEndpoint(BrowserStackPlaywrightOptions options)
    {
        Dictionary<string, object> capabilities = new()
        {
            ["browser"] = options.Browser,
            ["browserstack.username"] = options.Username,
            ["browserstack.accessKey"] = options.AccessKey,
            ["project"] = options.ProjectName,
            ["build"] = options.BuildName,
            ["name"] = options.SessionName,
            ["browserstack.local"] = options.UseLocal,
            ["browserstack.debug"] = options.Debug,
            ["browserstack.video"] = options.Video,
            ["browserstack.networkLogs"] = options.NetworkLogs,
            ["browserstack.idleTimeout"] = 300,
            ["browserstack.networkLogsOptions"] = new Dictionary<string, object>
            {
                ["captureContent"] = options.CaptureNetworkContent
            }
        };

        if (options.DeviceName.IsNullOrWhiteSpace())
        {
            capabilities["os"] = options.OperatingSystem;
            capabilities["os_version"] = options.OperatingSystemVersion;
            if (options.Browser.Is("chrome") || options.Browser.Is("edge"))
            {
                capabilities["browser_version"] = options.BrowserVersion;
            }
        }
        else
        {
            // Current BrowserStack Playwright mobile capabilities use the W3C camel-case names.
            capabilities["deviceName"] = options.DeviceName;
            capabilities["osVersion"] = options.OperatingSystemVersion;
            capabilities["realMobile"] = true;
        }

        Version? clientAssemblyVersion = typeof(IPlaywright).Assembly.GetName().Version;
        if (clientAssemblyVersion is not null)
        {
            string playwrightVersion =
                $"{clientAssemblyVersion.Major}.{clientAssemblyVersion.Minor}.{clientAssemblyVersion.Build}";
            capabilities["client.playwrightVersion"] = playwrightVersion;
            capabilities["browserstack.playwrightVersion"] = playwrightVersion;
        }

        if (!options.LocalIdentifier.IsNullOrWhiteSpace())
        {
            capabilities["browserstack.localIdentifier"] = options.LocalIdentifier;
        }

        string capabilitiesJson = JsonSerializer.Serialize(capabilities);
        return $"{options.Endpoint}?caps={Uri.EscapeDataString(capabilitiesJson)}";
    }
}
