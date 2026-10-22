using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Helpers;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;
using Application = VAA.EAPI.Automation.MarketingLoyalty.Constants.Application;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.TestDataCreation
{
    [Collection("TestDataCreation")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class TestDataCreation : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {

        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;
        private readonly PreRequisiteTestData preRequisiteTestData;

        public static string env = Config.Environment;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        private static string backendAPIEnv = ConfigReader.Get("BackendAPIEnv") ?? "";

        public TestDataCreation(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {
            this.tokenFixture = tokenFixture;
            this.cosmosFixture = cosmosFixture;

            if (!backendAPIEnv.Is("Project-DEV"))
                this.tokenFixture.AddTokenForCustomerApiFromKeyVault("customer");

            preRequisiteTestData = new();

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            customerCosmosDb = env.Equals("uat", StringComparison.InvariantCultureIgnoreCase) ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Equals("uat", StringComparison.InvariantCultureIgnoreCase) ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));
            this.cosmosFixture.RegisterType<MembershipChange>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            this.cosmosFixture.RegisterType<Award>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);


            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));
            this.cosmosFixture.RegisterType<Core.DB_Model.Entities.Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

        }

        public static string tierName = "Gold";

        [Trait("Category", Traits.TestDataCreationRegression)]
        [Fact, RunOrder(1)]
        public async Task MemberEnrolmentSuccessScenario()
        {

            int count = 20;

            string[] titles = { "Mr", "Mrs", "Ms", "Miss", "Mstr" };
            string postCode = "RH11 9LG";
            string country = "GB";
            string channelID = "POWERAPP";

            while (count > 0) // 20 Enrolments using PowerApps Channel
            {
                #region PreRequisite to create an enrolment before validations for the enrolment

                Random random = new();
                string title = titles[random.Next(titles.Length)];
                string firstName = DataGenerator.RandomFirstName();
                string lastName = DataGenerator.RandomLastName();
                string emailId = firstName.ToLower() + lastName.ToLower() + "@test.com";

                string enroledBy = "LOOPSScript@fly.virgin.com";

                DateTime endDate = DateTime.Now.AddDays(-6600);

                string dateOfBirth = DataGenerator.RandomDateBetweenTwoDates(endDate.AddDays(-7000), endDate).ToString("yyyy-MM-dd");

                (string customerId, string memberNumber) = await preRequisiteTestData.MemberEnrolment(channelID, title, firstName, lastName, emailId, postCode, country, dateOfBirth, enroledBy);

                Assert.True(!string.IsNullOrEmpty(customerId), "customerId returned from enrolment response is null/empty.");

                count--;

                #endregion
            }

        }

        [Trait("Category", Traits.TestDataCreationRegression)]
        [Fact, RunOrder(2)]
        public async Task TierUpgradeOfRedTierMembers()
        {
            #region Fetching Data From Reporting DB - Red Tier Members 

            string queryToFetchRedTierMembers = "Select Top 30 CustomerId,MemberNumber from SCV.FlyingClubMember where TierCode='Red' and TierCode not in ('Glass','VAC') and Status='Active' and CreatedBy!='WEB-MYA' order by CreatedOn desc";

            Dictionary<string, string> dbResponse = AzureReportingDBHelper.MultiRowKeyValuePairQueryResponse(queryToFetchRedTierMembers);

            #endregion

            #region Updating the Members to Silver and Gold by looping through list of fetched FC Members

            foreach (KeyValuePair<string, string> dict in dbResponse)
            {
                string custId = dict.Key;

                string updatedOn = DateTime.UtcNow.ToString(Application.ISODateTimeFormat);

                string tierStartDate = DateTime.UtcNow.AddDays(1).ToString(Application.ISODateTimeFormat);

                string tierEndDate = DateTime.UtcNow.AddMonths(13).AddDays(-Convert.ToInt32(DateTime.UtcNow.Day)).Date.ToString(Application.ISODateTimeFormat);

                tierName = tierName.Equals("Gold") ? "Silver" : "Gold";

                string queryForMembershipChangeDocument = $"SELECT * FROM CustomerDetailsCollection c where c.DocType = 'MembershipChange' and  c.PartitionKey='{custId}' and c.Active='Y'";

                MembershipChange membershipChangeDoc = cosmosFixture.QueryItem<MembershipChange>(queryForMembershipChangeDocument);

                string docId = membershipChangeDoc.Id;

                int revision = !string.IsNullOrEmpty(membershipChangeDoc.Revision.ToString()) ? Convert.ToInt32(Convert.ToDouble(membershipChangeDoc.Revision)) + 1 : 1;

                #region Creating a New Tier API Call

                HttpResponseMessage response = LoopsBackendAPIEndpoints.Customer.CreateMemberTier(custId, tokenFixture.GetToken("customer"), updatedOn, tierStartDate, tierEndDate, revision, tierName, "");

                dynamic result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

                string expectedResponse = "Tier Created Successfully";

                Assert.True(result.ToString().Contains(expectedResponse),
                    $"Expected the message to be {expectedResponse} but found {result.ToString()}");

                #endregion

                #region Fetching Document Id for the Newly Created Tier

                queryForMembershipChangeDocument = $"SELECT * FROM CustomerDetailsCollection c where c.DocType = 'MembershipChange' and  c.PartitionKey='{custId}' and c.Active='N' order by c.CreatedOn desc";

                membershipChangeDoc = cosmosFixture.QueryItem<MembershipChange>(queryForMembershipChangeDocument);

                int retry = 5;

                if ((retry > 0 && membershipChangeDoc == null) || membershipChangeDoc.Id == docId)
                {
                    await Task.Delay(500);

                    membershipChangeDoc = cosmosFixture.QueryItem<MembershipChange>(queryForMembershipChangeDocument);

                    retry--;
                }

                Assert.True(membershipChangeDoc is not null,
                    $"No Document Available for the searched query {queryForMembershipChangeDocument}.");

                docId = membershipChangeDoc.Id;

                #endregion

                #region Updating the Newly Created Tier API CALL

                response = LoopsBackendAPIEndpoints.Customer.UpdateMemberTier(custId, tokenFixture.GetToken("customer"), docId, updatedOn, tierStartDate, tierEndDate, revision);

                result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

                expectedResponse = "Tier Details have been saved successfully";

                Assert.True(result.message.ToString().Equals(expectedResponse),
                    $"Expected the message to be {expectedResponse} but found {result.ToString()}");

                #endregion
            }
            #endregion
        }

        [Trait("Category", Traits.TestDataCreationRegression)]
        [Fact, RunOrder(3)]
        public async Task AwardWingsAwardToTheMember()
        {

            #region Test Data

            string wingsAwardProductId = "1634561747";

            string wingsAwardProductName = "Top Member";

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode = @tierCode and c.Status = @status and c.MemberType = @memberType and c.HeadOfGroupMemberNumber = '' and c.CreatedBy not in ('VRED','AXIS','SNAPP','WEB-MYA') order by c.LastUpdatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode", "Gold").WithParameter("@memberType", "Individual").WithParameter("@status", "Active");

            FlyingClubMember fcmDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            Assert.True(fcmDoc is not null, "Didn't find any valid Gold Member");

            string customerId = fcmDoc.CustomerId;

            string memberNumber = fcmDoc.MemberNumber;

            string queryForCustomerDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", customerId);

            Core.DB_Model.Entities.Customer customerDoc = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(paramQuery);

            string updatedBy = DataGenerator.RandomFirstName();

            string operationIdentifier = "CONTACTUPDATE";

            DateTime currentDate = DateTime.UtcNow;

            string startDate = currentDate.ToString("MM/dd/yyyy");

            #endregion

            #region API Call 

            HttpResponseMessage response = LoopsFrontEndAPIEndpoints.AddOrUpdateAwardToTheMember(operationIdentifier, customerId, memberNumber,
                updatedBy, fcmDoc.Status, fcmDoc.NameOnCard, customerDoc.DateOfBirth.Value.ToString("MM/dd/yyyy"), customerDoc.FirstName, customerDoc.LastName,
                customerDoc.Title, customerDoc.FormalSalutation, wingsAwardProductName, startDate).EnsureIsSuccessStatusCode();

            dynamic result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(string.IsNullOrEmpty(result?.data?.errorCode.ToString()), $"The Award Creatuon was not successful. The response returned was {result}");

            #endregion

            #region Validation

            string queryForAwardDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey and c.ProductId = @productId order by c._ts desc";

            QueryDefinition awardDocQuery = new QueryDefinition(query: queryForAwardDoc).WithParameter("@docType", "Award").WithParameter("@partitionKey", customerId).WithParameter("@productId", wingsAwardProductId);

            Award awardDoc = cosmosFixture.CosmosQueryItem<Award>(awardDocQuery);

            int retry = 5;

            while (awardDoc is null && retry > 0)
            {
                await Task.Delay(1000);

                awardDoc = cosmosFixture.CosmosQueryItem<Award>(awardDocQuery);

                retry--;
            }

            Assert.True(awardDoc.ProductId.Equals(wingsAwardProductId),
                $"Expected the Voucher ProductId to be {wingsAwardProductId} but was {awardDoc.ProductId} .");

            Assert.True(awardDoc.StartDate.ToString("MM/dd/yyyy").Equals(startDate),
                $"Expected the Voucher StartDate to be {startDate} but was {awardDoc.StartDate.ToString("yyyy-MM-dd")} .");

            string expiryDate = awardDoc.ExpiryDate == DateTime.MinValue ? "" : awardDoc.ExpiryDate.ToString("yyyy-MM-dd");

            Assert.True(string.IsNullOrEmpty(expiryDate),
                $"Expected the Voucher EndDate to be Null or Empty but was {awardDoc.ExpiryDate} .");

            #endregion
        }
    }
}
