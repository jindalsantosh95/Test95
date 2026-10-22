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


namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.ContactNotes
{

    [Collection("LoyaltyNotesTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class LoyaltyDeleteNoteTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {

        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;

        private PreRequisiteTestData preRequisiteTestData;

        private string memberNumber = "";

        public static string env = Config.Environment;

        private readonly string customerTransactionCosmosDb;

        public LoyaltyDeleteNoteTests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {
            this.tokenFixture = tokenFixture;

            this.tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");

            this.cosmosFixture = cosmosFixture;

            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<ContactNote>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            preRequisiteTestData = new PreRequisiteTestData();

            memberNumber = preRequisiteTestData.GetValidFCNumber(out _);
        }

        Random random = new();

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(11)]
        public async Task A_LoyaltyDeleteNote_SuccessScenario_DeleteNoteUsingCustomerId()
        {

            #region Test Data

            string queryForContactNoteDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForContactNoteDoc).WithParameter("@docType", "ContactNote");

            ContactNote contactNoteDoc = cosmosFixture.CosmosQueryItem<ContactNote>(parameterizedQuery);

            string customerId = contactNoteDoc.CustomerId;

            #endregion

            HttpResponseMessage response = GraphQL.LoyaltyGetNote(tokenFixture.GetToken("graphqlv2"), customerId, "");

            dynamic loyaltyGetNoteResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string noteId = loyaltyGetNoteResult.data.loyaltyContactNotes[random.Next(loyaltyGetNoteResult.data.loyaltyContactNotes.Count)].id.ToString();

            response = GraphQL.LoyaltyDeleteNote(tokenFixture.GetToken("graphqlv2"), customerId, "", "", "", "", "", "",
                                                                       null, null, noteId);

            dynamic loyaltyDeleteNoteResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(!DynamicHelper.HasPropertyWithValue(loyaltyDeleteNoteResult, "message", "Request Failed"),
                $"Expected valid response but found {loyaltyDeleteNoteResult}");
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(12)]
        public async Task B_LoyaltyDeleteNote_SuccessScenario_DeleteNoteUsingMemberNumber()
        {
            #region Test Data

            string queryForContactNoteDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForContactNoteDoc).WithParameter("@docType", "ContactNote");

            ContactNote contactNoteDoc = cosmosFixture.CosmosQueryItem<ContactNote>(parameterizedQuery);

            string customerId = contactNoteDoc.CustomerId;

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @customerId ";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@customerId", customerId);

            FlyingClubMember fcmDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(paramQuery);

            string memberNumber = fcmDoc.MemberNumber;

            #endregion

            HttpResponseMessage response = GraphQL.LoyaltyGetNote(tokenFixture.GetToken("graphqlv2"), customerId, "");

            dynamic loyaltyGetNoteResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string noteId = loyaltyGetNoteResult.data.loyaltyContactNotes[random.Next(loyaltyGetNoteResult.data.loyaltyContactNotes.Count)].id.ToString();

            response = GraphQL.LoyaltyDeleteNote(tokenFixture.GetToken("graphqlv2"), "", memberNumber, "", "", "", "", "",
                                                                       null, null, noteId);

            dynamic loyaltyDeleteNoteResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(!DynamicHelper.HasPropertyWithValue(loyaltyDeleteNoteResult, "message", "Request Failed"),
                $"Expected valid response but found {loyaltyDeleteNoteResult}");
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(13)]
        public async Task C_LoyaltyDeleteNote_MandatoryFieldMissingError_WhenIdFieldIsNullOrEmpty()
        {

            HttpResponseMessage response = GraphQL.LoyaltyDeleteNote(tokenFixture.GetToken("graphqlv2"), "", memberNumber, "", "", "", "", "",
                                                                       null, null, "");

            dynamic loyaltyDeleteNoteResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "'Id' must not be empty.";

            Assert.True(loyaltyDeleteNoteResult.ToString().Contains(expectedResponse), "The Expected Response does not match.");

        }

    }
}
