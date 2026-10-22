using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;

namespace VAA.EAPI.Automation.SearchAndBook.UI.Support;

public sealed record LoyaltyAccount(
    string FlyingClubNumber,
    string Tier,
    int Points,
    string FirstName,
    string LastName,
    string Email,
    string DateOfBirth);

public sealed record VoucherLoyaltyAccount(
    string FlyingClubNumber,
    string Tier,
    int Points,
    string VoucherPayload,
    IReadOnlyList<DateOnly> VoucherExpiryDates);

public static class LoyaltyAccountProvider
{
    public static VoucherLoyaltyAccount GetVoucherAccount(
        string voucherType = "",
        DateOnly? validOnOrAfter = null,
        DateOnly? expiresBefore = null,
        int? minimumPoints = null,
        int? maximumPoints = null,
        int? exactVoucherCount = null,
        int? minimumVoucherCount = null,
        bool requireAllVouchersInExpiryRange = false,
        bool requireDifferentVoucherExpiryDates = false)
    {
        string accountEnvironment = ResolveAccountEnvironment();
        Dictionary<string, string> account =
            RetrieveFromStorageTable.RetrieveLoyaltyVoucherAccountDetailsFromStorageTable(
                environmentOverride: accountEnvironment,
                voucherType: voucherType,
                minimumPoints: minimumPoints,
                maximumPoints: maximumPoints,
                minimumVoucherExpiryDate: validOnOrAfter?.ToDateTime(TimeOnly.MinValue),
                voucherExpiryBeforeDate: expiresBefore?.ToDateTime(TimeOnly.MinValue),
                exactVoucherCount: exactVoucherCount,
                minimumVoucherCount: minimumVoucherCount,
                requireAllVouchersInExpiryRange: requireAllVouchersInExpiryRange,
                requireDifferentVoucherExpiryDates: requireDifferentVoucherExpiryDates);

        if (account.TryGetValue("error", out string? error) && !error.IsNullOrWhiteSpace())
        {
            throw new InvalidOperationException($"Voucher loyalty account lookup failed: {error}");
        }

        string number = GetRequired(account, "rowKey");
        string pointsText = GetRequired(account, "points");
        if (!int.TryParse(pointsText, out int points))
        {
            throw new InvalidOperationException(
                $"Voucher loyalty account '{number}' returned invalid points '{pointsText}'.");
        }

        string voucherPayload = GetRequired(account, "vouchers");
        IReadOnlyList<DateOnly> expiryDates = ParseVoucherExpiryDates(voucherPayload);
        if (expiryDates.Count == 0)
        {
            throw new InvalidOperationException(
                $"Voucher loyalty account '{number}' did not contain a parseable voucher expiry date.");
        }

        return new VoucherLoyaltyAccount(
            number,
            GetRequired(account, "tier"),
            points,
            voucherPayload,
            expiryDates);
    }

    public static LoyaltyAccount GetAdultAccount(
        string tier,
        int minimumPoints,
        int? maximumPoints = null)
    {
        string accountEnvironment = ResolveAccountEnvironment();
        Dictionary<string, string> account = maximumPoints.HasValue
            ? RetrieveFromStorageTable.RetrieveLoyaltyAccountNumberDetailsFromStorageTable(
                tier,
                maximumPoints.Value,
                min: true,
                environment: accountEnvironment,
                minimumPoints: minimumPoints)
            : RetrieveFromStorageTable.RetrieveLoyaltyAccountWithoutVouchersFromStorageTable(
                tier,
                minimumPoints,
                min: false,
                environmentOverride: accountEnvironment);

        if (account.TryGetValue("error", out string? error) && !error.IsNullOrWhiteSpace())
        {
            throw new InvalidOperationException($"Loyalty account lookup failed: {error}");
        }

        string number = GetRequired(account, "rowKey");
        string pointsText = GetRequired(account, "points");
        if (!int.TryParse(pointsText, out int points) ||
            points < minimumPoints ||
            (maximumPoints.HasValue && points >= maximumPoints.Value))
        {
            throw new InvalidOperationException(
                $"Loyalty account '{number}' did not meet the requested points range " +
                $"{minimumPoints} to {(maximumPoints.HasValue ? maximumPoints.Value - 1 : int.MaxValue)}. " +
                $"Actual: '{pointsText}'.");
        }

        return new LoyaltyAccount(
            number,
            GetRequired(account, "tier"),
            points,
            GetRequired(account, "firstName"),
            GetRequired(account, "lastName"),
            GetRequired(account, "email"),
            GetRequired(account, "dob"));
    }

    private static string ResolveAccountEnvironment()
    {
        string? configuredEnvironment = Environment.GetEnvironmentVariable("SAB_LOYALTY_ENVIRONMENT");
        if (!configuredEnvironment.IsNullOrWhiteSpace())
        {
            return configuredEnvironment;
        }

        string? baseUrl = Environment.GetEnvironmentVariable("SAB_BASE_URL");
        if (Uri.TryCreate(baseUrl, UriKind.Absolute, out Uri? uri) &&
            uri.Host.Equals("stg.virginatlantic.com", StringComparison.OrdinalIgnoreCase))
        {
            return "uat";
        }

        return Config.Environment;
    }

    private static IReadOnlyList<DateOnly> ParseVoucherExpiryDates(string voucherPayload)
    {
        using System.Text.Json.JsonDocument document =
            System.Text.Json.JsonDocument.Parse(voucherPayload);
        if (document.RootElement.ValueKind != System.Text.Json.JsonValueKind.Array)
        {
            return [];
        }

        return document.RootElement
            .EnumerateArray()
            .Select(voucher => voucher.TryGetProperty("expiryDate", out System.Text.Json.JsonElement expiry)
                ? expiry.GetString()
                : null)
            .Where(value => DateOnly.TryParse(value, out _))
            .Select(value => DateOnly.Parse(value!))
            .OrderBy(value => value)
            .ToArray();
    }

    private static string GetRequired(IReadOnlyDictionary<string, string> values, string key)
    {
        if (!values.TryGetValue(key, out string? value) || value.IsNullOrWhiteSpace())
        {
            throw new InvalidOperationException($"Loyalty account lookup did not return required field '{key}'.");
        }

        return value;
    }
}
