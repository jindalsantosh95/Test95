using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.Core.UI.Factory;

public static class IosProcessArguments
{
    private static readonly Regex SafeFeatureFlag = new(
        "^[A-Za-z0-9._-]+$",
        RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1));

    public static IReadOnlyDictionary<string, object> Resolve(IConfiguration config)
    {
        string buildProfile = ResolveConfiguredBuildProfile(config) ?? "sit_standard";
        string configuredFeatureFlags = config["IOS_AUTOMATION_FEATURE_FLAGS"]
            ?? config["IOSLaunch:AutomationFeatureFlags"]
            ?? string.Empty;
        string[] featureFlags = configuredFeatureFlags
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        foreach (string featureFlag in featureFlags)
        {
            if (!SafeFeatureFlag.IsMatch(featureFlag))
            {
                throw new InvalidOperationException(
                    $"Invalid iOS automation feature flag: {featureFlag}");
            }
        }

        List<string> arguments = ["uitesting", "-build_profile", buildProfile];
        if (featureFlags.Length > 0)
        {
            arguments.Add("-automationFeatureFlags");
            arguments.Add(string.Join(',', featureFlags));
        }

        AddVisualRuntimeArguments(arguments, config);

        return new Dictionary<string, object>
        {
            ["args"] = arguments.ToArray(),
        };
    }

    public static string ResolveConfiguredBuildProfile(IConfiguration config)
        => config["IOS_BUILD_PROFILE"] ?? config["IOSLaunch:BuildProfile"];

    private static void AddVisualRuntimeArguments(List<string> arguments, IConfiguration config)
    {
        string appearance = config["IOSLaunch:Appearance"]?.Trim();
        string locale = config["IOSLaunch:Locale"]?.Trim();
        string textSize = config["IOSLaunch:TextSize"]?.Trim();

        if (!appearance.IsNullOrWhiteSpace())
        {
            arguments.Add("-AppleInterfaceStyle");
            arguments.Add(appearance);
        }

        if (!locale.IsNullOrWhiteSpace())
        {
            arguments.Add("-AppleLanguages");
            arguments.Add($"({locale})");
            arguments.Add("-AppleLocale");
            arguments.Add(locale.Replace('-', '_'));
        }

        if (!textSize.IsNullOrWhiteSpace())
        {
            if (!textSize.Is("Default"))
            {
                throw new InvalidOperationException(
                    "IOSLaunch:TextSize currently supports only the pinned Default value.");
            }

            arguments.Add("-UIPreferredContentSizeCategoryName");
            arguments.Add("UICTContentSizeCategoryM");
        }
    }
}
