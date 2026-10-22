using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Helpers;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.GraphQL;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.ProductionSanity
{
    [Collection("ProductionSanityTests")]
    public class GraphQLTests : IClassFixture<CosmosFixture>, IClassFixture<TokenFixture>
    {

        private readonly CosmosFixture _cosmosFixture;

        private readonly TokenFixture _tokenFixture;

        public static string env = Config.Environment;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;
        private readonly string auditCosmosDb;

        private static string ocpSubKey = string.Empty;

        public GraphQLTests(CosmosFixture cosmosFixture, TokenFixture tokenFixture)
        {
            _cosmosFixture = cosmosFixture;

            _tokenFixture = tokenFixture;

            customerCosmosDb = Application.CustomerCosmosDbProd;
            customerTransactionCosmosDb = Application.CustomerTransactionCosmosDbProd;
            auditCosmosDb = Application.AuditCosmosDbProd;

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpointProd;

            _cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            _cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            _cosmosFixture.RegisterType<Transaction>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            string cosmosEndpoint = Application.CustomerCosmosDbEndpointProd;

            _cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));

            _cosmosFixture.RegisterType<Core.DB_Model.Entities.Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

            _cosmosFixture.RegisterType<CustomerSearch>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

            string auditCosmosEndpoint = Application.AuditCosmosDbEndpointProd;

            _cosmosFixture.RegisterClient(auditCosmosEndpoint, Vault.Default.GetSecretValue("TESTING-CmdbAudit-Automation-Key"));

            _cosmosFixture.RegisterType<AwardAudit>(auditCosmosEndpoint, auditCosmosDb, Application.AuditCosmosCollection);

            ocpSubKey = Vault.Default.GetSecretValue("Testing-Ocp-Apim-SubscriptionKey") ?? "";

            _tokenFixture.AddTokenForGraphQlFromKeyVault("GraphQLProd");

        }

        [Trait("Category", Traits.ProductionGraphQLSmokeRegression)]
        [Fact]
        public async Task CampaignSearch()
        {

            string queryForCampaignSearch = "SELECT TOP 1 CampaignCode from [dbo].[Campaigns] order by NEWID()";

            string campCode = (string)AzureReferenceDBHelper.RefDbQueryResponse(queryForCampaignSearch);

            HttpResponseMessage response = GraphQL.CampaignSearch(_tokenFixture.GetToken("GraphQLProd"), "", "", campCode, "", "", "", "", "", "", ocpSubKey).EnsureIsSuccessStatusCode();

            dynamic? result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            CampaignDetails campaignDetails = JsonConvert.DeserializeObject<CampaignDetails>(result.data.loyaltyCampaignSearch.items[0].ToString());

            Assert.True(campaignDetails is not null, $"Expected Campaign details for the searched campaign code {campCode} but found null response");

            Assert.True(campCode.Equals(campaignDetails.CampaignCode), $"Expected the CampaignCode in the response to be {campCode} but found {campaignDetails.CampaignCode}.");
        }

        [Trait("Category", Traits.ProductionGraphQLSmokeRegression)]
        [Fact]
        public async Task PromotionSearch()
        {
            string strInputQueryForPromotionName = "Select Top 1 Name from dbo.Promotions order by NEWID()";

            string promotionName = (string)AzureReferenceDBHelper.RefDbQueryResponse(strInputQueryForPromotionName);

            HttpResponseMessage response = GraphQL.PromotionSearchAndRetrieve(_tokenFixture.GetToken("GraphQLProd"), "", promotionName, "", "", "", "", "", "", ocpSubKey).EnsureIsSuccessStatusCode();

            string promotionSearchAndRetrieveResult = await response.Content.ReadAsStringAsync();

            Assert.True(!promotionSearchAndRetrieveResult.Contains("Error", StringComparison.OrdinalIgnoreCase), $"Response was not as expected . Response returned is \n {promotionSearchAndRetrieveResult}");

            Assert.True(!promotionSearchAndRetrieveResult.Contains("Failed", StringComparison.OrdinalIgnoreCase), $"Response was not as expected . Response returned is \n {promotionSearchAndRetrieveResult}");
        }

        [Trait("Category", Traits.ProductionGraphQLSmokeRegression)]
        [Fact]
        public async Task RetrieveMemberAccountDetails()
        {
            string queryForTxnDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TransactionType = @txnType and c.StatusCode = @statusCode and c.TransactionValue > 0 order by c.CreatedOn desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForTxnDoc).WithParameter("@docType", "Transaction").WithParameter("@txnType", "Accrual").WithParameter("@statusCode", "Processed");

            Transaction txnDoc = _cosmosFixture.CosmosQueryItem<Transaction>(paramQuery);

            Assert.True(txnDoc is not null, "Failed to Retrieve Test Data for Credit Transaction.");

            string customerId = txnDoc.CustomerId;

            HttpResponseMessage memberAccountDetailsResponse = GraphQL.RetrieveMemberAccountDetails(_tokenFixture.GetToken("GraphQLProd"), customerId, "", ocpSubKey).EnsureIsSuccessStatusCode();

            string retrieveMemberAccountResult = await memberAccountDetailsResponse.Content.ReadAsStringAsync();

            Assert.True(retrieveMemberAccountResult.Contains(customerId), $"The Retrieve Member Tier Acc Details response is not as expected. Expected {customerId} to be present in response but was {retrieveMemberAccountResult}");

            Assert.True(!retrieveMemberAccountResult.Contains("Error", StringComparison.OrdinalIgnoreCase), $"Response was not as expected . Response returned is \n {retrieveMemberAccountResult}");

            Assert.True(!retrieveMemberAccountResult.Contains("Failed", StringComparison.OrdinalIgnoreCase), $"Response was not as expected . Response returned is \n {retrieveMemberAccountResult}");
        }

        [Trait("Category", Traits.ProductionGraphQLSmokeRegression)]
        [Fact]
        public async Task RetrieveHouseholdMemberDetails()
        {

            string headAccountCustomerId = string.Empty;
            string headAccountMemberNumber = string.Empty;
            string subOrdinateAccountCustomerId = string.Empty;
            string subOrdinateAccountMemberNumber = string.Empty;

            string queryForFCMDoc = "SELECT TOP 1 * FROM  c where c.DocType = @docType  and c.Status = @status and  c.MemberType = @memberType and c.HeadOfGroupMemberNumber !='' order by c.LastUpdatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@status", "Active").WithParameter("@memberType", "Individual");

            FlyingClubMember flyingClubDocument = _cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            int retry = 5;

            while (retry > 0 && flyingClubDocument == null)
            {
                Thread.Sleep(500);

                flyingClubDocument = _cosmosFixture.QueryItem<FlyingClubMember>(queryForFCMDoc);

                retry--;
            }

            Assert.True(flyingClubDocument is not null, $"Failed to Retrieve the Test Data .No Document available for the Searched Query {queryForFCMDoc}.");

            headAccountMemberNumber = flyingClubDocument.HeadOfGroupMemberNumber;

            string queryForCustomerSearchDoc = $"SELECT TOP 1 * FROM  c where c.DocType='CustomerSearch'  and c.LoyaltyMemberNumber='{headAccountMemberNumber}'";

            CustomerSearch customer = _cosmosFixture.QueryItem<CustomerSearch>(queryForCustomerSearchDoc);

            retry = 5;

            while (retry > 0 && customer == null)
            {
                Thread.Sleep(500);

                customer = _cosmosFixture.QueryItem<CustomerSearch>(queryForCustomerSearchDoc);

                retry--;
            }

            Assert.True(customer is not null, $"No Document available for the Searched Query {queryForCustomerSearchDoc}.");

            headAccountCustomerId = customer.CustomerId;

            HttpResponseMessage response = GraphQL.RetrieveHouseHoldDetails(_tokenFixture.GetToken("GraphQLProd"), headAccountCustomerId, "", ocpSubKey).EnsureIsSuccessStatusCode();

            string retrieveHouseHoldDetailsResult = await response.Content.ReadAsStringAsync();

            HouseHoldDetailsRetrieval houseHoldDetailsRetrieval = JsonConvert.DeserializeObject<HouseHoldDetailsRetrieval>(retrieveHouseHoldDetailsResult);

            Assert.True(houseHoldDetailsRetrieval is not null, $"Failed to retrieve the member Household Details for customer {headAccountCustomerId}, the response was {retrieveHouseHoldDetailsResult} ");

            Assert.True(!retrieveHouseHoldDetailsResult.Contains("Error", StringComparison.OrdinalIgnoreCase), $"Response was not as expected . Response returned is \n {retrieveHouseHoldDetailsResult}");

            Assert.True(!retrieveHouseHoldDetailsResult.Contains("Failed", StringComparison.OrdinalIgnoreCase), $"Response was not as expected . Response returned is \n {retrieveHouseHoldDetailsResult}");
        }

        [Trait("Category", Traits.ProductionGraphQLSmokeRegression)]
        [Fact]
        public async Task RetrieveMemberAwardAuditHistory()
        {
            string queryForAwardAuditDoc = "SELECT TOP 1 * FROM AuditCollection c where c.DocType = @docType order by c.CreatedOn desc ";

            QueryDefinition query = new QueryDefinition(query: queryForAwardAuditDoc).WithParameter("@docType", "AwardAudit");

            AwardAudit awardAuditDoc = _cosmosFixture.CosmosQueryItem<AwardAudit>(query);

            Assert.True(awardAuditDoc is not null, "No Award Audit Document Found.");

            string customerId = awardAuditDoc.CustomerId;

            HttpResponseMessage response = GraphQL.RetrieveAwardAudit(_tokenFixture.GetToken("GraphQLProd"), customerId, "", ocpSubKey).EnsureIsSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();

            Assert.True(!result.Contains("Request failed"), $"Expected a valid response for retrieve award audit graphql operation but found {result}");

            Assert.True(!result.Contains("Error", StringComparison.OrdinalIgnoreCase), $"Response was not as expected . Response returned is \n {result}");

            Assert.True(!result.Contains("Failed", StringComparison.OrdinalIgnoreCase), $"Response was not as expected . Response returned is \n {result}");
        }

        [Trait("Category", Traits.ProductionGraphQLSmokeRegression)]
        [Fact]
        public async Task CustomerSearch()
        {
            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) order by c._ts desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC");

            FlyingClubMember fcmDoc = _cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            string customerId = fcmDoc.CustomerId;

            HttpResponseMessage response = GraphQL.CustomerSearch(_tokenFixture.GetToken("GraphQLProd"), customerId, "", ocpSubKey).EnsureIsSuccessStatusCode();

            string customerSearchResult = await response.Content.ReadAsStringAsync();

            Assert.True(!customerSearchResult.Contains("Request failed"), $"Expected a valid response for CustomerSearch graphql operation but found {customerSearchResult}");

            Assert.True(!customerSearchResult.Contains("Error", StringComparison.OrdinalIgnoreCase), $"Response was not as expected . Response returned is \n {customerSearchResult}");

            Assert.True(!customerSearchResult.Contains("Failed", StringComparison.OrdinalIgnoreCase), $"Response was not as expected . Response returned is \n {customerSearchResult}");
        }

    }
}
