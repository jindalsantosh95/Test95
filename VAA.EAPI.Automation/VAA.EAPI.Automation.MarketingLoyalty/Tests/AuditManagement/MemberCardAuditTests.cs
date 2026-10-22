using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.GraphQL;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.AuditManagement
{

    [Collection("LoyaltyMemberCardLinkTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class MemberCardAuditTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {

        private readonly TokenFixture _tokenFixture;

        private readonly CosmosFixture _cosmosFixture;

        public string env = Config.Environment;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;
        private readonly string auditCosmosdb;


        public MemberCardAuditTests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {
            _tokenFixture = tokenFixture;

            _cosmosFixture = cosmosFixture;

            _tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;
            auditCosmosdb = env.Is("uat") ? Application.AuditCosmosDbUAT : Application.AuditCosmosDbSIT;

            string auditCosmosEndpoint = Application.AuditCosmosDbEndpoint;

            _cosmosFixture.RegisterClient(auditCosmosEndpoint, Vault.Default.GetSecretValue("TESTING-CmdbAudit-Automation-Key"));

            _cosmosFixture.RegisterType<CardLinkingAudit>(auditCosmosEndpoint, auditCosmosdb, Application.AuditCosmosCollection);

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            _cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            _cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;

            _cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));

            _cosmosFixture.RegisterType<Core.DB_Model.Entities.Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(6)]
        public async Task TC01_ValidateCardLinkingAuditHistory_SuccessScenario()
        {

            #region Test Data    

            string queryForCardLinkingAuditDoc = "SELECT TOP 1 * FROM AuditCollection c where c.DocType = @docType order by c.CreatedOn desc ";

            QueryDefinition query = new QueryDefinition(query: queryForCardLinkingAuditDoc).WithParameter("@docType", "CardLinkingAudit");

            CardLinkingAudit cardLinkingAuditDoc = _cosmosFixture.CosmosQueryItem<CardLinkingAudit>(query);

            Assert.True(cardLinkingAuditDoc is not null, "No Card Linking Audit Document Found.");

            string customerId = cardLinkingAuditDoc.CustomerId;

            string queryForCustCardLinkingAuditDocs = "SELECT * FROM AuditCollection c where c.DocType = @docType and c.PartitionKey = @customerId ";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForCustCardLinkingAuditDocs).WithParameter("@docType", "CardLinkingAudit").WithParameter("@customerId", customerId);

            IEnumerable<CardLinkingAudit> custCardLinkingAuditDocs = _cosmosFixture.CosmosQueryItems<CardLinkingAudit>(paramQuery).OrderByDescending(x => x.CreatedOn);

            Assert.True(custCardLinkingAuditDocs.Any(), $"The Card Linking Audit Docs count does not found for the CustomerId {customerId}.");


            #endregion

            #region API Call

            HttpResponseMessage response = GraphQL.RetrieveAmexCenturionAuditHistory(_tokenFixture.GetToken("graphqlv2"), customerId, "").EnsureIsSuccessStatusCode();

            dynamic result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(!result.ToString().Contains("Request failed"), $"Expected a valid response for retrieve Card Linking audit graphql operation but found {result.ToString()}");

            CardLinkingAuditResponse[] cardLinkingAuditResponse = JsonConvert.DeserializeObject<CardLinkingAuditResponse[]>(result?.data?.retrieveAmexCenturionAuditHistory?.amexCenturionAuditHistory?.ToString());

            cardLinkingAuditResponse = cardLinkingAuditResponse.OrderByDescending(x => x.createdOn).ToArray();

            Assert.True(custCardLinkingAuditDocs.Count() == cardLinkingAuditResponse.Length, $"The Card Linking Audit Docs count does not match for customer Id {customerId} \n . Cosmos doc count is {custCardLinkingAuditDocs.Count()} and response doc count is {cardLinkingAuditResponse.Length}");

            #endregion

            #region Validation

            SoftAssertHelper softAssert = new();

            int i = 0;

            foreach (CardLinkingAudit cardLinkingAudit in custCardLinkingAuditDocs)
            {

                softAssert.True(cardLinkingAudit.CreatedBy == cardLinkingAuditResponse[i].createdBy, $"For the Doc Id {cardLinkingAudit.id}, Expected the Card Linking Doc CreatedBy to be {cardLinkingAudit.CreatedBy} but found {cardLinkingAuditResponse[i].createdBy} ");

                softAssert.True(cardLinkingAudit.CardLinkingUpdatedBy == cardLinkingAuditResponse[i].cardLinkingUpdatedBy, $"For the Doc Id {cardLinkingAudit.id}, Expected the Card Linking Doc CardLinkingUpdatedBy to be {cardLinkingAudit.CardLinkingUpdatedBy} but found {cardLinkingAuditResponse[i].cardLinkingUpdatedBy} ");

                softAssert.True(cardLinkingAudit.CreatedOn.Value.Date.ToString("dd-MMM-yyyy") == cardLinkingAuditResponse[i].createdOn.Value.Date.ToString("dd-MMM-yyyy"), $"For the Doc Id {cardLinkingAudit.id}, Expected the Card Linking Doc CreatedOn to be {cardLinkingAudit.CreatedOn.Value.Date.ToString("dd-MMM-yyyy")} but found {cardLinkingAuditResponse[i].createdOn.Value.Date.ToString("dd-MMM-yyyy")} ");

                softAssert.True(cardLinkingAudit.CardLinkingUpdatedOn.Value.Date.ToString("dd-MMM-yyyy") == cardLinkingAuditResponse[i].cardLinkingUpdatedOn.Value.Date.ToString("dd-MMM-yyyy"), $"For the Doc Id {cardLinkingAudit.id}, Expected the Card Linking Doc CardLinkingUpdatedOn to be {cardLinkingAudit.CardLinkingUpdatedOn.Value.Date.ToString("dd-MMM-yyyy")} but found {cardLinkingAuditResponse[i].cardLinkingUpdatedOn.Value.Date.ToString("dd-MMM-yyyy")} ");

                softAssert.True(cardLinkingAudit.StartDate.Value.Date.ToString("dd-MMM-yyyy") == cardLinkingAuditResponse[i].startDate.Value.Date.ToString("dd-MMM-yyyy"), $"For the Doc Id {cardLinkingAudit.id}, Expected the Card Linking Doc StartDate to be {cardLinkingAudit.StartDate.Value.Date.ToString("dd-MMM-yyyy")} but found {cardLinkingAuditResponse[i].startDate.Value.Date.ToString("dd-MMM-yyyy")} ");

                string actualEndDate = cardLinkingAuditResponse[i].endDate.HasValue ? cardLinkingAuditResponse[i].endDate?.ToString("yyyy-MM-dd") : "";

                string expectedEndDate = cardLinkingAudit.EndDate.HasValue ? cardLinkingAudit.EndDate?.ToString("yyyy-MM-dd") : "";

                softAssert.True(actualEndDate == expectedEndDate, $"For the Doc Id {cardLinkingAudit.id}, Expected the Card Linking Doc EndDate to be {expectedEndDate} but found {actualEndDate}");

                softAssert.True(cardLinkingAudit.StatusEnhancementCode == cardLinkingAuditResponse[i].statusEnhancementCode, $"For the Doc Id {cardLinkingAudit.id}, Expected the Card Linking Doc StatusEnhancementCode to be {cardLinkingAudit.StatusEnhancementCode} but found {cardLinkingAuditResponse[i].statusEnhancementCode} ");

                softAssert.True(cardLinkingAudit.PartnerCode == cardLinkingAuditResponse[i].partnerCode, $"For the Doc Id {cardLinkingAudit.id}, Expected the Card Linking Doc PartnerCode to be {cardLinkingAudit.PartnerCode} but found {cardLinkingAuditResponse[i].partnerCode} ");

                i++;

            }

            softAssert.AssertAll();

            #endregion

        }
    }
}
