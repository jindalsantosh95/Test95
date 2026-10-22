using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.AzureStorageTable;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Infrastructure.AzureStorage;

namespace VAA.EAPI.Automation.OCI.Helpers
{
    public static class OCIHelper
    {
        public static string env = Core.Helpers.ConfigReader.Get("env").ToString();

        public static string connectionString = Vault.Default.GetSecretValue("AzureStoragePnrCreationsConnectionString");

        private static Random rand = new Random();

        public static async Task OCIPNRDeletionFromStorageTableAsync(string tableName)
        {
            var tableStorageService = new TableStorageService<CreatedPnrsTableEntity>
            {
                TableName = tableName,
                ConnectionString = connectionString
            };

            var results = (await tableStorageService.RetrieveItemsAsync(default))
                          .Where(x => !string.IsNullOrEmpty(x.RowKey) && x.Env.Is(Config.Environment));

            foreach (var record in results)
            {
                await tableStorageService.DeleteAsync(record);
            }
        }



        public static async Task OCIPNRDeletionExpiredDataFromStorageTableAsync(string tableName)
        {
            var tableStorageService = new TableStorageService<CreatedPnrsTableEntity>
            {
                TableName = tableName,
                ConnectionString = connectionString
            };

            var nowUtc = DateTime.UtcNow;

            var results = (await tableStorageService.RetrieveItemsAsync(default))
                .Where(x => !string.IsNullOrEmpty(x.RowKey) && x.DepartureDate.Parse<DateTime>() < nowUtc);

            foreach (var record in results)
            {
                await tableStorageService.DeleteAsync(record);
            }
        }

        public static string GetDateFromTodayPlusDays(int days)
        {
            return DateTime.UtcNow.AddDays(days).ToString("yyyy-MM-ddT00:00:00");
        }

        public static string FormatBirthDate(string rawBirthDate)
        {
            if (string.IsNullOrWhiteSpace(rawBirthDate))
                return null;

            return DateTime.TryParse(rawBirthDate, out var parsedDate) ? parsedDate.ToString("yyyy-MM-dd") : null;
        }

        public static List<string> GetPassengersListFromCheckInResponse(HttpResponseMessage jsonResponse)
        {
            var result = JsonConvert.DeserializeObject<JToken>(jsonResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            // Extract passenger IDs as a list
            return result?.SelectTokens("$..passengerId").Where(t => t.Type != JTokenType.Null).Select(t => t.ToString()).ToList();
        }
    }
}