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
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.GraphQL;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.AuditManagement
{
    [Collection("VoucherAssociatedTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class AwardAuditTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {

        private readonly TokenFixture _tokenFixture;
        private readonly CosmosFixture _cosmosFixture;

        public string env = Config.Environment;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;
        private readonly string auditCosmosdb;

        private static string backendAPIEnv = ConfigReader.Get("BackendAPIEnv") ?? "";

        public AwardAuditTests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {
            _tokenFixture = tokenFixture;
            _cosmosFixture = cosmosFixture;

            _tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");

            if (!backendAPIEnv.Is("Project-DEV"))
                _tokenFixture.AddTokenForAwardApiFromKeyVault("award");

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;
            auditCosmosdb = env.Is("uat") ? Application.AuditCosmosDbUAT : Application.AuditCosmosDbSIT;

            string auditCosmosEndpoint = Application.AuditCosmosDbEndpoint;

            _cosmosFixture.RegisterClient(auditCosmosEndpoint, Vault.Default.GetSecretValue("TESTING-CmdbAudit-Automation-Key"));

            _cosmosFixture.RegisterType<AwardAudit>(auditCosmosEndpoint, auditCosmosdb, Application.AuditCosmosCollection);

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            _cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            _cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            _cosmosFixture.RegisterType<Award>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;

            _cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));

            _cosmosFixture.RegisterType<Core.DB_Model.Entities.Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(4)]
        public async Task TC01_ValidateAwardAuditHistory_SuccessScenario()
        {

            #region Test Data    

            string queryForAwardAuditDoc = "SELECT TOP 1 * FROM AuditCollection c where c.DocType = @docType order by c.CreatedOn desc ";

            QueryDefinition query = new QueryDefinition(query: queryForAwardAuditDoc).WithParameter("@docType", "AwardAudit");

            AwardAudit awardAuditDoc = _cosmosFixture.CosmosQueryItem<AwardAudit>(query);

            Assert.True(awardAuditDoc is not null, "No Award Audit Document Found.");

            string customerId = awardAuditDoc.CustomerId;

            string queryForCustAwardAuditDocs = "SELECT * FROM AuditCollection c where c.DocType = @docType and c.PartitionKey = @customerId order by c.CreatedOn asc ";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustAwardAuditDocs).WithParameter("@docType", "AwardAudit").WithParameter("@customerId", customerId);

            IEnumerable<AwardAudit> custAwardAuditDocs = _cosmosFixture.CosmosQueryItems<AwardAudit>(paramQuery);

            #endregion

            #region API Call

            HttpResponseMessage response = GraphQL.RetrieveAwardAudit(_tokenFixture.GetToken("graphqlv2"), customerId, "").EnsureIsSuccessStatusCode();

            dynamic result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(!result.ToString().Contains("Request failed"),
               $"Expected a valid response for retrieve award audit graphql operation but found {result.ToString()}");

            AwardAuditResponse[] awardAuditsResponse = JsonConvert.DeserializeObject<AwardAuditResponse[]>(result?.data.retrieveAwardAuditHistory.awardAuditHistory.ToString());

            AwardAuditResponse[] awardAudits = awardAuditsResponse.OrderBy(x => x.createdOn).ToArray();

            #endregion

            #region Validation

            int i = 0;

            foreach (AwardAudit awardAudit in custAwardAuditDocs)
            {

                Assert.True(awardAudit.AwardUpdatedBy == awardAudits[i].awardUpdatedBy,
                   $"Expected the Award LastUpdatedBy to be {awardAudit.AwardUpdatedBy} but found {awardAudits[i].awardUpdatedBy} ");

                Assert.True(awardAudit.CreatedBy == awardAudits[i].createdBy,
                   $"Expected the Award CreatedBy to be {awardAudit.CreatedBy} but found {awardAudits[i].createdBy} ");

                Assert.True(awardAudit.AwardUpdatedOn == awardAudits[i].awardUpdatedOn,
                   $"Expected the Award UpdatedOn to be {awardAudit.AwardUpdatedOn} but found {awardAudits[i].awardUpdatedOn} ");

                Assert.True(awardAudit.CreatedOn.ToString() == awardAudits[i].createdOn.ToString(),
                   $"Expected the Award CreatedOn to be {awardAudit.CreatedOn} but found {awardAudits[i].createdOn} ");

                Assert.True(awardAudit.AwardName == awardAudits[i].awardName,
                    $"Expected the Award Name Value to be {awardAudit.AwardName} but found {awardAudits[i].awardName} ");

                Assert.True(awardAudit.AwardNumber == awardAudits[i].awardNumber,
                    $"Expected the Award Number Value to be {awardAudit.AwardNumber} but found {awardAudits[i].awardNumber} ");

                Assert.True(awardAudit.StartDate == awardAudits[i].startDate,
                    $"Expected the Award StartDate Value to be {awardAudit.StartDate} but found {awardAudits[i].startDate} ");

                Assert.True(awardAudit.Status == awardAudits[i].status,
                    $"Expected the Award Status Value to be {awardAudit.Status} but found {awardAudits[i].status} ");

                Assert.True(awardAudit.AwardId == awardAudits[i].awardId,
                    $"Expected the Award Id Value to be {awardAudit.AwardId} but found {awardAudits[i].awardId} ");

                Assert.True(awardAudit.AwardProductId == awardAudits[i].awardProductId,
                    $"Expected the Award Product Id Value to be {awardAudit.AwardProductId} but found {awardAudits[i].awardProductId} ");

                Assert.True(awardAudit.GiftedFromCustomerId == awardAudits[i].giftedFromCustomerId,
                   $"Expected the Award Gifted From CustomerId Value to be {awardAudit.GiftedFromCustomerId} but found {awardAudits[i].giftedFromCustomerId} ");

                Assert.True(awardAudit.GiftedToCustomerId == awardAudits[i].giftedToCustomerId,
                  $"Expected the Award Gifted To CustomerId Value to be {awardAudit.GiftedToCustomerId} but found {awardAudits[i].giftedToCustomerId} ");

                Assert.True(awardAudit.GiftedFromMemberNumber == awardAudits[i].giftedFromMemberNumber,
                   $"Expected the Award Gifted From Member Number Value to be {awardAudit.GiftedFromMemberNumber} but found {awardAudits[i].giftedFromMemberNumber} ");

                Assert.True(awardAudit.GiftedToMemberNumber == awardAudits[i].giftedToMemberNumber,
                  $"Expected the Award Gifted To MemberNumber Value to be {awardAudit.GiftedToMemberNumber} but found {awardAudits[i].giftedToMemberNumber} ");

                i++;

            }

            #endregion

        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Theory, RunOrder(5)]
        [InlineData("LifeTime Gold", "1634561772")]
        [InlineData("Million Miler", "1634561592")]
        [InlineData("Top Member", "1634561747")]
        public async Task TC02_ValidateStatusFlagsAuditHistory_SuccessScenario(string awardName, string awardProductId)
        {

            #region Pre-requisite Test Data for Award Document 

            string queryForAwardDocument = "SELECT TOP 1 * FROM c where c.DocType = @docType and c.ProductId = @awardProductId order by c._ts desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForAwardDocument).WithParameter("@docType", "Award").WithParameter("@awardProductId", awardProductId);

            Award awardDocument = _cosmosFixture.CosmosQueryItem<Award>(parameterizedQuery);

            Assert.True(awardDocument is not null, $" No {awardName} Award Audit Document Found.");

            string customerId = awardDocument.CustomerId;

            #endregion

            #region fetching data from the Audit Cosmos DB

            string queryForAwardAuditDocs = "SELECT * FROM AuditCollection c where c.DocType = @docType and c.AwardProductId = @awardProductId and c.PartitionKey = @customerId order by c.CreatedOn desc ";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForAwardAuditDocs).WithParameter("@docType", "AwardAudit").WithParameter("@awardProductId", awardProductId).WithParameter("@customerId", customerId);

            IEnumerable<AwardAudit> awardAuditDocs = _cosmosFixture.CosmosQueryItems<AwardAudit>(paramQuery).OrderByDescending(x => x.AwardUpdatedOn);

            #endregion

            #region API call

            HttpResponseMessage response = GraphQL.RetrieveAwardAudit(_tokenFixture.GetToken("graphqlv2"), customerId, "").EnsureIsSuccessStatusCode();

            dynamic result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(!result.ToString().Contains("Request failed"),
               $"Expected a valid response for retrieve award audit graphql operation but found {result.ToString()}");

            AwardAuditResponse[] awardAudits = JsonConvert.DeserializeObject<AwardAuditResponse[]>(result?.data.retrieveAwardAuditHistory.awardAuditHistory.ToString());

            AwardAuditResponse[] awardAuditsSorted = awardAudits.Where(x => x.awardProductId.Equals(awardProductId)).OrderByDescending(x => x.awardUpdatedOn).ToArray();

            #endregion

            #region Valdations for Audit Document from COSMOS vs API response

            int i = 0;

            foreach (AwardAudit awardAuditDoc in awardAuditDocs)
            {

                Assert.True(awardAuditDoc.AwardUpdatedBy == awardAuditsSorted[i].awardUpdatedBy,
                   $"For the Customer {customerId} , Expected the {awardName} Award LastUpdatedBy to be {awardAuditDoc.AwardUpdatedBy} but found {awardAuditsSorted[i].awardUpdatedBy} ");

                Assert.True(awardAuditDoc.CreatedBy == awardAuditsSorted[i].createdBy,
                   $"For the Customer {customerId} , Expected the {awardName} Award CreatedBy to be {awardAuditDoc.CreatedBy} but found {awardAuditsSorted[i].createdBy} ");

                Assert.True(awardAuditDoc.AwardUpdatedOn == awardAuditsSorted[i].awardUpdatedOn,
                   $"For the Customer {customerId} , Expected the {awardName} Award UpdatedOn to be {awardAuditDoc.AwardUpdatedOn} but found {awardAuditsSorted[i].awardUpdatedOn} ");

                Assert.True(awardAuditDoc.CreatedOn == awardAuditsSorted[i].createdOn,
                   $"For the Customer {customerId} , Expected the {awardName} Award CreatedOn to be {awardAuditDoc.CreatedOn} but found {awardAuditsSorted[i].createdOn} ");

                Assert.True(awardAuditDoc.AwardName == awardAuditsSorted[i].awardName,
                   $"For the Customer {customerId} , Expected the {awardName} Award Name Value to be {awardAuditDoc.AwardName} but found {awardAuditsSorted[i].awardName} ");

                Assert.True(awardAuditDoc.AwardNumber == awardAuditsSorted[i].awardNumber,
                   $"For the Customer {customerId} , Expected the {awardName} Award Number Value to be {awardAuditDoc.AwardNumber} but found {awardAuditsSorted[i].awardNumber} ");

                Assert.True(awardAuditDoc.StartDate == awardAuditsSorted[i].startDate,
                   $"For the Customer {customerId} , Expected the {awardName} Award StartDate Value to be {awardAuditDoc.StartDate} but found {awardAuditsSorted[i].startDate} ");

                Assert.True(awardAuditDoc.Status == awardAuditsSorted[i].status,
                   $"For the Customer {customerId} , Expected the {awardName} Award Status Value to be {awardAuditDoc.Status} but found {awardAuditsSorted[i].status} ");

                Assert.True(awardAuditDoc.AwardId == awardAuditsSorted[i].awardId,
                   $"For the Customer {customerId} , Expected the {awardName} Award Id Value to be {awardAuditDoc.AwardId} but found {awardAuditsSorted[i].awardId} ");

                Assert.True(awardAuditDoc.AwardProductId == awardAuditsSorted[i].awardProductId,
                   $"For the Customer {customerId} , Expected the {awardName} Award Product Id Value to be {awardAuditDoc.AwardProductId} but found {awardAuditsSorted[i].awardProductId} ");

                Assert.True(awardAuditDoc.GiftedFromCustomerId == awardAuditsSorted[i].giftedFromCustomerId,
                   $"For the Customer {customerId} , Expected the {awardName} Award Gifted From CustomerId Value to be {awardAuditDoc.GiftedFromCustomerId} but found {awardAuditsSorted[i].giftedFromCustomerId} ");

                Assert.True(awardAuditDoc.GiftedToCustomerId == awardAuditsSorted[i].giftedToCustomerId,
                   $"For the Customer {customerId} , Expected the {awardName} Award Gifted To CustomerId Value to be {awardAuditDoc.GiftedToCustomerId} but found {awardAuditsSorted[i].giftedToCustomerId} ");

                Assert.True(awardAuditDoc.GiftedFromMemberNumber == awardAuditsSorted[i].giftedFromMemberNumber,
                   $"For the Customer {customerId} , Expected the {awardName} Award Gifted From MemberNumber Value to be {awardAuditDoc.GiftedFromMemberNumber} but found {awardAuditsSorted[i].giftedFromMemberNumber} ");

                Assert.True(awardAuditDoc.GiftedToMemberNumber == awardAuditsSorted[i].giftedToMemberNumber,
                   $"For the Customer {customerId} , Expected the {awardName} Award Gifted To MemberNumber Value to be {awardAuditDoc.GiftedToMemberNumber} but found {awardAuditsSorted[i].giftedToMemberNumber} ");

                i++;

            }

            #endregion

        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(6)]
        public async Task TC03_ValidateAwardAuditDocumentCreationWhenNewVoucherIsAwarded()
        {


            #region Test Data

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) order by c._ts desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC");

            FlyingClubMember fcmDoc = _cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            string customerId = fcmDoc.CustomerId;

            string queryForAwardDetails = "Select TOP 1 RowId,ProductName from dbo.Product where ProductType = 'Electronic Voucher' and active = 1 order by NEWID()";

            object[][] awardDetails = AzureReferenceDBHelper.MultiColumnRefDbQueryResponse(queryForAwardDetails);

            string awardName = awardDetails[0][1].ToString();

            string awardProductId = awardDetails[0][0].ToString();

            Assert.True(!string.IsNullOrEmpty(awardName), "Failed to Retrieve Award Name from Ref Db.");

            DateTime currentDate = DateTime.UtcNow.Date;

            DateTime awardStartDate = currentDate;

            DateTime awardEndDate = currentDate.AddDays(+365);

            string awardCreatedBy = $"TestScript{DataGenerator.RandomString(6).ToLower()}.com";

            #endregion

            #region API Call

            HttpResponseMessage response = LoopsBackendAPIEndpoints.Award.CreateAward(_tokenFixture.GetToken("award"), customerId, awardName, awardStartDate.ToString("yyyy-MM-dd"), awardEndDate.ToString("yyyy-MM-dd"), awardCreatedBy).EnsureIsSuccessStatusCode();

            dynamic result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Voucher rewarded successfully";

            string actualResponse = result.ToString();

            Assert.True(actualResponse.Has(expectedResponse),
                $"Expected {expectedResponse} but was {actualResponse} .");


            #endregion

            #region Audit Doc Validation

            string queryForAwardAuditDoc = "SELECT * FROM AuditCollection c where c.DocType = @docType and c.PartitionKey = @customerId and c.AwardProductId = @awardProductId and c.AwardUpdatedBy = @createdBy order by c.CreatedOn desc ";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForAwardAuditDoc).WithParameter("@docType", "AwardAudit").WithParameter("@customerId", customerId).WithParameter("@awardProductId", awardProductId).WithParameter("@createdBy", awardCreatedBy);

            AwardAudit awardAuditDoc = _cosmosFixture.CosmosQueryItem<AwardAudit>(paramQuery);

            int retry = 5;

            while (retry > 0 && awardAuditDoc is null)
            {
                await Task.Delay(1500);

                awardAuditDoc = _cosmosFixture.CosmosQueryItem<AwardAudit>(paramQuery);

                retry--;
            }

            Assert.True(awardAuditDoc is not null,
                $"For Customer Id {customerId} , Award Audit Doc not found for the Award {awardName} with Product Id {awardProductId} after retry of 7 seconds");

            string actualCreatedBy = awardAuditDoc.CreatedBy;

            string actualLastUpdatedBy = awardAuditDoc.AwardUpdatedBy;

            string actualCreatedOn = awardAuditDoc.CreatedOn.HasValue ? awardAuditDoc.CreatedOn.Value.ToString("dd-MMM-yyyy") : "";

            string actualLastUpdatedOn = awardAuditDoc.AwardUpdatedOn.HasValue ? awardAuditDoc.AwardUpdatedOn.Value.ToString("dd-MMM-yyyy") : "";

            string actualAwardProductId = awardAuditDoc.AwardProductId;

            string actualAwardStartDate = awardAuditDoc.StartDate.HasValue ? awardAuditDoc.StartDate.Value.ToString("dd-MMM-yyyy") : "";

            string actualAwardEndDate = awardAuditDoc.EndDate.HasValue ? awardAuditDoc.EndDate.Value.ToString("dd-MMM-yyyy") : "";

            string actualAwardName = awardAuditDoc.AwardName;

            string actualAwardStatus = awardAuditDoc.Status;

            string actualAwardUsedDate = awardAuditDoc.UsedDate.HasValue ? awardAuditDoc.UsedDate.Value.ToString("dd-MMM-yyyy") : "";

            string awardGiftedDate = awardAuditDoc.GiftedAwardDate.HasValue ? awardAuditDoc.GiftedAwardDate.Value.ToString("dd-MMM-yyyy") : "";

            string giftedFromCustomerId = awardAuditDoc.GiftedFromCustomerId;

            string giftedToCustomerId = awardAuditDoc.GiftedToCustomerId;

            string giftedFromMemberNumber = awardAuditDoc.GiftedFromMemberNumber;

            string giftedToMemberNumber = awardAuditDoc.GiftedToMemberNumber;


            Assert.True(actualCreatedBy == awardCreatedBy,
                $"For the CustomerId {customerId} , Expected Award Audit CreatedBy {awardCreatedBy} but found {actualCreatedBy}");

            Assert.True(actualLastUpdatedBy == awardCreatedBy,
                $"For the CustomerId {customerId} ,Expected Award Audit Doc LastUpdatedBy {awardCreatedBy} but found {actualLastUpdatedBy}");

            Assert.True(actualLastUpdatedBy == actualCreatedBy,
                $"For the CustomerId {customerId} ,Expected Award Audit Doc LastUpdatedBy and CreatedBy to be same but found awardCreatedBy as {actualCreatedBy} and the awardLastUpdatedBy as {actualLastUpdatedBy}");

            Assert.True(actualCreatedOn == currentDate.ToString("dd-MMM-yyyy"),
                $"For the CustomerId {customerId} ,Expected the Award Audit Doc CreatedOn Date to be {currentDate.ToString("dd-MMM-yyyy")} but found {actualCreatedOn} ");

            Assert.True(actualLastUpdatedOn == currentDate.ToString("dd-MMM-yyyy"),
               $"For the CustomerId {customerId} ,Expected the Award Audit Doc LastUpdatedOn Date to be {currentDate.ToString("dd-MMM-yyyy")} but found {actualLastUpdatedOn} ");

            Assert.True(actualCreatedOn == actualLastUpdatedOn,
                $"For the CustomerId {customerId} ,Expected Award Audit Doc  LastUpdatedOn and CreatedOn Date to be same but found awardCreatedOn Date as {actualCreatedOn} and the awardLastUpdatedOn as {actualLastUpdatedOn}");

            Assert.True(actualAwardName == awardName,
                $"For the CustomerId {customerId} ,Expected Award Name to be {awardName} but found {actualAwardName}");

            Assert.True(actualAwardProductId == awardProductId,
                $"For the CustomerId {customerId} ,Expected Award ProductId to be {awardProductId} but found {actualAwardProductId}");

            Assert.True(actualAwardStartDate == awardStartDate.ToString("dd-MMM-yyyy"),
                $"For the CustomerId {customerId} ,Expected Award StartDate to be {awardStartDate.ToString("dd-MMM-yyyy")} but found {actualAwardStartDate}");

            Assert.True(actualAwardEndDate == awardEndDate.ToString("dd-MMM-yyyy"),
                $"For the CustomerId {customerId} ,Expected Award EndDate to be {awardEndDate.ToString("dd-MMM-yyyy")} but found {actualAwardEndDate}");

            Assert.True(actualAwardStatus == "Available",
                $"For the CustomerId {customerId} ,Expected Award Status to be Available but found {actualAwardStatus}");

            Assert.True(actualAwardUsedDate == "",
                $"For the CustomerId {customerId} ,Expected Award UsedDate to be  but found {actualAwardUsedDate}");

            Assert.True(awardGiftedDate == "",
                $"For the CustomerId {customerId} ,Expected Award Gifted to be  but found {awardGiftedDate}");

            Assert.True(giftedFromCustomerId == "",
                $"For the CustomerId {customerId} ,Expected Award GiftedFromCustomerId to be  but found {giftedFromCustomerId}");

            Assert.True(giftedToCustomerId == "",
                $"For the CustomerId {customerId} ,Expected Award GiftedToCustomerId to be  but found {giftedToCustomerId}");

            Assert.True(giftedFromMemberNumber == "",
                $"For the CustomerId {customerId} ,Expected Award GiftedFromMember to be  but found {giftedFromMemberNumber}");

            Assert.True(giftedToMemberNumber == "",
                $"For the CustomerId {customerId} ,Expected Award GiftedToMember to be  but found {giftedToMemberNumber}");


            #endregion

        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(7)]
        public async Task TC04_ValidateAwardAuditDocumentCreationWhenAnExistingAwardIsUpdated()
        {

            #region Pre Requisite

            string queryForAwardDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.ProductId not in ('1634561592','1634561747','1634561772','')  and c.Status != @status and c.GiftedFromCustomerId = '' and IS_DEFINED(c.ExpiryDate) and c.ExpiryDate not in ( '',null) order by c.CreatedOn desc";

            //Excluding Status Flags for the Update as it is part of Customer Profile API
            QueryDefinition paramQuery = new QueryDefinition(query: queryForAwardDoc).WithParameter("@docType", "Award").WithParameter("@status", "Gifted");

            Award awardDoc = _cosmosFixture.CosmosQueryItem<Award>(paramQuery);

            Assert.True(awardDoc is not null, "Failed to retrieve an Award Doc for Update Scenario.");

            string customerId = awardDoc.CustomerId;

            string voucherId = awardDoc.AwardId;

            string voucherStatus = awardDoc.Status;

            string[] validStatuses = { "Available", "Expired", "Used", "Cancelled", "Partially Used" };

            string newStatus = validStatuses[0];

            Random random = new();

            while (newStatus == voucherStatus)
            {
                newStatus = validStatuses[random.Next(validStatuses.Length)];
            }


            DateTime currentDate = DateTime.UtcNow.Date;

            string startDate = awardDoc.StartDate.AddDays(-1).ToString("yyyy-MM-dd");

            string endDate = (awardDoc.ExpiryDate == DateTime.MinValue) ? awardDoc.StartDate.AddYears(+1).ToString("yyyy-MM-dd") : awardDoc.ExpiryDate.AddDays(+1).ToString("yyyy-MM-dd");

            string usedDate = newStatus.Equals("Used") ? currentDate.ToString("yyyy-MM-dd") : "";

            string awardUpdatedBy = $"TestScript{DataGenerator.RandomString(6).ToLower()}.co.ing";


            #endregion

            #region API Call

            HttpResponseMessage response = LoopsBackendAPIEndpoints.Award.UpdateAward(_tokenFixture.GetToken("award"), customerId, voucherId, newStatus, startDate, endDate, usedDate, awardUpdatedBy).EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Voucher updated successfully";

            string actualResponse = result.ToString();

            Assert.True(actualResponse.Has(expectedResponse),
                $"Expected {expectedResponse} but was {actualResponse} .");

            #endregion

            #region Audit Doc Validation

            string queryForAwardAuditDoc = "SELECT * FROM AuditCollection c where c.DocType = @docType and c.PartitionKey = @customerId and c.AwardId = @awardId and c.AwardUpdatedBy = @createdBy order by c.CreatedOn desc ";

            QueryDefinition query = new QueryDefinition(query: queryForAwardAuditDoc).WithParameter("@docType", "AwardAudit").WithParameter("@customerId", customerId).WithParameter("@awardId", voucherId).WithParameter("@createdBy", awardUpdatedBy);

            AwardAudit awardAuditDoc = _cosmosFixture.CosmosQueryItem<AwardAudit>(query);

            int retry = 10;

            while (retry > 0 && awardAuditDoc is null)
            {
                await Task.Delay(2000);

                awardAuditDoc = _cosmosFixture.CosmosQueryItem<AwardAudit>(query);

                retry--;
            }

            Assert.True(awardAuditDoc is not null, $"For Customer Id {customerId} , Award Audit Doc not found for the Award Id {voucherId} after retry of 10 seconds");

            string actualCreatedBy = awardAuditDoc.CreatedBy;

            string actualLastUpdatedBy = awardAuditDoc.AwardUpdatedBy;

            string actualCreatedOn = awardAuditDoc.CreatedOn.HasValue ? awardAuditDoc.CreatedOn.Value.ToString("yyyy-MM-dd") : "";

            string actualLastUpdatedOn = awardAuditDoc.AwardUpdatedOn.HasValue ? awardAuditDoc.AwardUpdatedOn.Value.ToString("yyyy-MM-dd") : "";

            string actualAwardStartDate = awardAuditDoc.StartDate.HasValue ? awardAuditDoc.StartDate.Value.ToString("yyyy-MM-dd") : "";

            string actualAwardEndDate = awardAuditDoc.EndDate.HasValue ? awardAuditDoc.EndDate.Value.ToString("yyyy-MM-dd") : "";

            string actualAwardId = awardAuditDoc.AwardId;

            string actualAwardStatus = awardAuditDoc.Status;

            string actualAwardUsedDate = awardAuditDoc.UsedDate.HasValue ? awardAuditDoc.UsedDate.Value.ToString("yyyy-MM-dd") : "";

            string awardGiftedDate = awardAuditDoc.GiftedAwardDate.HasValue ? awardAuditDoc.GiftedAwardDate.Value.ToString("yyyy-MM-dd") : "";

            string giftedFromCustomerId = awardAuditDoc.GiftedFromCustomerId;

            string giftedToCustomerId = awardAuditDoc.GiftedToCustomerId;

            string giftedFromMemberNumber = awardAuditDoc.GiftedFromMemberNumber;

            string giftedToMemberNumber = awardAuditDoc.GiftedToMemberNumber;


            Assert.True(actualCreatedBy == awardUpdatedBy,
                $"For the CustomerId {customerId} , Expected Award Audit CreatedBy {awardUpdatedBy} but found {actualCreatedBy}");

            Assert.True(actualLastUpdatedBy == awardUpdatedBy,
                $"For the CustomerId {customerId} ,Expected Award Audit Doc LastUpdatedBy {awardUpdatedBy} but found {actualLastUpdatedBy}");

            Assert.True(actualLastUpdatedBy == actualCreatedBy,
                $"For the CustomerId {customerId} ,Expected Award Audit Doc LastUpdatedBy and CreatedBy to be same but found awardCreatedBy as {actualCreatedBy} and the awardLastUpdatedBy as {actualLastUpdatedBy}");

            Assert.True(actualCreatedOn == currentDate.ToString("yyyy-MM-dd"),
                $"For the CustomerId {customerId} ,Expected the Award Audit Doc CreatedOn Date to be {currentDate.ToString("yyyy-MM-dd")} but found {actualCreatedOn} ");

            Assert.True(actualLastUpdatedOn == currentDate.ToString("yyyy-MM-dd"),
               $"For the CustomerId {customerId} ,Expected the Award Audit Doc LastUpdatedOn Date to be {currentDate.ToString("yyyy-MM-dd")} but found {actualLastUpdatedOn} ");

            Assert.True(actualCreatedOn == actualLastUpdatedOn,
                $"For the CustomerId {customerId} ,Expected Award Audit Doc  LastUpdatedOn and CreatedOn Date to be same but found awardCreatedOn Date as {actualCreatedOn} and the awardLastUpdatedOn as {actualLastUpdatedOn}");

            Assert.True(actualAwardStartDate == startDate,
                $"For the CustomerId {customerId} ,Expected Award StartDate to be {startDate} but found {actualAwardStartDate}");

            Assert.True(actualAwardId == voucherId,
                $"For the CustomerId {customerId} ,Expected Award Id to be {voucherId} but found {actualAwardId}");

            Assert.True(actualAwardEndDate == endDate,
                $"For the CustomerId {customerId} ,Expected Award EndDate to be {endDate} but found {actualAwardEndDate}");

            Assert.True(actualAwardStatus == newStatus,
                $"For the CustomerId {customerId} ,Expected Award Status to be {newStatus} but found {actualAwardStatus}");

            Assert.True(actualAwardUsedDate == usedDate,
                $"For the CustomerId {customerId} ,Expected Award UsedDate to be {usedDate}  but found {actualAwardUsedDate}");

            Assert.True(giftedFromCustomerId == "",
                $"For the CustomerId {customerId} ,Expected Award GiftedFromCustomerId to be  but found {giftedFromCustomerId}");

            Assert.True(giftedToCustomerId == "",
                $"For the CustomerId {customerId} ,Expected Award GiftedToCustomerId to be  but found {giftedToCustomerId}");

            Assert.True(giftedFromMemberNumber == "",
                $"For the CustomerId {customerId} ,Expected Award GiftedFromMember to be  but found {giftedFromMemberNumber}");

            Assert.True(giftedToMemberNumber == "",
                $"For the CustomerId {customerId} ,Expected Award GiftedToMember to be  but found {giftedToMemberNumber}");


            #endregion

        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(8)]
        public async Task TC05_ValidateAwardAuditDocumentCreationWhenAVoucherIsGiftedFromAccountToAnother()
        {

            #region Pre Requisite

            string queryForAwardDoc = "SELECT TOP 10 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.Status = @status and c.ProductId not in ('1634561592','1634561747','1634561772','') and c.AwardNumber not like '%-2'  order by c._ts desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForAwardDoc).WithParameter("@docType", "Award").WithParameter("@status", "Available");

            IEnumerable<Award> awardDocs = _cosmosFixture.CosmosQueryItems<Award>(paramQuery);

            Award awardDoc = awardDocs.FirstOrDefault(x =>
            {
                string queryForFCMDoc = "SELECT * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey and c.Status = @status";

                QueryDefinition fcmDocQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@partitionKey", x.CustomerId).WithParameter("@status", "Active");

                FlyingClubMember fcmDoc = _cosmosFixture.CosmosQueryItem<FlyingClubMember>(fcmDocQuery);

                return fcmDoc is not null;
            });

            Assert.True(awardDoc is not null, "Unable to find a valid sender account with Active status for Award Gifting");

            string senderCustomerId = awardDoc.CustomerId;

            string queryForCustDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey";

            QueryDefinition custDocQuery = new QueryDefinition(query: queryForCustDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", senderCustomerId);

            string senderMemberNumber = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(custDocQuery).LoyaltyMemberNumber;

            string voucherId = awardDoc.AwardId;

            string senderAwardNumber = awardDoc.AwardNumber;

            string startDate = awardDoc.StartDate.ToString("yyyy-MM-dd");

            string queryForCustomerDoc = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey <> @partitionKey and c.Status = @status order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForCustomerDoc).WithParameter("@docType", "Customer").WithParameter("@partitionKey", senderCustomerId).WithParameter("@status", "Active");

            Core.DB_Model.Entities.Customer customerDoc = _cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(parameterizedQuery);

            string recipientCustomerId = customerDoc.CustomerId;

            string recipientMemberNumber = customerDoc.LoyaltyMemberNumber;

            string giftedBy = $"TestScript{DataGenerator.RandomString(8).ToLower()}.testmail.on";

            DateTime currentDate = DateTime.UtcNow.Date;

            #endregion

            #region API Call

            HttpResponseMessage response = LoopsBackendAPIEndpoints.Award.GiftAward(_tokenFixture.GetToken("award"), senderCustomerId, voucherId, recipientMemberNumber, recipientCustomerId, giftedBy).EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Voucher gifted successfully";

            string actualResponse = result.ToString();

            Assert.True(actualResponse.Has(expectedResponse),
                $"Expected {expectedResponse} but was {actualResponse} .");

            string expectedAwardNumberAtReceiverEnd = $"{senderAwardNumber}-2";

            #endregion

            #region Audit Doc Validation at Sender's End

            string queryForAwardAuditDoc = "SELECT * FROM AuditCollection c where c.DocType = @docType and c.PartitionKey = @customerId and c.AwardId = @awardId and c.AwardUpdatedBy = @createdBy order by c.CreatedOn desc ";

            QueryDefinition query = new QueryDefinition(query: queryForAwardAuditDoc).WithParameter("@docType", "AwardAudit").WithParameter("@customerId", senderCustomerId).WithParameter("@awardId", voucherId).WithParameter("@createdBy", giftedBy);

            AwardAudit awardAuditDoc = _cosmosFixture.CosmosQueryItem<AwardAudit>(query);

            int retry = 5;

            while (retry > 0 && awardAuditDoc is null)
            {
                await Task.Delay(1500);

                awardAuditDoc = _cosmosFixture.CosmosQueryItem<AwardAudit>(paramQuery);

                retry--;
            }

            Assert.True(awardAuditDoc is not null,
                $"For Customer Id {senderCustomerId} , Award Audit Doc not found for the Award Id {voucherId} after retry of 7 seconds");

            string actualCreatedBy = awardAuditDoc.CreatedBy;

            string actualLastUpdatedBy = awardAuditDoc.AwardUpdatedBy;

            string actualCreatedOn = awardAuditDoc.CreatedOn.HasValue ? awardAuditDoc.CreatedOn.Value.ToString("yyyy-MM-dd") : "";

            string actualLastUpdatedOn = awardAuditDoc.AwardUpdatedOn.HasValue ? awardAuditDoc.AwardUpdatedOn.Value.ToString("yyyy-MM-dd") : "";

            string actualAwardStartDate = awardAuditDoc.StartDate.HasValue ? awardAuditDoc.StartDate.Value.ToString("yyyy-MM-dd") : "";

            string actualAwardId = awardAuditDoc.AwardId;

            string actualAwardStatus = awardAuditDoc.Status;

            string awardGiftedDate = awardAuditDoc.GiftedAwardDate.HasValue ? awardAuditDoc.GiftedAwardDate.Value.ToString("yyyy-MM-dd") : "";

            string giftedFromCustomerId = awardAuditDoc.GiftedFromCustomerId;

            string giftedToCustomerId = awardAuditDoc.GiftedToCustomerId;

            string giftedFromMemberNumber = awardAuditDoc.GiftedFromMemberNumber;

            string giftedToMemberNumber = awardAuditDoc.GiftedToMemberNumber;


            Assert.True(actualCreatedBy == giftedBy,
                $"For the CustomerId {senderCustomerId} , Expected Award Audit CreatedBy {giftedBy} but found {actualCreatedBy}");

            Assert.True(actualLastUpdatedBy == giftedBy,
                $"For the senderCustomerId {senderCustomerId} ,Expected Award Audit Doc LastUpdatedBy {giftedBy} but found {actualLastUpdatedBy}");

            Assert.True(actualLastUpdatedBy == actualCreatedBy,
                $"For the senderCustomerId {senderCustomerId} ,Expected Award Audit Doc LastUpdatedBy and CreatedBy to be same but found awardCreatedBy as {actualCreatedBy} and the awardLastUpdatedBy as {actualLastUpdatedBy}");

            Assert.True(actualCreatedOn == currentDate.ToString("yyyy-MM-dd"),
                $"For the senderCustomerId {senderCustomerId} ,Expected the Award Audit Doc CreatedOn Date to be {currentDate.ToString("yyyy-MM-dd")} but found {actualCreatedOn} ");

            Assert.True(actualLastUpdatedOn == currentDate.ToString("yyyy-MM-dd"),
               $"For the senderCustomerId {senderCustomerId} ,Expected the Award Audit Doc LastUpdatedOn Date to be {currentDate.ToString("yyyy-MM-dd")} but found {actualLastUpdatedOn} ");

            Assert.True(actualCreatedOn == actualLastUpdatedOn,
                $"For the senderCustomerId {senderCustomerId} ,Expected Award Audit Doc  LastUpdatedOn and CreatedOn Date to be same but found awardCreatedOn Date as {actualCreatedOn} and the awardLastUpdatedOn as {actualLastUpdatedOn}");

            Assert.True(actualAwardStartDate == startDate,
                $"For the senderCustomerId {senderCustomerId} ,Expected Award StartDate to be {startDate} but found {actualAwardStartDate}");

            Assert.True(actualAwardStatus == "Gifted",
                $"For the senderCustomerId {senderCustomerId} ,Expected Award Status to be gifted but found {actualAwardStatus}");

            Assert.True(actualAwardId == voucherId,
                $"For the senderCustomerId {senderCustomerId} ,Expected Award Id to be {voucherId} but found {actualAwardId}");

            Assert.True(awardGiftedDate == currentDate.ToString("yyyy-MM-dd"),
                $"For the senderCustomerId {senderCustomerId} ,Expected Award Gifted to be {currentDate.ToString("yyyy-MM-dd")} but found {awardGiftedDate}");

            Assert.True(giftedFromCustomerId == "",
                $"For the senderCustomerId {senderCustomerId} ,Expected Award GiftedFromCustomerId to be null/empty  but found {giftedFromCustomerId}");

            Assert.True(giftedToCustomerId == recipientCustomerId,
                $"For the senderCustomerId {senderCustomerId} ,Expected Award GiftedToCustomerId  to be {recipientCustomerId}  but found {giftedToCustomerId}");

            Assert.True(giftedFromMemberNumber == "",
                $"For the senderCustomerId {senderCustomerId} ,Expected Award GiftedFromMember to be null/empty but found {giftedFromMemberNumber}");

            Assert.True(giftedToMemberNumber == recipientMemberNumber,
                $"For the senderCustomerId {senderCustomerId} ,Expected Award GiftedToMember to be {recipientMemberNumber} but found {giftedToMemberNumber}");


            #endregion

            #region Audit Doc Validation at Receiver's End

            queryForAwardAuditDoc = "SELECT * FROM AuditCollection c where c.DocType = @docType and c.PartitionKey = @customerId and c.AwardNumber = @awardNumber and c.AwardUpdatedBy = @updatedBy order by c.CreatedOn desc ";

            query = new QueryDefinition(query: queryForAwardAuditDoc).WithParameter("@docType", "AwardAudit").WithParameter("@customerId", recipientCustomerId).WithParameter("@awardNumber", expectedAwardNumberAtReceiverEnd).WithParameter("@updatedBy", giftedBy);

            AwardAudit recipientAwardAuditDoc = _cosmosFixture.CosmosQueryItem<AwardAudit>(query);

            retry = 5;

            while (retry > 0 && recipientAwardAuditDoc is null)
            {
                await Task.Delay(1500);

                recipientAwardAuditDoc = _cosmosFixture.CosmosQueryItem<AwardAudit>(query);

                retry--;
            }

            Assert.True(recipientAwardAuditDoc is not null,
                $"For Recipient Customer Id {recipientCustomerId} , Award Audit Doc not found for the Award Id {voucherId} after retry of 7.5 seconds");

            actualCreatedBy = recipientAwardAuditDoc.CreatedBy;

            actualLastUpdatedBy = awardAuditDoc.AwardUpdatedBy;

            actualCreatedOn = recipientAwardAuditDoc.CreatedOn.HasValue ? recipientAwardAuditDoc.CreatedOn.Value.ToString("yyyy-MM-dd") : "";

            actualLastUpdatedOn = recipientAwardAuditDoc.AwardUpdatedOn.HasValue ? recipientAwardAuditDoc.AwardUpdatedOn.Value.ToString("yyyy-MM-dd") : "";

            actualAwardStartDate = recipientAwardAuditDoc.StartDate.HasValue ? recipientAwardAuditDoc.StartDate.Value.ToString("yyyy-MM-dd") : "";

            string actualAwardNumber = recipientAwardAuditDoc.AwardNumber;

            actualAwardStatus = recipientAwardAuditDoc.Status;

            awardGiftedDate = recipientAwardAuditDoc.GiftedAwardDate.HasValue ? recipientAwardAuditDoc.GiftedAwardDate.Value.ToString("yyyy-MM-dd") : "";

            giftedFromCustomerId = recipientAwardAuditDoc.GiftedFromCustomerId;

            giftedToCustomerId = recipientAwardAuditDoc.GiftedToCustomerId;

            giftedFromMemberNumber = recipientAwardAuditDoc.GiftedFromMemberNumber;

            giftedToMemberNumber = recipientAwardAuditDoc.GiftedToMemberNumber;


            Assert.True(actualCreatedBy == giftedBy,
                $"For the CustomerId {recipientCustomerId} , Expected Award Audit CreatedBy {giftedBy} but found {actualCreatedBy}");

            Assert.True(actualLastUpdatedBy == giftedBy,
                $"For the recipientCustomerId {recipientCustomerId} ,Expected Award Audit Doc LastUpdatedBy {giftedBy} but found {actualLastUpdatedBy}");

            Assert.True(actualLastUpdatedBy == actualCreatedBy,
                $"For the recipientCustomerId {recipientCustomerId} ,Expected Award Audit Doc LastUpdatedBy and CreatedBy to be same but found awardCreatedBy as {actualCreatedBy} and the awardLastUpdatedBy as {actualLastUpdatedBy}");

            Assert.True(actualCreatedOn == currentDate.ToString("yyyy-MM-dd"),
                $"For the recipientCustomerId {recipientCustomerId} ,Expected the Award Audit Doc CreatedOn Date to be {currentDate.ToString("yyyy-MM-dd")} but found {actualCreatedOn} ");

            Assert.True(actualLastUpdatedOn == currentDate.ToString("yyyy-MM-dd"),
               $"For the recipientCustomerId {recipientCustomerId} ,Expected the Award Audit Doc LastUpdatedOn Date to be {currentDate.ToString("yyyy-MM-dd")} but found {actualLastUpdatedOn} ");

            Assert.True(actualCreatedOn == actualLastUpdatedOn,
                $"For the recipientCustomerId {recipientCustomerId} ,Expected Award Audit Doc LastUpdatedOn and CreatedOn Date to be same but found awardCreatedOn Date as {actualCreatedOn} and the awardLastUpdatedOn as {actualLastUpdatedOn}");

            Assert.True(actualAwardStartDate == startDate,
                $"For the recipientCustomerId {recipientCustomerId} ,Expected Award StartDate to be {startDate} but found {actualAwardStartDate}");

            Assert.True(actualAwardStatus == "Available",
                $"For the recipientCustomerId {recipientCustomerId} ,Expected Award Status to be Available but found {actualAwardStatus}");

            Assert.True(actualAwardNumber == expectedAwardNumberAtReceiverEnd,
                $"For the recipientCustomerId {recipientCustomerId} ,Expected Award Id to be {expectedAwardNumberAtReceiverEnd} but found {actualAwardId}");

            Assert.True(awardGiftedDate == currentDate.ToString("yyyy-MM-dd"),
                $"For the recipientCustomerId {recipientCustomerId} ,Expected Award Gifted to be {currentDate.ToString("yyyy-MM-dd")} but found {awardGiftedDate}");

            Assert.True(giftedFromCustomerId == senderCustomerId,
                $"For the recipientCustomerId {recipientCustomerId} ,Expected Award GiftedFromCustomerId to be {senderCustomerId} but found {giftedFromCustomerId}");

            Assert.True(giftedToCustomerId == "",
                $"For the recipientCustomerId {recipientCustomerId} ,Expected Award GiftedToCustomerId to be null/empty but found {giftedToCustomerId}");

            Assert.True(giftedFromMemberNumber == senderMemberNumber,
                $"For the recipientCustomerId {recipientCustomerId} ,Expected Award GiftedFromMember to be {senderMemberNumber} but found {giftedFromMemberNumber}");

            Assert.True(giftedToMemberNumber == "",
                $"For the recipientCustomerId {recipientCustomerId} ,Expected Award GiftedToMember to be null/empty but found {giftedToMemberNumber}");


            #endregion
        }

    }
}
