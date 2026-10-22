using System.Runtime.CompilerServices;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;

namespace VAA.EAPI.Automation.Trips.UI.Support;

internal static class LocalSettingsBootstrap
{
    private const string SettingsFileName = "local.settings.local.json";

    [ModuleInitializer]
    internal static void Load()
    {
        string? settingsDirectory = FindSettingsDirectory();
        if (settingsDirectory is not null)
        {
            ConfigReader.LoadEnvironmentSettingsFile(
                SettingsFileName,
                optional: true,
                forceOverride: false,
                root: settingsDirectory);
        }

        // ConfigReader performs case-insensitive lookups through lower-case process keys.
        // Environment variables are case-sensitive on macOS/Linux, so always retain the
        // externally supplied names while also supplying the Core-compatible aliases.
        SetLowerCaseAlias("KeyVaultUrl");
        SetLowerCaseAlias("KeyVaultTenantId", "KeyVaulttenantId");
        SetLowerCaseAlias("KeyVaultClientId", "KeyVaultclientId");
        SetLowerCaseAlias("KeyVaultClientSecret", "KeyVaultclientSecret");

        Map("TRIPS_UI_BROWSER", "SAB_BROWSER");
        Map("TRIPS_UI_HEADLESS", "SAB_HEADLESS");
        Map("TRIPS_UI_ARTIFACT_DIR", "SAB_ARTIFACT_DIR");
        Map("TRIPS_UI_RUN_MODE", "SAB_RUN_MODE");
        Map("TRIPS_UI_BS_OS", "SAB_BS_OS");
        Map("TRIPS_UI_BS_OS_VERSION", "SAB_BS_OS_VERSION");
        Map("TRIPS_UI_BS_BROWSER", "SAB_BS_BROWSER");
        Map("TRIPS_UI_BS_BROWSER_VERSION", "SAB_BS_BROWSER_VERSION");
        Map("TRIPS_UI_BS_DEVICE_NAME", "SAB_BS_DEVICE_NAME");
        Map("TRIPS_UI_BS_SESSION_NAME", "SAB_BS_SESSION_NAME");
        Map("TRIPS_UI_BS_PROJECT", "BROWSERSTACK_PROJECT");
        Map("TRIPS_UI_BS_BUILD", "BROWSERSTACK_BUILD");

        SetIfMissing("BROWSERSTACK_PROJECT", "Virgin Atlantic Web - Trips UI");
        SetIfMissing("BROWSERSTACK_BUILD", "Trips UI - Local");
        SetIfMissing("SAB_BS_SESSION_NAME", "Trips UI - Retrieve Trip");
    }

    private static void Map(string source, string target)
    {
        if (Environment.GetEnvironmentVariable(target) is null)
        {
            string? value = Environment.GetEnvironmentVariable(source);
            if (!value.IsNullOrWhiteSpace())
            {
                Environment.SetEnvironmentVariable(target, value);
            }
        }
    }

    private static string? FindSettingsDirectory()
    {
        foreach (string startPath in new[] { Environment.CurrentDirectory, AppContext.BaseDirectory })
        {
            DirectoryInfo? directory = new(startPath);
            while (directory is not null)
            {
                string directSettingsPath = Path.Combine(directory.FullName, SettingsFileName);
                if (File.Exists(directSettingsPath))
                {
                    return directory.FullName;
                }

                string projectDirectory = Path.Combine(
                    directory.FullName,
                    "VAA.EAPI.Automation.Trips.UI");
                if (File.Exists(Path.Combine(projectDirectory, SettingsFileName)))
                {
                    return projectDirectory;
                }

                directory = directory.Parent;
            }
        }

        return null;
    }

    private static void SetLowerCaseAlias(string canonicalName, string? existingName = null)
    {
        string? value = Environment.GetEnvironmentVariable(canonicalName) ??
            (existingName is null ? null : Environment.GetEnvironmentVariable(existingName));
        if (!value.IsNullOrWhiteSpace())
        {
            Environment.SetEnvironmentVariable(canonicalName.ToLowerInvariant(), value);
        }
    }

    private static void SetIfMissing(string name, string value)
    {
        if (Environment.GetEnvironmentVariable(name).IsNullOrWhiteSpace())
        {
            Environment.SetEnvironmentVariable(name, value);
        }
    }
}
