using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.GraphQL;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.AuditManagement
{

    [Collection("RelationshipTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class SupplementarySilverAuditTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {

        private readonly TokenFixture _tokenFixture;

        private readonly CosmosFixture _cosmosFixture;

        public string env = Config.Environment;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;
        private readonly string auditCosmosdb;


        public SupplementarySilverAuditTests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {
            _tokenFixture = tokenFixture;

            _cosmosFixture = cosmosFixture;

            _tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;
            auditCosmosdb = env.Is("uat") ? Application.AuditCosmosDbUAT : Application.AuditCosmosDbSIT;

            string auditCosmosEndpoint = Application.AuditCosmosDbEndpoint;

            _cosmosFixture.RegisterClient(auditCosmosEndpoint, Vault.Default.GetSecretValue("TESTING-CmdbAudit-Automation-Key"));

            _cosmosFixture.RegisterType<OtherAudit>(auditCosmosEndpoint, auditCosmosdb, Application.AuditCosmosCollection);

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            _cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            _cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;

            _cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));

            _cosmosFixture.RegisterType<Core.DB_Model.Entities.Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(40)]
        public async Task TC01_ValidateSupplementarySilverAuditHistory_SuccessScenario()
        {

            #region Test Data    

            string queryForSuppSilverAuditDoc = "SELECT TOP 1 * FROM AuditCollection c where c.DocType = @docType and c.AuditCategory = @auditCategory order by c.CreatedOn desc ";

            QueryDefinition query = new QueryDefinition(query: queryForSuppSilverAuditDoc).WithParameter("@docType", "OtherAudit").WithParameter("@auditCategory", "Supplementary Silver");

            OtherAudit suppSilverAuditDoc = _cosmosFixture.CosmosQueryItem<OtherAudit>(query);

            Assert.True(suppSilverAuditDoc is not null, "No Supp Silver Audit Document Found.");

            string customerId = suppSilverAuditDoc.CustomerId;

            string queryForCustSuppSilverAuditDocs = "SELECT * FROM AuditCollection c where c.DocType = @docType and c.PartitionKey = @customerId and c.AuditCategory = @auditCategory ";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustSuppSilverAuditDocs).WithParameter("@docType", "OtherAudit").WithParameter("@customerId", customerId).WithParameter("@auditCategory", "Supplementary Silver");

            IEnumerable<OtherAudit> custSuppSilverAuditDocs = _cosmosFixture.CosmosQueryItems<OtherAudit>(paramQuery).OrderByDescending(x => x.CreatedOn);

            Assert.True(custSuppSilverAuditDocs.Any(),
                $"The Supp Silver Audit Docs count does not found for the CustomerId {customerId}.");


            #endregion

            #region API Call

            HttpResponseMessage response = GraphQL.RetrieveSupplementarySilverAudit(_tokenFixture.GetToken("graphqlv2"), customerId, "").EnsureIsSuccessStatusCode();

            dynamic result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(!result.ToString().Contains("Request failed"),
               $"Expected a valid response for retrieve Supp Silver audit graphql operation but found {result.ToString()}");

            SupplementarySilverAuditResponse[] suppSilverAuditResponse = JsonConvert.DeserializeObject<SupplementarySilverAuditResponse[]>(result?.data?.retrieveSupplementarySilverAuditHistory?.supplementarySilverAuditHistory?.ToString());

            suppSilverAuditResponse = suppSilverAuditResponse.OrderByDescending(x => x.createdOn).ToArray();

            Assert.True(custSuppSilverAuditDocs.Count() == suppSilverAuditResponse.Length,
                $"The Supp Silver Audit Docs count does not match for customer Id {customerId} \n . Cosmos doc count is {custSuppSilverAuditDocs.Count()} and response doc count is {suppSilverAuditResponse.Length}");

            #endregion

            #region Validation

            int i = 0;

            foreach (OtherAudit suppSilverAudit in custSuppSilverAuditDocs)
            {

                Assert.True(suppSilverAudit.CreatedBy == suppSilverAuditResponse[i].createdBy,
                   $"For the Doc Id {suppSilverAudit.Id}, Expected the Supp Silver Doc CreatedBy to be {suppSilverAudit.CreatedBy} but found {suppSilverAuditResponse[i].createdBy} ");

                Assert.True(suppSilverAudit.DataUpdatedBy == suppSilverAuditResponse[i].dataUpdatedBy,
                    $"For the Doc Id {suppSilverAudit.Id}, Expected the Supp Silver Doc UpdatedBy to be {suppSilverAudit.DataUpdatedBy} but found {suppSilverAuditResponse[i].dataUpdatedBy} ");

                Assert.True(suppSilverAudit.CreatedOn.Value.Date.ToString("dd-MMM-yyyy") == suppSilverAuditResponse[i].createdOn.Value.Date.ToString("dd-MMM-yyyy"),
                    $"For the Doc Id {suppSilverAudit.Id}, Expected the Supp Silver Doc CreatedOnDate to be {suppSilverAudit.CreatedOn.Value.Date.ToString("dd-MMM-yyyy")} but found {suppSilverAuditResponse[i].createdOn.Value.Date.ToString("dd-MMM-yyyy")} ");

                Assert.True(suppSilverAudit.DataUpdatedOn.Value.Date.ToString("dd-MMM-yyyy") == suppSilverAuditResponse[i].dataUpdatedOn.Value.Date.ToString("dd-MMM-yyyy"),
                    $"For the Doc Id {suppSilverAudit.Id}, Expected the Supp Silver Doc DataUpdatedOnDate to be {suppSilverAudit.DataUpdatedOn.Value.Date.ToString("dd-MMM-yyyy")} but found {suppSilverAuditResponse[i].dataUpdatedOn.Value.Date.ToString("dd-MMM-yyyy")} ");

                Assert.True(suppSilverAudit.RelationshipType == suppSilverAuditResponse[i].relationshipType,
                    $"For the Doc Id {suppSilverAudit.Id}, Expected the Supp Silver Doc RelationshipType to be {suppSilverAudit.RelationshipType} but found {suppSilverAuditResponse[i].relationshipType} ");

                Assert.True(suppSilverAudit.AuditData == suppSilverAuditResponse[i].auditData,
                    $"For the Doc Id {suppSilverAudit.Id}, Expected the Supp Silver Doc AuditData/MemberNumber to be {suppSilverAudit.AuditData} but found {suppSilverAuditResponse[i].auditData} ");

                i++;

            }

            #endregion

        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(41)]
        public async Task TC02_ValidateSupplementarySilverAuditHistoryAfterSSALinking_SuccessScenario()
        {

            string parentCustomerId = string.Empty;
            string parentMemberNumber = string.Empty;
            string childCustomerId = string.Empty;
            string childMemberNumber = string.Empty;


            #region Pre-Requisite for Fetching the Gold FC Number (Parent Account) for Supplementary Silver Linking 

            string queryForParentFCMDocuments = "SELECT TOP 10 * FROM CustomerDetailsCollection c where c.DocType = @docType and  c.TierCode = @tierCode and c.Status = @status and c.SSAMemberNumber = @ssaMemberNumber order by c.LastUpdatedOn desc";

            QueryDefinition query = new QueryDefinition(query: queryForParentFCMDocuments).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode", "Gold").WithParameter("@status", "Active").WithParameter("@ssaMemberNumber", "");

            IEnumerable<FlyingClubMember> flyingClubMemberDocs = _cosmosFixture.CosmosQueryItems<FlyingClubMember>(query);

            int retry = 5;

            while (retry > 0 && !flyingClubMemberDocs.Any())
            {
                await Task.Delay(1000);

                flyingClubMemberDocs = _cosmosFixture.CosmosQueryItems<FlyingClubMember>(query);

                retry--;
            }

            Assert.True(flyingClubMemberDocs.Any(), "No Valid Gold Member Found for SSA Parent.");

            FlyingClubMember flyingClubMemberDoc = null;

            foreach (FlyingClubMember fcmDoc in flyingClubMemberDocs)
            {

                parentCustomerId = fcmDoc.CustomerId;

                parentMemberNumber = fcmDoc.MemberNumber;

                string queryForFCMDoc = "SELECT * FROM CustomerDetailsCollection c where c.DocType = @docType and c.SSAMemberNumber = @ssaMemberNumber ";

                QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@ssaMemberNumber", parentMemberNumber);

                flyingClubMemberDoc = _cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

                if (flyingClubMemberDoc is null)
                {
                    break; // Validating if the Fetched Gold Member is a parent to some other Member
                }

            }

            Assert.True(flyingClubMemberDoc is null,
               "The Retrieved Parent FC Accounts are SSA Members to some other accounts.Test Data Criteria Didn't met");

            #endregion

            #region Pre-Requisite for Fetching the Red FC Numbers (Child Account) for Supplementary Silver Linking

            string queryForChildFcmDocs = "SELECT TOP 10 * FROM CustomerDetailsCollection c where c.DocType = @docType and  c.TierCode= @tierCode and c.Status= @status and c.SSAMemberNumber = @ssaMemberNumber order by c.CreatedOn desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForChildFcmDocs).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode", "Red").WithParameter("@status", "Active").WithParameter("@ssaMemberNumber", "");

            flyingClubMemberDocs = _cosmosFixture.CosmosQueryItems<FlyingClubMember>(paramQuery);

            retry = 5;

            while (retry > 0 && !flyingClubMemberDocs.Any())
            {
                await Task.Delay(1000);

                flyingClubMemberDocs = _cosmosFixture.CosmosQueryItems<FlyingClubMember>(paramQuery);

                retry--;
            }

            Assert.True(flyingClubMemberDocs.Any(), "No Valid Red/Child Member Found for SSA Child.");

            FlyingClubMember flyingClubMemberDoc1 = null;

            foreach (FlyingClubMember fcmDoc in flyingClubMemberDocs)
            {

                childCustomerId = fcmDoc.CustomerId;

                childMemberNumber = fcmDoc.MemberNumber;

                string queryForFCMDoc = "SELECT * FROM CustomerDetailsCollection c where c.DocType = @docType and c.SSAMemberNumber = @ssaMemberNumber ";

                QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@ssaMemberNumber", childMemberNumber);

                flyingClubMemberDoc1 = _cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

                if (flyingClubMemberDoc1 is null)
                {
                    break; // Validating if the Fetched Red Member is not a SSAMember to an FC Account
                }
            }

            Assert.True(flyingClubMemberDoc1 is null, "The Retrieved Child FC Accounts are SSA Members to some other accounts.Test Data Criteria Didn't met");

            string linkedBy = DataGenerator.RandomFirstName();

            #endregion

            #region Supplementary Account Linking using GraphQL endpoint

            HttpResponseMessage response = GraphQL.SupplementarySilverLink(_tokenFixture.GetToken("graphqlv2"), childCustomerId, childMemberNumber
                                    , parentCustomerId, parentMemberNumber, linkedBy).EnsureIsSuccessStatusCode();

            dynamic linkingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Request successfully initiated";

            Assert.True(linkingResult?.data?.loyaltySupplementarySilverLinking?.ToString().Contains(expectedResponse),
                $"Expected {expectedResponse} in the response text but found {linkingResult?.ToString()} ");

            #endregion

            #region Validation of SSA Audit for SSA Parent

            string queryForSuppSilverAuditDoc = "SELECT TOP 1 * FROM AuditCollection c where c.DocType = @docType and c.AuditCategory = @auditCategory and c.PartitionKey  = @partitionKey and c.CreatedBy = @createdBy order by c.CreatedOn desc ";

            QueryDefinition auditDocQuery = new QueryDefinition(query: queryForSuppSilverAuditDoc).WithParameter("@docType", "OtherAudit").WithParameter("@auditCategory", "Supplementary Silver").WithParameter("@partitionKey", parentCustomerId).WithParameter("@createdBy", linkedBy);

            OtherAudit suppSilverAuditDoc = _cosmosFixture.CosmosQueryItem<OtherAudit>(auditDocQuery);

            retry = 5;

            while (retry > 0 && suppSilverAuditDoc is null)
            {
                await Task.Delay(1000);

                suppSilverAuditDoc = _cosmosFixture.CosmosQueryItem<OtherAudit>(auditDocQuery);

                retry--;
            }

            Assert.True(suppSilverAuditDoc is not null, $"No Supp Silver Audit Document Found for the SSA Parent Customer {parentCustomerId} after Successful Linking");

            Assert.True(suppSilverAuditDoc.RelationshipType == "Supp Silver",
                $"In the Parent SSA Audit Doc {parentCustomerId}, Expected the RelationType with the child to be Supp Silver but found {suppSilverAuditDoc.RelationshipType}");

            Assert.True(suppSilverAuditDoc.AuditSubCategory == "Linking",
                $"In the Parent SSA Audit Doc {parentCustomerId}, Expected the Audit Sub Category to be Linking but found {suppSilverAuditDoc.AuditSubCategory}");

            Assert.True(suppSilverAuditDoc.AuditData == childMemberNumber,
                $"In the Parent SSA Audit Doc {parentCustomerId}, Expected the Audit Data / MemberNumber to be {childMemberNumber} but found {suppSilverAuditDoc.AuditData}");

            Assert.True(suppSilverAuditDoc.DataUpdatedBy == linkedBy,
                $"In the Parent SSA Audit Doc {parentCustomerId}, Expected the AuditDoc UpdatedBy to be {linkedBy} but found {suppSilverAuditDoc.DataUpdatedBy}");

            string createdOnDate = DateTime.UtcNow.ToString("dd-MMM-yyyy");

            Assert.True(suppSilverAuditDoc.CreatedOn.Value.Date.ToString("dd-MMM-yyyy") == createdOnDate,
                $"In the Parent SSA Audit Doc {parentCustomerId}, Expected the Audit Doc CreatedOn  to be {createdOnDate} but found {suppSilverAuditDoc.CreatedOn.Value.Date.ToString("dd-MMM-yyyy")}");

            Assert.True(suppSilverAuditDoc.DataUpdatedOn.Value.Date.ToString("dd-MMM-yyyy") == createdOnDate,
                $"In the Parent SSA Audit Doc {parentCustomerId}, Expected the Audit Doc UpdatedOn  to be {createdOnDate} but found {suppSilverAuditDoc.DataUpdatedOn.Value.Date.ToString("dd-MMM-yyyy")}");



            #endregion

            #region Validation of SSA Audit for SSA Child

            queryForSuppSilverAuditDoc = "SELECT TOP 1 * FROM AuditCollection c where c.DocType = @docType and c.AuditCategory = @auditCategory and c.PartitionKey  = @partitionKey and c.CreatedBy = @createdBy order by c.CreatedOn desc ";

            auditDocQuery = new QueryDefinition(query: queryForSuppSilverAuditDoc).WithParameter("@docType", "OtherAudit").WithParameter("@auditCategory", "Supplementary Silver").WithParameter("@partitionKey", childCustomerId).WithParameter("@createdBy", linkedBy);

            suppSilverAuditDoc = _cosmosFixture.CosmosQueryItem<OtherAudit>(auditDocQuery);

            retry = 5;

            while (retry > 0 && suppSilverAuditDoc is null)
            {
                await Task.Delay(1000);

                suppSilverAuditDoc = _cosmosFixture.CosmosQueryItem<OtherAudit>(auditDocQuery);

                retry--;
            }

            Assert.True(suppSilverAuditDoc is not null, $"No Supp Silver Audit Document Found for the SSA Child Customer {childCustomerId} after Successful Linking");

            Assert.True(suppSilverAuditDoc.RelationshipType == "Linked Gold",
                $"In the Child SSA Audit Doc {childCustomerId}, Expected the RelationType with parent to be Linked Gold but found {suppSilverAuditDoc.RelationshipType}");

            Assert.True(suppSilverAuditDoc.AuditSubCategory == "Linking",
                $"In the Child SSA Audit Doc {childCustomerId}, Expected the Audit Sub Category to be Linking but found {suppSilverAuditDoc.AuditSubCategory}");

            Assert.True(suppSilverAuditDoc.AuditData == parentMemberNumber,
                $"In the Child SSA Audit Doc {childCustomerId}, Expected the Audit Data / MemberNumber to be {parentMemberNumber} but found {suppSilverAuditDoc.AuditData}");

            Assert.True(suppSilverAuditDoc.DataUpdatedBy == linkedBy,
                $"In the Child SSA Audit Doc {childCustomerId}, Expected the AuditDoc UpdatedBy to be {linkedBy} but found {suppSilverAuditDoc.DataUpdatedBy}");

            Assert.True(suppSilverAuditDoc.CreatedOn.Value.Date.ToString("dd-MMM-yyyy") == createdOnDate,
                $"In the Child SSA Audit Doc {childCustomerId}, Expected the Audit Doc CreatedOn  to be {createdOnDate} but found {suppSilverAuditDoc.CreatedOn.Value.Date.ToString("dd-MMM-yyyy")}");

            Assert.True(suppSilverAuditDoc.DataUpdatedOn.Value.Date.ToString("dd-MMM-yyyy") == createdOnDate,
                $"In the Child SSA Audit Doc {childCustomerId}, Expected the Audit Doc UpdatedOn  to be {createdOnDate} but found {suppSilverAuditDoc.DataUpdatedOn.Value.Date.ToString("dd-MMM-yyyy")}");

            #endregion

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(42)]
        public async Task TC03_ValidateSupplementarySilverAuditHistoryAfterSSADelinking_SuccessScenario()
        {


            #region Pre-Requisite for Fetching the Child member details and SSA member and  CustomerId from the FC document 

            string queryForFCMDocument = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and  c.SSAMemberNumber <> '' order by c._ts desc";

            QueryDefinition query = new QueryDefinition(query: queryForFCMDocument).WithParameter("@docType", "FlyingClubMember");

            FlyingClubMember flyingClubMember = _cosmosFixture.CosmosQueryItem<FlyingClubMember>(query);

            int retry = 5;

            while (retry > 0 && flyingClubMember == null)
            {
                await Task.Delay(1000);

                flyingClubMember = _cosmosFixture.CosmosQueryItem<FlyingClubMember>(query);

                retry--;
            }

            Assert.True(flyingClubMember is not null, "Didn't find any SSA child Member for Delinking");

            string childCustomerId = flyingClubMember.CustomerId;

            string childMemberNumber = flyingClubMember.MemberNumber;

            string parentMemberNumber = flyingClubMember.SSAMemberNumber;

            string queryForSSAFCMDocument = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and  c.MemberNumber = @ssaMemberNumber";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForSSAFCMDocument).WithParameter("@docType", "FlyingClubMember").WithParameter("@ssaMemberNumber", parentMemberNumber);

            FlyingClubMember ssaFlyingClubMember = _cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            retry = 5;

            while (retry > 0 && ssaFlyingClubMember == null)
            {
                await Task.Delay(1000);

                ssaFlyingClubMember = _cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

                retry--;
            }

            Assert.True(ssaFlyingClubMember is not null, $"Din't find SSA Member Number associated with the child Member {childCustomerId} .");


            string parentCustomerId = ssaFlyingClubMember.CustomerId;

            string delinkedBy = DataGenerator.RandomFirstName();

            #endregion

            #region Supplementary Account Delinking using GraphQL endpoint

            HttpResponseMessage response = GraphQL.SupplementarySilverDelink(_tokenFixture.GetToken("graphqlv2"), childCustomerId, childMemberNumber
                                    , parentCustomerId, parentMemberNumber, delinkedBy).EnsureIsSuccessStatusCode();

            dynamic delinkResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Request successfully initiated";

            Assert.True(delinkResult.data?.loyaltySupplementarySilverDelinking?.ToString().Contains(expectedResponse),
                $"Expected {expectedResponse} in the response text but found {delinkResult?.ToString()} ");

            #endregion

            #region Validation of SSA Audit for SSA Parent

            string queryForSuppSilverAuditDoc = "SELECT TOP 1 * FROM AuditCollection c where c.DocType = @docType and c.AuditCategory = @auditCategory and c.PartitionKey  = @partitionKey and c.CreatedBy = @createdBy order by c.CreatedOn desc ";

            QueryDefinition auditDocQuery = new QueryDefinition(query: queryForSuppSilverAuditDoc).WithParameter("@docType", "OtherAudit").WithParameter("@auditCategory", "Supplementary Silver").WithParameter("@partitionKey", parentCustomerId).WithParameter("@createdBy", delinkedBy);

            OtherAudit suppSilverAuditDoc = _cosmosFixture.CosmosQueryItem<OtherAudit>(auditDocQuery);

            retry = 5;

            while (retry > 0 && suppSilverAuditDoc is null)
            {
                await Task.Delay(1000);

                suppSilverAuditDoc = _cosmosFixture.CosmosQueryItem<OtherAudit>(auditDocQuery);

                retry--;
            }

            Assert.True(suppSilverAuditDoc is not null, $"No Supp Silver Audit Document Found for the SSA Parent Customer {parentCustomerId} after Successful Linking");

            Assert.True(suppSilverAuditDoc.RelationshipType == "Supp Silver",
                $"In the Parent SSA Audit Doc {parentCustomerId}, Expected the RelationType with the child to be Supp Silver but found {suppSilverAuditDoc.RelationshipType}");

            Assert.True(suppSilverAuditDoc.AuditSubCategory == "Delinking",
                $"In the Parent SSA Audit Doc {parentCustomerId}, Expected the Audit Sub Category to be Delinking but found {suppSilverAuditDoc.AuditSubCategory}");

            Assert.True(suppSilverAuditDoc.AuditData == childMemberNumber,
                $"In the Parent SSA Audit Doc {parentCustomerId}, Expected the Audit Data / MemberNumber to be {childMemberNumber} but found {suppSilverAuditDoc.AuditData}");

            Assert.True(suppSilverAuditDoc.DataUpdatedBy == delinkedBy,
                $"In the Parent SSA Audit Doc {parentCustomerId}, Expected the AuditDoc UpdatedBy to be {delinkedBy} but found {suppSilverAuditDoc.DataUpdatedBy}");

            string createdOnDate = DateTime.UtcNow.ToString("dd-MMM-yyyy");

            Assert.True(suppSilverAuditDoc.CreatedOn.Value.Date.ToString("dd-MMM-yyyy") == createdOnDate,
                $"In the Parent SSA Audit Doc {parentCustomerId}, Expected the Audit Doc CreatedOn  to be {createdOnDate} but found {suppSilverAuditDoc.CreatedOn.Value.Date.ToString("dd-MMM-yyyy")}");

            Assert.True(suppSilverAuditDoc.DataUpdatedOn.Value.Date.ToString("dd-MMM-yyyy") == createdOnDate,
                $"In the Parent SSA Audit Doc {parentCustomerId}, Expected the Audit Doc UpdatedOn  to be {createdOnDate} but found {suppSilverAuditDoc.DataUpdatedOn.Value.Date.ToString("dd-MMM-yyyy")}");



            #endregion

            #region Validation of SSA Audit for SSA Child

            queryForSuppSilverAuditDoc = "SELECT TOP 1 * FROM AuditCollection c where c.DocType = @docType and c.AuditCategory = @auditCategory and c.PartitionKey  = @partitionKey and c.CreatedBy = @createdBy order by c.CreatedOn desc ";

            auditDocQuery = new QueryDefinition(query: queryForSuppSilverAuditDoc).WithParameter("@docType", "OtherAudit").WithParameter("@auditCategory", "Supplementary Silver").WithParameter("@partitionKey", childCustomerId).WithParameter("@createdBy", delinkedBy);

            suppSilverAuditDoc = _cosmosFixture.CosmosQueryItem<OtherAudit>(auditDocQuery);

            retry = 5;

            while (retry > 0 && suppSilverAuditDoc is null)
            {
                await Task.Delay(1000);

                suppSilverAuditDoc = _cosmosFixture.CosmosQueryItem<OtherAudit>(auditDocQuery);

                retry--;
            }

            Assert.True(suppSilverAuditDoc is not null, $"No Supp Silver Audit Document Found for the SSA Child Customer {childCustomerId} after Successful Linking");

            Assert.True(suppSilverAuditDoc.RelationshipType == "Linked Gold",
                $"In the Child SSA Audit Doc {childCustomerId}, Expected the RelationType with parent to be Linked Gold but found {suppSilverAuditDoc.RelationshipType}");

            Assert.True(suppSilverAuditDoc.AuditSubCategory == "Delinking",
                $"In the Child SSA Audit Doc {childCustomerId}, Expected the Audit Sub Category to be Delinking but found {suppSilverAuditDoc.AuditSubCategory}");

            Assert.True(suppSilverAuditDoc.AuditData == parentMemberNumber,
                $"In the Child SSA Audit Doc {childCustomerId}, Expected the Audit Data / MemberNumber to be {parentMemberNumber} but found {suppSilverAuditDoc.AuditData}");

            Assert.True(suppSilverAuditDoc.DataUpdatedBy == delinkedBy,
                $"In the Child SSA Audit Doc {childCustomerId}, Expected the AuditDoc UpdatedBy to be {delinkedBy} but found {suppSilverAuditDoc.DataUpdatedBy}");

            Assert.True(suppSilverAuditDoc.CreatedOn.Value.Date.ToString("dd-MMM-yyyy") == createdOnDate,
                $"In the Child SSA Audit Doc {childCustomerId}, Expected the Audit Doc CreatedOn  to be {createdOnDate} but found {suppSilverAuditDoc.CreatedOn.Value.Date.ToString("dd-MMM-yyyy")}");

            Assert.True(suppSilverAuditDoc.DataUpdatedOn.Value.Date.ToString("dd-MMM-yyyy") == createdOnDate,
                $"In the Child SSA Audit Doc {childCustomerId}, Expected the Audit Doc UpdatedOn  to be {createdOnDate} but found {suppSilverAuditDoc.DataUpdatedOn.Value.Date.ToString("dd-MMM-yyyy")}");

            #endregion

        }

    }
}
