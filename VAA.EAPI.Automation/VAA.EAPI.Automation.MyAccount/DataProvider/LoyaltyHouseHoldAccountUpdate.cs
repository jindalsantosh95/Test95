using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.AzureStorageTable;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.MyAccount.Constants;
using VAA.EAPI.Automation.MyAccount.Helpers;
using VAA.EAPI.Automation.MyAccount.Requests.Endpoint;
using VAA.EAPI.Infrastructure.AzureStorage;

namespace VAA.EAPI.Automation.MyAccount.DataProvider
{
    [Collection("Updates")]
    public sealed class LoyaltyHouseHoldAccountUpdate
    {
        private const string LoyaltyHouseHoldAccountsTableName = "LoyaltyHouseHoldAccounts";

        private readonly string _environment = Config.Environment;
        private readonly string _legacyPassword = Vault.Default.GetSecretValue("MYA-LegacyPassword");
        private readonly string _connectionString = Vault.Default.GetSecretValue("AzureStoragePnrCreationsConnectionString");

        [Fact]
        [Trait("Category", Traits.LoyaltyHouseHoldAccountDataUpdate)]
        public async Task UpdateLoyaltyHouseHoldAccountsTable()
        {
            TableStorageService<LoyaltyHouseHoldandHighFiveAccountsTableEntity> tableStorageService = new() { TableName = LoyaltyHouseHoldAccountsTableName, ConnectionString = _connectionString };

            List<LoyaltyHouseHoldandHighFiveAccountsTableEntity> householdRecords = (await tableStorageService.RetrieveItemsAsync(default)).Where(record => !record.RowKey.IsNullOrWhiteSpace() && record.PartitionKey.Is(_environment) && !record.ParentFirstName.IsNullOrWhiteSpace()).ToList();

            Assert.NotEmpty(householdRecords);

            foreach (LoyaltyHouseHoldandHighFiveAccountsTableEntity record in householdRecords)
            {
                string parentFlyingClubNumber = record.RowKey;
                string parentToken = await EntraTokenHelper.GetIdTokenWithPasswordFallback(_environment, string.Empty, parentFlyingClubNumber, Application.LoginPassword, _legacyPassword);

                HttpResponseMessage response = GraphQL.RetrieveHouseHoldAccount(_environment, parentToken, string.Empty, "Household").EnsureGQLResponseSuccess();
                JObject householdResponse = JObject.Parse(await response.Content.ReadAsStringAsync());

                string retrievedParentFlyingClubNumber = householdResponse.RequireToken("data.retrieveHouseHoldAccount.leaderInfo.fCNumber").ToString();
                Assert.True(parentFlyingClubNumber.Is(retrievedParentFlyingClubNumber), $"Expected household leader '{parentFlyingClubNumber}', but found '{retrievedParentFlyingClubNumber}'.");

                string parentFirstName = householdResponse.RequireToken("data.retrieveHouseHoldAccount.leaderInfo.firstName").ToString();
                string parentLastName = householdResponse.RequireToken("data.retrieveHouseHoldAccount.leaderInfo.lastName").ToString();
                string parentTierCode = householdResponse.RequireToken("data.retrieveHouseHoldAccount.leaderInfo.tierCode").ToString();
                JArray linkedMembers = Assert.IsType<JArray>(householdResponse.RequireToken("data.retrieveHouseHoldAccount.linkedMemberInfo"));

                if (linkedMembers.Count == 0)
                {
                    // Confirm the empty result twice more because linked-member data can lag after a household change.
                    for (int confirmationAttempt = 0; confirmationAttempt < 2; confirmationAttempt++)
                    {
                        await Task.Delay(2000);
                        HttpResponseMessage confirmationResponse = GraphQL.RetrieveHouseHoldAccount(_environment, parentToken, string.Empty, "Household").EnsureGQLResponseSuccess();
                        JObject confirmationHouseholdResponse = JObject.Parse(await confirmationResponse.Content.ReadAsStringAsync());
                        linkedMembers = Assert.IsType<JArray>(confirmationHouseholdResponse.RequireToken("data.retrieveHouseHoldAccount.linkedMemberInfo"));

                        if (linkedMembers.Count > 0)
                        {
                            break;
                        }
                    }

                    if (linkedMembers.Count == 0)
                    {
                        // Remove dissolved households so storage only offers accounts with active linked members.
                        await tableStorageService.DeleteAsync(new LoyaltyHouseHoldandHighFiveAccountsTableEntity() { PartitionKey = record.PartitionKey, RowKey = parentFlyingClubNumber });
                        continue;
                    }
                }

                // Normalize member identifiers so response ordering does not cause unnecessary storage updates.
                string childFlyingClubNumbers = string.Join(" , ", linkedMembers.Select(member => member.RequireToken("fCNumber").ToString()).Where(memberNumber => !memberNumber.IsNullOrWhiteSpace()).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(memberNumber => memberNumber, StringComparer.OrdinalIgnoreCase));
                string storedChildFlyingClubNumbers = string.Join(" , ", (record.ChildFCNumbers ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Where(memberNumber => !memberNumber.IsNullOrWhiteSpace()).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(memberNumber => memberNumber, StringComparer.OrdinalIgnoreCase));

                // Only merge a row when the live household state differs from the stored test-data baseline.
                bool hasChanged = !record.ParentFirstName.Is(parentFirstName) || !record.ParentLastName.Is(parentLastName) || !record.ParentTierCode.Is(parentTierCode) || !storedChildFlyingClubNumbers.Is(childFlyingClubNumbers);

                if (!hasChanged)
                {
                    continue;
                }

                // Merge only the latest household fields so manually changed member data becomes the next test-data baseline.
                await tableStorageService.InsertOrMergeAsync(new LoyaltyHouseHoldandHighFiveAccountsTableEntity() { PartitionKey = record.PartitionKey, RowKey = parentFlyingClubNumber, ParentFirstName = parentFirstName, ParentLastName = parentLastName, ParentTierCode = parentTierCode, ChildFCNumbers = childFlyingClubNumbers });
            }
        }

    }
}
