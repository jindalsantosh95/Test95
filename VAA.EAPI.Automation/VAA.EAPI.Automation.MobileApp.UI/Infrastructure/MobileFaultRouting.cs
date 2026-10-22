using Newtonsoft.Json.Linq;
using OpenQA.Selenium.Appium;

namespace VAA.EAPI.Automation.MobileApp.UI.Infrastructure;

public static class MobileFaultRouting
{
    public static int GetTestAppProcessId(AppiumDriver driver)
    {
        object state = driver.ExecuteScript("mobile: activeAppInfo")
            ?? throw new InvalidOperationException("The foreground app state was unavailable; capture was not started.");
        JObject activeApp = JObject.FromObject(state);
        if (activeApp.Value<string>("bundleId") != "com.virginatlantic.mobile.ios.testapp"
            || activeApp.Value<int?>("pid") is not int processId || processId <= 0)
            throw new InvalidOperationException("Cannot restrict fault capture to the foreground iOS test app process.");
        return processId;
    }
}
