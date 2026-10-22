using Microsoft.Extensions.Configuration;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.Core.UI.Factory
{
    public static class LocalDriverFactory
    {
        public static AppiumDriver Create(IConfiguration config)
        {
            var platform = config["Platform"] ?? throw new InvalidOperationException("Platform not specified in configuration");
            var serverUrl = config[$"Appium:{platform}:ServerUrl"] ?? throw new InvalidOperationException($"ServerUrl not specified for {platform}");

            if (platform.Equals("iOS", StringComparison.OrdinalIgnoreCase))
            {
                return CreateiOSDriver(config, serverUrl);
            }
            else if (platform.Equals("Android", StringComparison.OrdinalIgnoreCase))
            {
                return CreateAndroidDriver(config, serverUrl);
            }
            else
            {
                throw new InvalidOperationException($"Unsupported platform: {platform}");
            }
        }

        private static IOSDriver CreateiOSDriver(IConfiguration config, string serverUrl)
        {
            var options = new AppiumOptions();
            options.PlatformName = "iOS";
            
            // Use properties instead of AddAdditionalAppiumOption for standard capabilities
            options.DeviceName = config["Appium:iOS:DeviceName"];
            options.PlatformVersion = config["Appium:iOS:PlatformVersion"];
            options.App = config["Appium:iOS:App"];
            options.AutomationName = "XCUITest";
            
            // Use AddAdditionalAppiumOption for iOS-specific capabilities
            options.AddAdditionalAppiumOption("udid", config["Appium:iOS:UDID"]);
            options.AddAdditionalAppiumOption("bundleId", config["Appium:iOS:BundleId"]);
            options.AddAdditionalAppiumOption("noReset", config.GetValue<bool>("Appium:iOS:NoReset"));
            options.AddAdditionalAppiumOption("fullReset", config.GetValue<bool>("Appium:iOS:FullReset"));
            options.AddAdditionalAppiumOption("autoLaunch", config.GetValue<bool>("Appium:iOS:AutoLaunch"));
            options.AddAdditionalAppiumOption(
                "forceAppLaunch",
                config.GetValue("Appium:iOS:ForceAppLaunch", true));
            options.AddAdditionalAppiumOption("processArguments", IosProcessArguments.Resolve(config));
            
            // Add WebDriverAgent timeout capabilities
            if (config.GetValue<int>("Appium:iOS:WdaLaunchTimeout") > 0)
                options.AddAdditionalAppiumOption("wdaLaunchTimeout", config.GetValue<int>("Appium:iOS:WdaLaunchTimeout"));
            if (config.GetValue<int>("Appium:iOS:WdaConnectionTimeout") > 0)
                options.AddAdditionalAppiumOption("wdaConnectionTimeout", config.GetValue<int>("Appium:iOS:WdaConnectionTimeout"));

            return new IOSDriver(new Uri(serverUrl), options, TimeSpan.FromSeconds(config.GetValue<int>("Appium:iOS:CommandTimeout")));
        }

        private static AndroidDriver CreateAndroidDriver(IConfiguration config, string serverUrl)
        {
            var options = new AppiumOptions();
            options.PlatformName = "Android";
            options.DeviceName = config["Appium:Android:DeviceName"] ?? "emulator-5554";
            options.App = config["Appium:Android:App"];
            options.AutomationName = "UIAutomator2"; // Use the AutomationName property

            // Removed redundant AddAdditionalAppiumOption for AutomationName
            options.AddAdditionalAppiumOption("udid", config["Appium:Android:UDID"]);
            options.AddAdditionalAppiumOption("appPackage", config["Appium:Android:AppPackage"]);
            options.AddAdditionalAppiumOption("appActivity", config["Appium:Android:AppActivity"]);
            options.AddAdditionalAppiumOption("noReset", config.GetValue<bool>("Appium:Android:NoReset"));
            options.AddAdditionalAppiumOption("fullReset", config.GetValue<bool>("Appium:Android:FullReset"));
            options.AddAdditionalAppiumOption("forceAppLaunch", config.GetValue("Appium:Android:ForceAppLaunch", true));
            string? intentArguments = config["Appium:Android:OptionalIntentArguments"];
            if (!intentArguments.IsNullOrWhiteSpace())
            {
                options.AddAdditionalAppiumOption("optionalIntentArguments", intentArguments);
            }
            options.AddAdditionalAppiumOption("settings[disableIdLocatorAutocompletion]", true);
            options.AddAdditionalAppiumOption("adbExecTimeout", 120000); // 120s
            options.AddAdditionalAppiumOption("appWaitDuration", 120000); // wait for app launch
            options.AddAdditionalAppiumOption("uiautomator2ServerLaunchTimeout", 120000);
            options.AddAdditionalAppiumOption("uiautomator2ServerInstallTimeout", 120000);
            //options.AddAdditionalAppiumOption("fullReset", false);
            //options.AddAdditionalAppiumOption("noReset", true);

            return new AndroidDriver(new Uri(serverUrl), options, TimeSpan.FromSeconds(config.GetValue<int>("Appium:Android:CommandTimeout")));
        }
    }
}
