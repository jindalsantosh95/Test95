using Microsoft.Azure.Cosmos.Table;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Infrastructure.AzureStorage;

namespace VAA.EAPI.Automation.Core.Helpers;

public static class OtpStorageCodeReader
{
    public static async Task<int> GetByEmailIdAsync(
        string emailId,
        int expectedOtpLength = 6,
        int maxAttempts = 30,
        DateTimeOffset? newerThan = null)
    {
        if (emailId.IsNullOrWhiteSpace())
        {
            return 0;
        }

        TableStorageService<OtpRequestTableEntity> tableStorageService = CreateTableStorageService();

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            OtpRequestTableEntity record = await tableStorageService.RetrieveAsync(
                "OTPGeneration",
                emailId.ToLowerInvariant());
            bool hasExpectedLength = expectedOtpLength == 0 ||
                record?.OTP.ToString().Length == expectedOtpLength;
            bool isFresh = newerThan is null || record?.Timestamp > newerThan;

            if (record is not null && hasExpectedLength && isFresh)
            {
                return record.OTP;
            }

            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        return 0;
    }

    public static async Task<DateTimeOffset?> GetTimestampByEmailIdAsync(string emailId)
    {
        if (emailId.IsNullOrWhiteSpace())
        {
            return null;
        }

        TableStorageService<OtpRequestTableEntity> tableStorageService = CreateTableStorageService();
        OtpRequestTableEntity record = await tableStorageService.RetrieveAsync(
            "OTPGeneration",
            emailId.ToLowerInvariant());
        return record?.Timestamp;
    }

    private static TableStorageService<OtpRequestTableEntity> CreateTableStorageService()
    {
        string environment = Config.Environment;
        string connectionString = Vault.Default.GetSecretValue(
            $"IdentityStorageAccountConnectionString-{environment}");
        return new TableStorageService<OtpRequestTableEntity>
        {
            ConnectionString = connectionString,
            TableName = "OTPRequest",
        };
    }

    private sealed class OtpRequestTableEntity : TableEntity
    {
        public int OTP { get; set; }
    }
}
