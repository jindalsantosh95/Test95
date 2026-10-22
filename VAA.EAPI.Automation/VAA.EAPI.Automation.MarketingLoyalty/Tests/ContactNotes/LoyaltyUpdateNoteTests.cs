using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    public class LoyaltyUpdateNoteTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {

        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;
        PreRequisiteTestData preRequisiteTestData;

        private readonly static string[] validGroupTypes = { "GEN", "TOP50", "SPEC1", "SPEC2", "FOOD", "SEAT", "FWY", "MISC" };
        private readonly static string[] validNoteTypes = { "GEN", "INFO", "PREF", "SPEC" };
        private readonly Random random = new();

        private readonly string customerTransactionCosmosDb;

        public static string env = Config.Environment;

        public LoyaltyUpdateNoteTests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {
            this.tokenFixture = tokenFixture;

            this.tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");

            this.cosmosFixture = cosmosFixture;

            preRequisiteTestData = new();

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<ContactNote>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(18)]
        public async Task A_LoyaltyUpdateNote_SuccessScenario_UpdateUsingCustomerId()
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

            int i = random.Next(validGroupTypes.Length);

            int j = random.Next(validNoteTypes.Length);

            string inputText = DataGenerator.RandomString(10);

            response = GraphQL.LoyaltyUpdateNote(tokenFixture.GetToken("graphqlv2"), customerId, "",
                                                   validGroupTypes[i], validNoteTypes[j], inputText,
                                                   "testScript", "testScript", null, null, noteId);

            dynamic loyaltyUpdateNoteResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            LoyaltyNote loyaltyNotes = ((JObject)loyaltyUpdateNoteResult.data.loyaltyUpdateNote).ToObject<LoyaltyNote>();

            Assert.True(loyaltyNotes.text.Equals(inputText),
                $"The Text is not as expected.Expected {inputText} but found {loyaltyNotes.text}");

            Assert.True(loyaltyNotes.group.Equals(validGroupTypes[i]),
                $"The Group type is not as expected.Expected {validGroupTypes[i]} but found {loyaltyNotes.group}");

            Assert.True(loyaltyNotes.type.Equals(validNoteTypes[j]),
                $"The Group type is not as expected.Expected {validNoteTypes[j]} but found {loyaltyNotes.type}");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(19)]
        public async Task B_LoyaltyUpdateNote_SuccessScenario_UpdateUsingMemberNumber()
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

            HttpResponseMessage response = GraphQL.LoyaltyGetNote(tokenFixture.GetToken("graphqlv2"), "", memberNumber);

            dynamic loyaltyGetNoteResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string noteId = loyaltyGetNoteResult.data.loyaltyContactNotes[random.Next(loyaltyGetNoteResult.data.loyaltyContactNotes.Count)].id.ToString();

            int i = random.Next(validGroupTypes.Length);

            int j = random.Next(validNoteTypes.Length);

            string inputText = DataGenerator.RandomString(10);


            response = GraphQL.LoyaltyUpdateNote(tokenFixture.GetToken("graphqlv2"), "", memberNumber,
                                                   validGroupTypes[i], validNoteTypes[j], inputText,
                                                   "testScript", "testScript", null, null, noteId);

            dynamic loyaltyUpdateNoteResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            LoyaltyNote loyaltyNotes = ((JObject)loyaltyUpdateNoteResult.data.loyaltyUpdateNote).ToObject<LoyaltyNote>();

            Assert.True(loyaltyNotes.text.Equals(inputText),
                $"The Text is not as expected.Expected {inputText} but found {loyaltyNotes.text}");

            Assert.True(loyaltyNotes.group.Equals(validGroupTypes[i]),
                $"The Group type is not as expected.Expected {validGroupTypes[i]} but found {loyaltyNotes.group}");

            Assert.True(loyaltyNotes.type.Equals(validNoteTypes[j]),
                $"The Group type is not as expected.Expected {validNoteTypes[j]} but found {loyaltyNotes.type}");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(20)]
        public async Task C_LoyaltyUpdateNote_MandatoryFieldMissingError_WhenBothCustomerIdandMemberNumberPassedIsNullOrEmpty()
        {

            string queryForContactNoteDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForContactNoteDoc).WithParameter("@docType", "ContactNote");

            ContactNote contactNoteDoc = cosmosFixture.CosmosQueryItem<ContactNote>(parameterizedQuery);

            string customerId = contactNoteDoc.CustomerId;

            HttpResponseMessage response = GraphQL.LoyaltyGetNote(tokenFixture.GetToken("graphqlv2"), customerId, "");

            dynamic loyaltyGetNoteResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(loyaltyGetNoteResult.data.loyaltyContactNotes.Count > 0, $"No Notes found for the member {customerId}.");

            string noteId = loyaltyGetNoteResult.data.loyaltyContactNotes[0].id.ToString();

            response = GraphQL.LoyaltyUpdateNote(tokenFixture.GetToken("graphqlv2"), "", "",
                                                    "GEN", "GEN", "Welcome to VAA",
                                                    "testScript", "testScript", null, null, noteId);

            dynamic loyaltyUpdateNoteResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Either 'Customer ID' or 'Member Number' must be specified";

            Assert.True(loyaltyUpdateNoteResult.ToString().Contains(expectedResponse), "The Expected Response does not match.");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(21)]
        public async Task D_LoyaltyUpdateNote_MandatoryFieldMissingError_WhenIdFieldValuePassedIsNullorEmpty()
        {
            string customerId;

            string memberNumber = preRequisiteTestData.GetValidFCNumber(out customerId);

            HttpResponseMessage response = GraphQL.LoyaltyUpdateNote(tokenFixture.GetToken("graphqlv2"), customerId, memberNumber,
                                                     "GEN", "GEN", "Welcome to VAA",
                                                     "testScript", "testScript", null, null, "");

            dynamic loyaltyUpdateNoteResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "'Id' must not be empty.";

            Assert.True(loyaltyUpdateNoteResult.ToString().Contains(expectedResponse), "The Expected Response does not match.");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(22)]
        public async Task E_LoyaltyUpdateNote_InvalidFieldValueError_WhenIdFieldValueIsInvalid()
        {
            string customerId;

            string memberNumber = preRequisiteTestData.GetValidFCNumber(out customerId);

            HttpResponseMessage response = GraphQL.LoyaltyUpdateNote(tokenFixture.GetToken("graphqlv2"), customerId, memberNumber,
                                                     "GEN", "GEN", "Welcome to VAA",
                                                     "testScript", "testScript", null, null, "af5d-4tr5-3uhy-8ujf-3y6t");

            dynamic loyaltyUpdateNoteResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Field value is missing or invalid.";

            Assert.True(loyaltyUpdateNoteResult.ToString().Contains(expectedResponse), "The Expected Response does not match.");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(23)]
        public async Task F_LoyaltyUpdateNote_InvalidFieldValueError_WhenCustomerIdPassedIsInvalid()
        {

            string queryForContactNoteDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForContactNoteDoc).WithParameter("@docType", "ContactNote");

            ContactNote contactNoteDoc = cosmosFixture.CosmosQueryItem<ContactNote>(parameterizedQuery);

            string customerId = contactNoteDoc.CustomerId;

            HttpResponseMessage response = GraphQL.LoyaltyGetNote(tokenFixture.GetToken("graphqlv2"), customerId, "");

            dynamic loyaltyGetNoteResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(loyaltyGetNoteResult.data.loyaltyContactNotes.Count > 0, $"No Notes found for the member {customerId}.");

            string noteId = loyaltyGetNoteResult.data.loyaltyContactNotes[0].id.ToString();

            response = GraphQL.LoyaltyUpdateNote(tokenFixture.GetToken("graphqlv2"), "719589ab-75ed-40ac-94f4-50d37sc1a05", "",
                                                    "GEN", "GEN", "Welcome to VAA",
                                                    "testScript", "testScript", null, null, noteId);

            dynamic loyaltyUpdateNoteResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Field value is missing or invalid.";

            Assert.True(loyaltyUpdateNoteResult.ToString().Contains(expectedResponse), "The Expected Response does not match.");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(24)]
        public async Task G_LoyaltyUpdateNote_InvalidFieldValueError_WhenMemberNumberPassedIsInvalid()
        {

            string queryForContactNoteDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForContactNoteDoc).WithParameter("@docType", "ContactNote");

            ContactNote contactNoteDoc = cosmosFixture.CosmosQueryItem<ContactNote>(parameterizedQuery);

            string customerId = contactNoteDoc.CustomerId;

            HttpResponseMessage response = GraphQL.LoyaltyGetNote(tokenFixture.GetToken("graphqlv2"), customerId, "");

            dynamic loyaltyGetNoteResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(loyaltyGetNoteResult.data.loyaltyContactNotes.Count > 0, $"No Notes found for the member {customerId}.");

            string noteId = loyaltyGetNoteResult.data.loyaltyContactNotes[0].id.ToString();

            response = GraphQL.LoyaltyUpdateNote(tokenFixture.GetToken("graphqlv2"), "", "2104522323",
                                                    "GEN", "GEN", "Welcome to VAA",
                                                    "testScript", "testScript", null, null, noteId);

            dynamic loyaltyUpdateNoteResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Data is unavailable from the source.";

            Assert.True(loyaltyUpdateNoteResult.ToString().Contains(expectedResponse), "The Expected Response does not match.");

        }

    }
}
