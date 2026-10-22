using Microsoft.Extensions.Configuration;
using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

[Trait("Category", "MobileApp-Offline")]
public sealed class PayPalReadinessTests
{
    [Theory]
    [InlineData("https://www.sandbox.paypal.com/checkout", true)]
    [InlineData("https://sandbox.paypal.com/signin", true)]
    [InlineData("https://www.paypal.com/signin", false)]
    [InlineData("http://sandbox.paypal.com/signin", false)]
    [InlineData("https://sandbox.paypal.com.evil.example/signin", false)]
    [InlineData("https://sandbox.paypal.com@evil.example/signin", false)]
    [InlineData("https://user@sandbox.paypal.com/signin", false)]
    [InlineData("not-a-url", false)]
    public void ProviderOrigin_RequiresHttpsSandbox(string url, bool expected)
        => Assert.Equal(expected, NativePayPalPage.IsSandboxUrl(url));

    [Theory]
    [InlineData("PayPal:Enabled", "false")]
    [InlineData("PayPal:AllowSandboxPayment", "false")]
    [InlineData("PayPal:CredentialSafeLoggingConfirmed", "false")]
    [InlineData("RunMode", "Remote")]
    [InlineData("Platform", "Android")]
    [InlineData("FaultInjection:Enabled", "true")]
    [InlineData("IOSLaunch:BuildProfile", "production")]
    public void Discovery_RejectsUnqualifiedExecution(string key, string value)
    {
        Dictionary<string, string?> settings = Settings();
        settings[key] = value;
        Assert.False(MobilePayPalFactAttribute.CanRun(new ConfigurationBuilder().AddInMemoryCollection(settings).Build()));
    }

    [Fact]
    public void Discovery_DefaultsToDisabled()
        => Assert.False(MobilePayPalFactAttribute.CanRun(new ConfigurationBuilder().Build()));

    [Fact]
    public void Discovery_PermitsExplicitLocalSitPrivacyOptIn()
        => Assert.True(MobilePayPalFactAttribute.CanRun(new ConfigurationBuilder().AddInMemoryCollection(Settings()).Build()));

    private static Dictionary<string, string?> Settings() => new()
    {
        ["RunMode"] = "Local",
        ["Platform"] = "iOS",
        ["IOSLaunch:BuildProfile"] = "sit_standard",
        ["PayPal:Enabled"] = "true",
        ["PayPal:AllowSandboxPayment"] = "true",
        ["PayPal:CredentialSafeLoggingConfirmed"] = "true",
    };
}
