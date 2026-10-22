using System.Globalization;
using System.Text.Json;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.Requests;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using Xunit;

namespace VAA.EAPI.Automation.Core.Helpers
{
    public class FlyingClubMemberHelper
    {

        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        public string loyaltyEnv = Config.Environment;

        public string vRedocpSubKey = Vault.Default.GetSecretValue("TestingVRedMSSubscriptionKey");

        public FlyingClubMemberHelper()
        {
            tokenFixture = new();

            tokenFixture.AddTokenForDynamicsEnrolmentFromKeyVault("enrolment");
            tokenFixture.AddTokenForCustomerApiFromKeyVault("customer");
            tokenFixture.AddTokenForVRedMicroserviceFromKeyVault("VRedMS");

            cosmosFixture = new();

            customerCosmosDb = loyaltyEnv.Is("uat") ? "CustomerDB_UAT" : "CustomerDB_Acc";
            customerTransactionCosmosDb = loyaltyEnv.Is("uat") ? "CustomerTransactionsDB_UAT" : "CustomerTransactionsDB_Acc";

            string cosmosEndpointForCustomerTrx = "https://avadeuncmdbcustomertransactions.documents.azure.com:443/";

            cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));
            cosmosFixture.RegisterType<MembershipChange>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, "CustomerDetailsCollection");

            string cosmosEndpoint = "https://avadeuncmdbcustomer.documents.azure.com:443/";

            cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));
            cosmosFixture.RegisterType<DB_Model.Entities.Customer>(cosmosEndpoint, customerCosmosDb, "CustomerCollection");
        }

        /// <summary>
        /// Updated/Changes the Member Tier Of a FlyingClub Member provided the memberNumber and NewTierCode (Current TierCode and NewTierCode shall not be same)
        /// </summary> 
        public async Task UpgradeTheFlyingClubMemberTier(string memberNumber, FlyingClubMemberTiers newTierCode, string customerId = "")
        {
            DB_Model.Entities.Customer customerDoc = null;

            string custId = customerId;

            int retry = 5;

            if (string.IsNullOrEmpty(custId))
            {

                string queryForCustomerDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.LoyaltyMemberNumber = @memberNumber ";

                Microsoft.Azure.Cosmos.QueryDefinition paramQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@memberNumber", memberNumber);

                customerDoc = cosmosFixture.CosmosQueryItem<DB_Model.Entities.Customer>(paramQuery);

                while (customerDoc is null && retry > 0)
                {
                    await Task.Delay(500);
                    customerDoc = cosmosFixture.CosmosQueryItem<DB_Model.Entities.Customer>(paramQuery);
                    retry--;

                }

                Assert.True(customerDoc is not null, $"Customer Doc Not found for the MemberNumber {memberNumber}.");

                custId = customerDoc.CustomerId;
            }

            string queryForMembershipChangeDocument = "SELECT * FROM CustomerDetailsCollection c where c.DocType = @docType and  c.PartitionKey = @customerId and c.Active = @activeFlag";

            Microsoft.Azure.Cosmos.QueryDefinition query = new QueryDefinition(query: queryForMembershipChangeDocument).WithParameter("@docType", "MembershipChange").WithParameter("@customerId", custId).WithParameter("@activeFlag", "Y");

            MembershipChange membershipChangeDoc = cosmosFixture.CosmosQueryItem<MembershipChange>(query);

            Assert.True(membershipChangeDoc is not null, $"No Active MembershipChange/Tier Doc Not found for the MemberNumber {memberNumber}.");

            if (!membershipChangeDoc.TierCode.Is(newTierCode.ToString()))
            {
                string updatedOn = DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");

                string tierStartDate = DateTime.UtcNow.AddDays(1).ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");

                string tierEndDate = DateTime.UtcNow.AddMonths(13).AddDays(-Convert.ToInt32(DateTime.UtcNow.Day)).Date.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");

                string docId = membershipChangeDoc.Id;

                int revision = !string.IsNullOrEmpty(membershipChangeDoc.Revision.ToString()) ? Convert.ToInt32(Convert.ToDouble(membershipChangeDoc.Revision)) + 1 : 1;

                #region Creating a New Tier API Call

                System.Net.Http.HttpResponseMessage response = Endpoints.CreateMemberTier(loyaltyEnv, custId, tokenFixture.GetToken("customer"), updatedOn, tierStartDate, tierEndDate, revision, newTierCode.ToString(), "").EnsureIsSuccessStatusCode();

                dynamic result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

                string expectedResponse = "Tier Created Successfully";

                Assert.True(result.ToString().Contains(expectedResponse),
                    $"Expected the message to be {expectedResponse} but found {result.ToString()}");

                #endregion

                #region Fetching Document Id for the Newly Created Tier

                queryForMembershipChangeDocument = "SELECT * FROM CustomerDetailsCollection c where c.DocType = @docType and  c.PartitionKey = @partitionKey and c.Active = @activeFlag order by c.CreatedOn desc";

                query = new QueryDefinition(query: queryForMembershipChangeDocument).WithParameter("@docType", "MembershipChange").WithParameter("@partitionKey", custId).WithParameter("@activeFlag", "N");

                membershipChangeDoc = cosmosFixture.CosmosQueryItem<MembershipChange>(query);

                retry = 5;

                if ((membershipChangeDoc == null || membershipChangeDoc.Id == docId) && retry > 0)
                {
                    await Task.Delay(500);

                    membershipChangeDoc = cosmosFixture.CosmosQueryItem<MembershipChange>(query);

                    retry--;
                }

                Assert.True(membershipChangeDoc is not null,
                    $"No Document Available for the searched query {queryForMembershipChangeDocument}.");

                docId = membershipChangeDoc.Id;

                #endregion

                #region Updating the Newly Created Tier API CALL

                response = Endpoints.UpdateMemberTier(loyaltyEnv, custId, tokenFixture.GetToken("customer"), docId, updatedOn, tierStartDate, tierEndDate, revision).EnsureIsSuccessStatusCode();

                result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

                expectedResponse = "Tier Details have been saved successfully";

                Assert.True(result.message.ToString().Equals(expectedResponse),
                    $"Expected the message to be {expectedResponse} but found {result.ToString()}");

                #endregion
            }
        }


        /// <summary>
        /// Adds Loyalty Points to the FlyingClub Member provided the MemberNumber and the No Of Points to Credit
        /// </summary> 
        public async Task<bool> AddLoyaltyPointsToTheFlyingClubAccount(string memberNumber, int noOfPoints)
        {
            bool result = false;

            string queryForCustomerDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.LoyaltyMemberNumber = @memberNumber ";

            Microsoft.Azure.Cosmos.QueryDefinition paramQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@memberNumber", memberNumber);

            DB_Model.Entities.Customer customerDoc = cosmosFixture.CosmosQueryItem<DB_Model.Entities.Customer>(paramQuery);

            Assert.True(customerDoc is not null, $"Customer Doc Not found for the MemberNumber {memberNumber}.");

            string activityDt = DataGenerator.RandomPastDate(10).ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture);

            await RetryHelper.RunWithRetryAsync(async () =>
            {
                System.Net.Http.HttpResponseMessage txnresponse = Endpoints.CreateTransaction(loyaltyEnv, tokenFixture.GetToken("VRedMS"), vRedocpSubKey, customerDoc.CustomerId, memberNumber, "VS", "Authorised Points Credit", activityDt, "Test txn", 0, noOfPoints, "Virgin Flying Club Manual Adjustment", "Virgin Atlantic Airways", "", "Manual Adjustment", "AutomationScript", "AutomationScript@test.com", "PAUSER", "Virgin Flying Club Manual Adjustment", "Accrual").EnsureIsAcceptedStatusCode();

                string responseUrl = txnresponse?.Headers?.Location?.ToString();

                System.Net.Http.HttpResponseMessage actualTxnResponse = Endpoints.GetTransactionResponse(tokenFixture.GetToken("VRedMS"), vRedocpSubKey, responseUrl).EnsureIsSuccessStatusCode();

                string content = await actualTxnResponse.Content.ReadAsStringAsync();

                string partnerTransactionId = JsonDocument.Parse(JsonDocument.Parse(content).RootElement.GetProperty("value").GetString()).RootElement.GetProperty("partnerTransactionId").GetString();

                Assert.True(!partnerTransactionId.IsNullOrEmpty(), $"Transaction creation failed for the member {memberNumber}. The response returned is \n {actualTxnResponse}");

                result = true;

            }, 5, 1000);

            return result;
        }


        /// <summary>
        /// Returns the Current Usable Point Balance of the FlyingClub Member provided the MemberNumber
        /// </summary> 
        public async Task<long> GetPointBalanceOfFlyingClubAccount(string memberNumber)
        {
            int retry = 5;

            System.Net.Http.HttpResponseMessage response = Endpoints.GetMemberPointBalance(loyaltyEnv, tokenFixture.GetToken("VRedMS"), vRedocpSubKey, memberNumber).EnsureIsSuccessStatusCode();
            //Retry to add in code
            dynamic result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            long usableBalance = string.IsNullOrEmpty(result.Usable?.ToString()) ? 0 : Convert.ToInt64(result.Usable);

            while (retry > 0 && usableBalance == 0)
            {
                await Task.Delay(1000);

                response = Endpoints.GetMemberPointBalance(loyaltyEnv, tokenFixture.GetToken("VRedMS"), vRedocpSubKey, memberNumber).EnsureIsSuccessStatusCode();

                result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

                usableBalance = string.IsNullOrEmpty(result.Usable?.ToString()) ? 0 : Convert.ToInt64(result.Usable);

                retry--;
            }

            return usableBalance;
        }


        public static HttpResponseMessage Enrolment(string env, string channelID, string firstName, string lastName, string emailId, string postCode, string countryCode, string password = "Virgin123", string dateOfBirth = "")
        {
            if (dateOfBirth.IsNullOrWhiteSpace())
                dateOfBirth = "1990-01-12";

            return WebRequest.Json.Post(Config.TestResourcesVirginAtlantic.GetUrl($"/{env}{ConfigReader.Get("graphQlMyAccountPublic")}"),
                Factory.GraphQL.Enrolment(channelID, firstName, lastName, emailId, postCode, countryCode, password, dateOfBirth));
        }

        public async Task AwardVoucherToFlyingClubAccount(
            string customerId,
            string memberNumber,
            string voucherName,
            bool expiresWithinSixMonths = false,
            int? expiryMonthsFromNow = null)
        {
            DateTime date = DateTime.UtcNow;

            string startDate = date.ToString("yyyy-MM-dd");

            if (expiryMonthsFromNow.HasValue)
            {
                Assert.True(
                    expiryMonthsFromNow.Value is >= 1 and <= 36,
                    "An explicit voucher expiry offset must be between one and 36 months.");
            }

            string expiryDate = expiryMonthsFromNow.HasValue
                ? date.AddMonths(expiryMonthsFromNow.Value).ToString("yyyy-MM-dd")
                : expiresWithinSixMonths
                    ? date.AddMonths(Random.Shared.Next(1, 7)).ToString("yyyy-MM-dd")
                    : date.AddYears(2).ToString("yyyy-MM-dd");

            string createdByUserName = DataGenerator.RandomFirstName();

            string createdByEmailId = $"{createdByUserName}@test.com";

            HttpResponseMessage response = Endpoints.AwardVoucher(customerId, voucherName, startDate, expiryDate, "AutomationScript", createdByEmailId, createdByUserName).EnsureIsSuccessStatusCode();

            string expectedResponse = "Voucher rewarded successfully";

            string actualResponse = await response.Content.ReadAsStringAsync();

            Assert.True(actualResponse.Has(expectedResponse),
                $"For member {memberNumber} (customerId {customerId}), expected '{expectedResponse}' but was '{actualResponse}'.");
        }
    }
}
