using Azure.Data.Tables;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Trips.Constants;

namespace VAA.EAPI.Automation.Trips.Tests.TripsTests.TestData
{
    [Collection("TestDataCleanUp")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class TestDataCleanUpTests
    {
        [Theory]
        [Trait("Category", Traits.TestDataScripts)]
        [InlineData("PNRs")]
        [InlineData("MobileAppPnrs")]
        [InlineData("MobileAppPnrsForLoyaltyAccounts")]
        [InlineData("MobileAppPnrsForSeatScenarios")]
        public async Task DeleteRecordsBeforeCurrentWeekFromStorage(string tableName)
        {
            #region Test Data - Delete PNR from the Storage Table "PNRs" created last week

            // Calculate current week's Monday
            DateTime today = DateTime.UtcNow;
            DateTime currentWeekMonday = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
            currentWeekMonday = currentWeekMonday.Date; // Set to midnight

            // Get initial count of records from last week
            int initialCount = await GetOldRecordsCount(tableName, currentWeekMonday);

            // Perform deletion
            await DeleteFromStorageTable.DeleteFromStorage(tableName, currentWeekMonday);

            // Get count after deletion
            int finalCount = await GetOldRecordsCount(tableName, currentWeekMonday);

            // Assertions
            if (initialCount > 0)
            {
                Assert.True(finalCount < initialCount, "Records should have been deleted");
            }
            else
            {
                // If there were no records to begin with, test should pass
                Assert.Equal(0, finalCount);
            }
            #endregion
        }

        // Helper method to get count of all records except the current week records
        private async Task<int> GetOldRecordsCount(string tableName, DateTime currentWeekMonday)
        {
            TableClient tableClient = new(DeleteFromStorageTable.connectionString, tableName);

            // Query for records before current week's Monday
            Azure.AsyncPageable<TableEntity> query = tableClient.QueryAsync<TableEntity>(filter:
                $"Timestamp lt datetime'{currentWeekMonday:yyyy-MM-dd}T00:00:00.0000000Z'");

            int count = 0;
            await foreach (TableEntity entity in query)
            {
                count++;
            }
            return count;
        }
    }
}
