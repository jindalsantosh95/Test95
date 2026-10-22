using OpenQA.Selenium.Appium;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.MobileApp.UI.PageObjects;

public static class FlightSearchPanelPageFactory
{
    public static IFlightSearchPanelPage Create(AppiumDriver driver)
        => IsAndroid(driver.Capabilities.GetCapability("platformName")?.ToString())
            ? new AndroidSearchPanelPage(driver)
            : new IosSearchPanelPage(driver);

    internal static bool IsAndroid(string? platform)
    {
        if (platform.Is("Android"))
            return true;
        if (platform.Is("iOS"))
            return false;
        throw new NotSupportedException($"Search panel does not support platform '{platform ?? "missing"}'.");
    }
}
