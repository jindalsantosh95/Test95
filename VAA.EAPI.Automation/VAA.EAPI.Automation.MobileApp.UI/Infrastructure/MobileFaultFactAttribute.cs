using Microsoft.Extensions.Configuration;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.UI.Factory;

namespace VAA.EAPI.Automation.MobileApp.UI.Infrastructure;

/// <summary>Discovery-time opt-in prevents a default pack from creating even an Appium session.</summary>
public sealed class MobileFaultFactAttribute : FactAttribute
{
    public MobileFaultFactAttribute(bool postHpp = false)
    {
        IConfiguration configuration = LoadConfiguration();
        if (!CanRun(configuration, postHpp))
            Skip = "Opt-in fault pack: isolated iOS proxy routing and CA trust must be confirmed first.";
    }

    public static IConfiguration LoadConfiguration()
        => new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("local.settings.local.json", optional: true)
            .AddEnvironmentVariables().Build();

    public static bool CanRun(IConfiguration configuration, bool postHpp = false)
    {
        if (postHpp && !configuration.GetValue<bool>("FaultInjection:AllowPostHppTestPayment"))
            return false;
        if (!configuration.GetValue<bool>("FaultInjection:Enabled")
            || !configuration.GetValue<bool>("FaultInjection:IsolatedRoutingConfirmed")
            || IosProcessArguments.ResolveConfiguredBuildProfile(configuration) != "sit_standard")
            return false;
        if (configuration["RunMode"] == "Local")
            return configuration["Platform"] == "iOS";
        return configuration["RunMode"] == "Remote"
            && configuration["BrowserStack:Platform"] == "iOS"
            && configuration.GetValue<bool>("BrowserStack:Local")
            && !configuration["BrowserStack:LocalIdentifier"].IsNullOrWhiteSpace()
            && configuration.GetValue<bool>("BrowserStack:NetworkLogs")
            && !configuration.GetValue<bool>("BrowserStack:CaptureNetworkContent")
            && !configuration.GetValue<bool>("FaultInjection:UseProcessCapture");
    }
}

[CollectionDefinition("Mobile fault injection", DisableParallelization = true)]
public sealed class MobileFaultCollection { }
