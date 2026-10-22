using Microsoft.Extensions.Configuration;
using OpenQA.Selenium.Appium;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.MobileApp.UI.VisualValidation;

public static class SearchPanelVisualRuntimeContract
{
    public const string DeviceName = "iPhone 17 Pro";
    public const string PlatformVersion = "26";
    public const int ViewportWidthPoints = 402;
    public const string Appearance = "Light";
    public const string Locale = "en-GB";
    public const string TextSize = "Default";

    public static IReadOnlyDictionary<string, string?> ConfigurationOverrides { get; } =
        new Dictionary<string, string?>
        {
            ["Platform"] = "iOS",
            ["Appium:iOS:DeviceName"] = DeviceName,
            ["Appium:iOS:PlatformVersion"] = PlatformVersion,
            ["BrowserStack:DeviceName"] = DeviceName,
            ["BrowserStack:PlatformVersion"] = PlatformVersion,
            ["BrowserStack:DeviceFallbacksEnabled"] = "false",
            ["BrowserStack:Name"] = "Search and Book | iOS | Visual regression",
            ["IOSLaunch:Appearance"] = Appearance,
            ["IOSLaunch:Locale"] = Locale,
            ["IOSLaunch:TextSize"] = TextSize,
        };

    public static void Validate(
        IConfiguration configuration,
        AppiumDriver driver,
        VisualBaselineRuntime runtime)
    {
        ValidateConfiguration(configuration, runtime);

        int actualViewportWidth = driver.Manage().Window.Size.Width;
        if (actualViewportWidth != runtime.ViewportWidthPoints)
        {
            throw new InvalidOperationException(
                $"Visual regression requires a {runtime.ViewportWidthPoints}-point viewport on " +
                $"{runtime.Device}, but Appium reported {actualViewportWidth} points. " +
                "The comparison was stopped before evaluating the Figma baseline.");
        }
    }

    public static void ValidateConfiguration(
        IConfiguration configuration,
        VisualBaselineRuntime? runtime = null)
    {
        VisualBaselineRuntime expected = runtime ?? new VisualBaselineRuntime(
            Platform: "iOS",
            Device: DeviceName,
            ViewportWidthPoints,
            Appearance,
            Locale,
            TextSize);
        string appearance = configuration["IOSLaunch:Appearance"] ?? string.Empty;
        string locale = configuration["IOSLaunch:Locale"] ?? string.Empty;
        string textSize = configuration["IOSLaunch:TextSize"] ?? string.Empty;

        if (!expected.Platform.Is("iOS") ||
            !expected.Device.Is(DeviceName) ||
            expected.ViewportWidthPoints != ViewportWidthPoints ||
            !expected.Appearance.Is(Appearance) ||
            !expected.Locale.Is(Locale) ||
            !expected.TextSize.Is(TextSize) ||
            !appearance.Is(expected.Appearance) ||
            !locale.Is(expected.Locale) ||
            !textSize.Is(expected.TextSize))
        {
            throw new InvalidOperationException(
                $"Search panel visual regression requires {expected.Appearance} appearance, " +
                $"{expected.Locale} locale and {expected.TextSize} text size.");
        }

        if (!(configuration["RunMode"] ?? "Local").Is("Remote"))
        {
            string localPlatform = configuration["Platform"] ?? string.Empty;
            string localDevice = configuration["Appium:iOS:DeviceName"] ?? string.Empty;
            string localPlatformVersion = configuration["Appium:iOS:PlatformVersion"] ?? string.Empty;
            if (!localPlatform.Is("iOS") ||
                !localDevice.Is(expected.Device) ||
                !localPlatformVersion.Is(PlatformVersion))
            {
                throw new InvalidOperationException(
                    $"Local search panel visual regression is pinned to {expected.Device}/iOS " +
                    $"{PlatformVersion}; configured values were {localDevice}/{localPlatform} " +
                    $"{localPlatformVersion}.");
            }

            return;
        }

        string device = configuration["BrowserStack:DeviceName"] ?? string.Empty;
        string platformVersion = configuration["BrowserStack:PlatformVersion"] ?? string.Empty;
        bool fallbacksEnabled = configuration.GetValue("BrowserStack:DeviceFallbacksEnabled", true);
        if (!device.Is(DeviceName) || !platformVersion.Is(PlatformVersion) || fallbacksEnabled)
        {
            throw new InvalidOperationException(
                $"Search panel visual regression is pinned to {DeviceName}/iOS {PlatformVersion} " +
                "with BrowserStack device fallback disabled.");
        }
    }
}
