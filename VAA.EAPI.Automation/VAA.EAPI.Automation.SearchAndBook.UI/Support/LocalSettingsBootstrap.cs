using System.Runtime.CompilerServices;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;

namespace VAA.EAPI.Automation.SearchAndBook.UI.Support;

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
                    "VAA.EAPI.Automation.SearchAndBook.UI");
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
}
