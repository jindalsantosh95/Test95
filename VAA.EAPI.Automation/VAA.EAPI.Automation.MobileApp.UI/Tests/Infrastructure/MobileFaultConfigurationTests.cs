using Microsoft.Extensions.Configuration;
using VAA.EAPI.Automation.Core.UI.Factory;
using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

[Trait("Category", "MobileApp-Offline")]
public sealed class MobileFaultConfigurationTests
{
    [Fact]
    public void DefaultConfiguration_DoesNotEnableFaults()
        => Assert.False(MobileFaultFactAttribute.CanRun(new ConfigurationBuilder().Build()));

    [Theory]
    [InlineData("FaultInjection:Enabled", "false")]
    [InlineData("FaultInjection:IsolatedRoutingConfirmed", "false")]
    [InlineData("FaultInjection:UseProcessCapture", "true")]
    [InlineData("BrowserStack:Platform", "Android")]
    [InlineData("BrowserStack:Local", "false")]
    [InlineData("BrowserStack:LocalIdentifier", "")]
    [InlineData("BrowserStack:NetworkLogs", "false")]
    [InlineData("BrowserStack:CaptureNetworkContent", "true")]
    [InlineData("IOSLaunch:BuildProfile", "production")]
    [InlineData("IOS_BUILD_PROFILE", "production")]
    [InlineData("IOS_BUILD_PROFILE", "")]
    public void RemoteConfiguration_RejectsUnsafeOrIncompleteRouting(string key, string value)
    {
        Dictionary<string, string?> settings = RemoteSettings();
        settings[key] = value;
        Assert.False(MobileFaultFactAttribute.CanRun(new ConfigurationBuilder().AddInMemoryCollection(settings).Build()));
    }

    [Fact]
    public void RemoteConfiguration_AllExplicitGuardsPermitQualification()
        => Assert.True(MobileFaultFactAttribute.CanRun(new ConfigurationBuilder().AddInMemoryCollection(RemoteSettings()).Build()));

    [Fact]
    public void PostHpp_RequiresSeparatePaymentOptIn()
    {
        Dictionary<string, string?> settings = RemoteSettings();
        Assert.False(MobileFaultFactAttribute.CanRun(new ConfigurationBuilder().AddInMemoryCollection(settings).Build(), postHpp: true));
        settings["FaultInjection:AllowPostHppTestPayment"] = "true";
        Assert.True(MobileFaultFactAttribute.CanRun(new ConfigurationBuilder().AddInMemoryCollection(settings).Build(), postHpp: true));
        settings["IOSLaunch:BuildProfile"] = "production";
        Assert.False(MobileFaultFactAttribute.CanRun(new ConfigurationBuilder().AddInMemoryCollection(settings).Build(), postHpp: true));
    }

    [Theory]
    [InlineData("Local", false)]
    [InlineData("Local", true)]
    [InlineData("Remote", false)]
    [InlineData("Remote", true)]
    public void EffectiveProfile_GateMatchesLaunchArguments(string runMode, bool postHpp)
    {
        Dictionary<string, string?> settings = RemoteSettings();
        settings["RunMode"] = runMode;
        settings["Platform"] = "iOS";
        settings["FaultInjection:AllowPostHppTestPayment"] = "true";
        foreach (string profile in new[] { "production", "", "sit_standard" })
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .AddInMemoryCollection(new Dictionary<string, string?> { ["IOS_BUILD_PROFILE"] = profile })
                .Build();
            Assert.Equal(profile, ((string[])IosProcessArguments.Resolve(configuration)["args"])[2]);
            Assert.Equal(profile == "sit_standard", MobileFaultFactAttribute.CanRun(configuration, postHpp));
        }
    }

    private static Dictionary<string, string?> RemoteSettings() => new()
    {
        ["RunMode"] = "Remote",
        ["IOSLaunch:BuildProfile"] = "sit_standard",
        ["FaultInjection:Enabled"] = "true",
        ["FaultInjection:IsolatedRoutingConfirmed"] = "true",
        ["FaultInjection:UseProcessCapture"] = "false",
        ["BrowserStack:Platform"] = "iOS",
        ["BrowserStack:Local"] = "true",
        ["BrowserStack:LocalIdentifier"] = "isolated-test-tunnel",
        ["BrowserStack:NetworkLogs"] = "true",
        ["BrowserStack:CaptureNetworkContent"] = "false"
    };
}
