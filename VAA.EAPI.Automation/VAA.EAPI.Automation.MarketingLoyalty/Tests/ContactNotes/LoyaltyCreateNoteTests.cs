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
    public class LoyaltyCreateNoteTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {

        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;

        private PreRequisiteTestData preRequisiteTestData;

        private string memberNumber = "";
        private string customerId = "";

        public static string env = Config.Environment;

        private readonly string customerTransactionCosmosDb;

        public LoyaltyCreateNoteTests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {

            this.tokenFixture = tokenFixture;
            this.cosmosFixture = cosmosFixture;

            this.tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");

            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<ContactNote>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            preRequisiteTestData = new PreRequisiteTestData();

            memberNumber = preRequisiteTestData.GetValidFCNumber(out customerId);
        }



        private readonly static string[] validGroupTypes = { "GEN", "TOP50", "SPEC1", "SPEC2", "FOOD", "SEAT", "FWY", "MISC" };
        private readonly static string[] validNoteTypes = { "GEN", "INFO", "PREF", "SPEC" };
        Random random = new();

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(1)]
        public async Task A_LoyaltyCreateNote_SuccessScenario_CreateNoteUsingCustomerId()
        {

            #region Test Data

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) order by c._ts desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC");

            FlyingClubMember fcmDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            string customerId = fcmDoc.CustomerId;

            #endregion

            int i = random.Next(validGroupTypes.Length);

            int j = random.Next(validNoteTypes.Length);

            string notes = DataGenerator.RandomString(10);

            string createdBy = DataGenerator.RandomFirstName();

            HttpResponseMessage response = GraphQL.LoyaltyCreateNote(tokenFixture.GetToken("graphqlv2"), customerId, "",
                                                    validGroupTypes[i], validNoteTypes[j], notes, createdBy,
                                                    createdBy, null, null, "");

            dynamic loyaltyCreateNoteResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string queryForContactNotes = $"SELECT * FROM CustomerDetailsCollection c WHERE  c.DocType = 'ContactNote' and c.PartitionKey='{customerId}' order by c._ts desc";

            ContactNote contactNotes = cosmosFixture.QueryItem<ContactNote>(queryForContactNotes);

            int retry = 5;

            while (retry > 0 && contactNotes == null)
            {
                await Task.Delay(500);

                contactNotes = cosmosFixture.QueryItem<ContactNote>(queryForContactNotes);

                retry--;
            }


            Assert.True(contactNotes.Description.Equals(notes), $"Expected the note description as {notes} but found {contactNotes.Description}.");

            Assert.True(contactNotes.CreatedBy.Equals(createdBy), $"Expected the CreatedBy field Value as {createdBy} but found {contactNotes.CreatedBy}.");

            Assert.True(contactNotes.LastUpdatedBy.Equals(createdBy), $"Expected the LastUpdatedBy field Value as {createdBy} but found {contactNotes.LastUpdatedBy}.");

            Assert.True(contactNotes.NoteTypeGroup.Equals(validGroupTypes[i]), $"The Group type is not as expected.Expected {validGroupTypes[i]} but found {contactNotes.NoteTypeGroup}");

            Assert.True(contactNotes.NoteType.Equals(validNoteTypes[j]), $"The Group type is not as expected.Expected {validNoteTypes[j]} but found {contactNotes.NoteType}");

            LoyaltyNote loyaltyNotes = ((JObject)loyaltyCreateNoteResult.data.loyaltyCreateNote).ToObject<LoyaltyNote>();

            Assert.True(loyaltyNotes.text.Equals(notes), $"The Text is not as expected.Expected Welcome to VAA but found {loyaltyNotes.text}");

            Assert.True(loyaltyNotes.group.Equals(validGroupTypes[i]), $"The Group type is not as expected.Expected {validGroupTypes[i]} but found {loyaltyNotes.group}");

            Assert.True(loyaltyNotes.type.Equals(validNoteTypes[j]), $"The Group type is not as expected.Expected {validNoteTypes[j]} but found {loyaltyNotes.type}");


        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(2)]
        public async Task B_LoyaltyCreateNote_SuccessScenario_CreateNoteUsingMemberNumber()
        {

            #region Test Data

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) order by c._ts desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC");

            FlyingClubMember fcmDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            string memberNumber = fcmDoc.MemberNumber;

            #endregion

            int i = random.Next(validGroupTypes.Length);

            int j = random.Next(validNoteTypes.Length);

            string notes = DataGenerator.RandomString(10);

            string createdBy = DataGenerator.RandomFirstName();

            HttpResponseMessage response = GraphQL.LoyaltyCreateNote(tokenFixture.GetToken("graphqlv2"), "", memberNumber,
                                                    validGroupTypes[i], validNoteTypes[j], notes,
                                                    createdBy, "", null, null, "");

            dynamic loyaltyCreateNoteResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            LoyaltyNote loyaltyNotes = ((JObject)loyaltyCreateNoteResult.data.loyaltyCreateNote).ToObject<LoyaltyNote>();

            Assert.True(loyaltyNotes.text.Equals(notes), $"The Text is not as expected.Expected {notes} but found {loyaltyNotes.text}");
            Assert.True(loyaltyNotes.group.Equals(validGroupTypes[i]), $"The Group type is not as expected.Expected {validGroupTypes[i]} but found {loyaltyNotes.group}");
            Assert.True(loyaltyNotes.type.Equals(validNoteTypes[j]), $"The Group type is not as expected.Expected {validNoteTypes[j]} but found {loyaltyNotes.type}");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(3)]
        public async Task C_LoyaltyCreateNote_MandatoryFieldMissingError_WhenBothCustomerIdandMemberNumberPassedIsNullOrEmpty()
        {

            HttpResponseMessage response = GraphQL.LoyaltyCreateNote(tokenFixture.GetToken("graphqlv2"), "", "",
                                                    "GEN", "GEN", "Welcome to VAA", "testScript", "testScript", null, null, "");

            dynamic loyaltyCreateNoteResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Either 'Customer ID' or 'Member Number' must be specified";

            Assert.True(loyaltyCreateNoteResult.ToString().Contains(expectedResponse), "The Expected Response does not match.");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(4)]
        public async Task D_LoyaltyCreateNote_MandatoryFieldMissingError_WhenGroupTypePassedIsNullOrEmpty()
        {

            HttpResponseMessage response = GraphQL.LoyaltyCreateNote(tokenFixture.GetToken("graphqlv2"), customerId, memberNumber,
                                                    "", "GEN", "Welcome to VAA", "testScript", "testScript", null, null, "");

            dynamic loyaltyCreateNoteResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Field value is missing or invalid.";

            Assert.True(loyaltyCreateNoteResult.ToString().Contains(expectedResponse), "The Expected Response does not match.");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(5)]
        public async Task E_LoyaltyCreateNote_MandatoryFieldMissingError_WhenNotesTypePassedIsNullOrEmpty()
        {

            HttpResponseMessage response = GraphQL.LoyaltyCreateNote(tokenFixture.GetToken("graphqlv2"), customerId, memberNumber,
                                                    "GEN", "", "Welcome to VAA", "testScript", "testScript", null, null, "");

            dynamic loyaltyCreateNoteResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Field value is missing or invalid.";

            Assert.True(loyaltyCreateNoteResult.ToString().Contains(expectedResponse), "The Expected Response does not match.");


        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(6)]
        public async Task F_LoyaltyCreateNote_MandatoryFieldMissingError_WhenTextPassedIsNullOrEmpty()
        {
            HttpResponseMessage response = GraphQL.LoyaltyCreateNote(tokenFixture.GetToken("graphqlv2"), customerId, memberNumber,
                                                    "GEN", "GEN", "", "testScript", "testScript", null, null, "");

            dynamic loyaltyCreateNoteResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Field value is missing.";

            Assert.True(loyaltyCreateNoteResult.ToString().Contains(expectedResponse), "The Expected Response does not match.");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(7)]
        public async Task G_LoyaltyCreateNote_InvalidDateFormatError_WhenCreatedOnDatePassedIsAnEmptyString()
        {

            HttpResponseMessage response = GraphQL.LoyaltyCreateNote(tokenFixture.GetToken("graphqlv2"), customerId, memberNumber,
                                                    "GEN", "GEN", "Write your Note here", "testScript", "testScript", "", null, "");

            dynamic loyaltyCreateNoteResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "DateTime cannot parse the given literal of type `StringValueNode`.";

            Assert.True(loyaltyCreateNoteResult.ToString().Contains(expectedResponse), "The Expected Response does not match.");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(8)]
        public async Task H_LoyaltyCreateNote_InvalidDateFormatError_WhenLastUpdatedOnDatePassedIsAnEmptyString()
        {

            HttpResponseMessage response = GraphQL.LoyaltyCreateNote(tokenFixture.GetToken("graphqlv2"), customerId, memberNumber,
                                                    "GEN", "GEN", "Write your Note here", "testScript", "testScript", null, "", "");

            dynamic loyaltyCreateNoteResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "DateTime cannot parse the given literal of type `StringValueNode`.";

            Assert.True(loyaltyCreateNoteResult.ToString().Contains(expectedResponse), "The Expected Response does not match.");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(9)]
        public async Task I_LoyaltyCreateNote_InvalidFieldValueError_WhenGroupTypeValueIsInvalid()
        {

            HttpResponseMessage response = GraphQL.LoyaltyCreateNote(tokenFixture.GetToken("graphqlv2"), customerId, memberNumber,
                                        "FACT", "GEN", "Write your Note here", "testScript", "testScript", null, null, "");

            dynamic loyaltyCreateNoteResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Field value is missing or invalid.";

            Assert.True(loyaltyCreateNoteResult.ToString().Contains(expectedResponse), "The Expected Response does not match.");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(10)]
        public async Task J_LoyaltyCreateNote_InvalidFieldValueError_WhenNoteTypeValueIsInvalid()
        {

            HttpResponseMessage response = GraphQL.LoyaltyCreateNote(tokenFixture.GetToken("graphqlv2"), customerId, memberNumber,
                                                    "FOOD", "GENE", "Write your Note here", "testScript", "testScript", null, null, "");

            dynamic loyaltyCreateNoteResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Field value is missing or invalid.";

            Assert.True(loyaltyCreateNoteResult.ToString().Contains(expectedResponse), "The Expected Response does not match.");
        }

    }
}
