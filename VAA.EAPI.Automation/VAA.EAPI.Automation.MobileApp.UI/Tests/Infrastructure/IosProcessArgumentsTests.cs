using Microsoft.Extensions.Configuration;
using VAA.EAPI.Automation.Core.UI.Factory;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

public sealed class IosProcessArgumentsTests
{
    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void Resolve_WithoutFeatureFlags_PreservesUiTestingAndBuildProfile()
    {
        IConfiguration config = BuildConfig(new Dictionary<string, string?>
        {
            ["IOSLaunch:BuildProfile"] = "sit_standard",
        });

        IReadOnlyDictionary<string, object> processArguments = IosProcessArguments.Resolve(config);

        Assert.Equal(
            ["uitesting", "-build_profile", "sit_standard"],
            Assert.IsType<string[]>(processArguments["args"]));
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void Resolve_WithFeatureFlags_TrimsAndDeduplicatesValues()
    {
        IConfiguration config = BuildConfig(new Dictionary<string, string?>
        {
            ["IOSLaunch:BuildProfile"] = "uat_standard",
            ["IOSLaunch:AutomationFeatureFlags"] = " first-ios-flag, second_ios.flag, ,first-ios-flag ",
        });

        IReadOnlyDictionary<string, object> processArguments = IosProcessArguments.Resolve(config);

        Assert.Equal(
            [
                "uitesting",
                "-build_profile",
                "uat_standard",
                "-automationFeatureFlags",
                "first-ios-flag,second_ios.flag",
            ],
            Assert.IsType<string[]>(processArguments["args"]));
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void Resolve_WithUnsafeFeatureFlag_FailsBeforeDriverCreation()
    {
        IConfiguration config = BuildConfig(new Dictionary<string, string?>
        {
            ["IOSLaunch:AutomationFeatureFlags"] = "valid-flag;unexpected-command",
        });

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => IosProcessArguments.Resolve(config));

        Assert.Contains("Invalid iOS automation feature flag", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void Resolve_WithVisualRuntime_PinsAppearanceLocaleAndDefaultTextSize()
    {
        IConfiguration config = BuildConfig(new Dictionary<string, string?>
        {
            ["IOSLaunch:Appearance"] = "Light",
            ["IOSLaunch:Locale"] = "en-GB",
            ["IOSLaunch:TextSize"] = "Default",
        });

        IReadOnlyDictionary<string, object> processArguments = IosProcessArguments.Resolve(config);

        Assert.Equal(
            [
                "uitesting",
                "-build_profile",
                "sit_standard",
                "-AppleInterfaceStyle",
                "Light",
                "-AppleLanguages",
                "(en-GB)",
                "-AppleLocale",
                "en_GB",
                "-UIPreferredContentSizeCategoryName",
                "UICTContentSizeCategoryM",
            ],
            Assert.IsType<string[]>(processArguments["args"]));
    }

    private static IConfiguration BuildConfig(IReadOnlyDictionary<string, string?> values)
        => new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
}
