using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.MobileApp.UI.Models;
using CoreConfigReader = VAA.EAPI.Automation.Core.Helpers.ConfigReader;

namespace VAA.EAPI.Automation.MobileApp.UI.Data;

/// <summary>
/// Typed, parallel-safe adapter over the loyalty-account pool already used by EAPI backend tests.
/// A lease prevents two tests in this process from selecting the same mutable account concurrently.
/// </summary>
public sealed class LoyaltyAccountProvider
{
    private const int MaximumSelectionAttempts = 12;
    private static readonly object RetrievalLock = new();
    private static readonly ConcurrentDictionary<string, byte> ActiveAccounts = new(StringComparer.OrdinalIgnoreCase);
    private readonly IConfiguration _config;

    public LoyaltyAccountProvider(IConfiguration config)
    {
        _config = config;
    }

    public LoyaltyAccountLease Lease(LoyaltyAccountCriteria criteria)
    {
        Validate(criteria);

        for (int attempt = 1; attempt <= MaximumSelectionAttempts; attempt++)
        {
            Dictionary<string, string> details;
            lock (RetrievalLock)
            {
                ConfigureCore(criteria.Environment);
                details = RetrieveFromStorageTable.RetrieveLoyaltyAccountNumberDetailsFromStorageTable(
                    criteria.Tier,
                    points: criteria.MaximumPoints ?? -1,
                    min: criteria.MaximumPoints.HasValue,
                    underSixteen: criteria.UnderSixteen,
                    environment: criteria.Environment,
                    minimumPoints: criteria.MinimumPoints);
            }

            if (details.TryGetValue("error", out string? error))
            {
                throw new InvalidOperationException(
                    $"No loyalty account matched {Describe(criteria)} in the LoyaltyAccounts pool. {error}");
            }

            LoyaltyAccount account = Map(details);
            EnsureMatches(account, criteria);
            if (ActiveAccounts.TryAdd(account.FlyingClubNumber, 0))
            {
                return new LoyaltyAccountLease(account, Release);
            }
        }

        throw new InvalidOperationException(
            $"Could not reserve a unique loyalty account after {MaximumSelectionAttempts} selections. " +
            "The current parallel run may be larger than the eligible account pool.");
    }

    public string GetPassword() => RequiredSetting("Loyalty:Password");

    private void ConfigureCore(string environment)
    {
        SetCoreValue("KeyVaultUrl", RequiredSetting("Core:KeyVaultUrl"));
        SetCoreValue("KeyVaultTenantId", _config["Core:KeyVaultTenantId"]);
        SetCoreValue("KeyVaultClientId", _config["Core:KeyVaultClientId"]);
        SetCoreValue("KeyVaultClientSecret", _config["Core:KeyVaultClientSecret"]);
        SetCoreValue("env", environment);
    }

    private static void SetCoreValue(string key, string? value)
    {
        string configuredValue = (value ?? string.Empty).Trim();
        if (configuredValue.StartsWith("replace-with-", StringComparison.OrdinalIgnoreCase))
        {
            configuredValue = string.Empty;
        }

        // ConfigReader normalises lookup keys to lower case on macOS/Linux. Writing empty values
        // also clears cached/process credentials so another test run cannot leak into this one.
        CoreConfigReader.Set(key.ToLowerInvariant(), configuredValue);
    }

    private static void Validate(LoyaltyAccountCriteria criteria)
    {
        ArgumentNullException.ThrowIfNull(criteria);
        if (criteria.Environment.IsNullOrWhiteSpace())
        {
            throw new ArgumentException("A loyalty account environment is required.", nameof(criteria.Environment));
        }

        if (criteria.Tier.IsNullOrWhiteSpace())
        {
            throw new ArgumentException("A loyalty account tier is required.", nameof(criteria.Tier));
        }

        if (criteria.MinimumPoints < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(criteria.MinimumPoints),
                "Minimum points cannot be negative.");
        }

        if (criteria.MaximumPoints.HasValue && criteria.MaximumPoints.Value <= criteria.MinimumPoints)
        {
            throw new ArgumentOutOfRangeException(
                nameof(criteria.MaximumPoints),
                "Maximum points is exclusive and must be greater than minimum points.");
        }
    }

    private static void EnsureMatches(LoyaltyAccount account, LoyaltyAccountCriteria criteria)
    {
        bool pointsMatch = account.Points >= criteria.MinimumPoints &&
            (!criteria.MaximumPoints.HasValue || account.Points < criteria.MaximumPoints.Value);
        if (!account.Tier.Equals(criteria.Tier, StringComparison.OrdinalIgnoreCase) || !pointsMatch)
        {
            throw new InvalidOperationException(
                $"Core returned an account outside the requested criteria ({Describe(criteria)}). " +
                "The account was rejected before app login.");
        }
    }

    private static string Describe(LoyaltyAccountCriteria criteria) =>
        $"environment={criteria.Environment}, tier={criteria.Tier}, " +
        $"points=[{criteria.MinimumPoints}, {(criteria.MaximumPoints?.ToString() ?? "unbounded")}), " +
        $"underSixteen={criteria.UnderSixteen}";

    private static LoyaltyAccount Map(IReadOnlyDictionary<string, string> details)
    {
        string flyingClubNumber = RequiredValue(details, "rowKey");
        string email = details.GetValueOrDefault("email")?.Trim() ?? string.Empty;
        string loginIdentifier = email.IsNullOrWhiteSpace() ? flyingClubNumber : email;
        _ = int.TryParse(details.GetValueOrDefault("points"), out int points);

        return new LoyaltyAccount(
            loginIdentifier,
            flyingClubNumber,
            RequiredValue(details, "firstName"),
            RequiredValue(details, "lastName"),
            RequiredValue(details, "tier"),
            points);
    }

    private static string RequiredValue(IReadOnlyDictionary<string, string> details, string key)
    {
        string value = details.GetValueOrDefault(key)?.Trim() ?? string.Empty;
        return value.IsNullOrWhiteSpace()
            ? throw new InvalidOperationException($"The selected loyalty account has no '{key}' value.")
            : value;
    }

    private string RequiredSetting(string key)
    {
        string value = _config[key]?.Trim() ?? string.Empty;
        if (value.IsNullOrWhiteSpace() || value.StartsWith("replace-with-", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Set '{key}' in the gitignored VAA.EAPI.Automation.MobileApp.UI/local.settings.local.json before running loyalty tests.");
        }

        return value;
    }

    private static void Release(string flyingClubNumber) => ActiveAccounts.TryRemove(flyingClubNumber, out _);
}

public sealed class LoyaltyAccountLease : IDisposable
{
    private readonly Action<string> _release;
    private bool _disposed;

    internal LoyaltyAccountLease(LoyaltyAccount account, Action<string> release)
    {
        Account = account;
        _release = release;
    }

    public LoyaltyAccount Account { get; }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _release(Account.FlyingClubNumber);
    }
}
