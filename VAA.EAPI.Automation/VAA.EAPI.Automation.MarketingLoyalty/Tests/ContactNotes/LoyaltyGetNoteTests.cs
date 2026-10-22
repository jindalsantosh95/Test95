using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Helpers;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;


namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.ContactNotes
{
    [Collection("LoyaltyNotesTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]

    public class LoyaltyGetNoteTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {

        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;

        public static string env = Config.Environment;

        private PreRequisiteTestData preRequisiteTestData;

        private string memberNumber = "";
        private string customerId = "";


        private readonly string customerTransactionCosmosDb;

        public LoyaltyGetNoteTests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {
            this.tokenFixture = tokenFixture;

            this.tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");

            this.cosmosFixture = cosmosFixture;

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<ContactNote>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            preRequisiteTestData = new PreRequisiteTestData();

            memberNumber = preRequisiteTestData.GetValidFCNumber(out customerId);
        }


        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(14)]
        public async Task A_LoyaltyGetNote_SuccessScenario_GetNotesUsingCustomerId()
        {

            #region Test Data

            string queryForContactNoteDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForContactNoteDoc).WithParameter("@docType", "ContactNote");

            ContactNote contactNoteDoc = cosmosFixture.CosmosQueryItem<ContactNote>(parameterizedQuery);

            string customerId = contactNoteDoc.CustomerId;

            #endregion

            HttpResponseMessage response = GraphQL.LoyaltyGetNote(tokenFixture.GetToken("graphqlv2"), customerId, "");

            dynamic loyaltyGetNoteResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            IEnumerable<LoyaltyNote> contactNotes = JsonConvert.DeserializeObject<IEnumerable<LoyaltyNote>>(loyaltyGetNoteResult?.data?.loyaltyContactNotes.ToString());

            LoyaltyNote[] memberNotes = contactNotes.OrderByDescending(x => x.id).ToArray();

            Assert.True(memberNotes.Any(),
                "The Response is not as expected. Expected data but found null or empty");

            #region Validations

            string queryForContactNoteDocs = "SELECT * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @customerId";

            QueryDefinition query = new QueryDefinition(query: queryForContactNoteDocs).WithParameter("@docType", "ContactNote").WithParameter("@customerId", customerId);

            IEnumerable<ContactNote> contactNoteDocs = cosmosFixture.CosmosQueryItems<ContactNote>(query).OrderByDescending(c => c.id);

            Assert.True(memberNotes.Length == contactNoteDocs.Count(),
                $"The Contact Notes doc count does not match with the response doc count. Expected {contactNoteDocs.Count()} but found {memberNotes.Length}");

            int i = 0;

            foreach (ContactNote note in contactNoteDocs)
            {

                Assert.True(note.Description.Equals(memberNotes[i].text),
                    $"The Text is not as expected.Expected {note.Description} but found {memberNotes[i].text}");

                Assert.True(note.NoteTypeGroup.Equals(memberNotes[i].group),
                    $"The Group type is not as expected.Expected {note.NoteTypeGroup} but found {memberNotes[i].group}");

                Assert.True(note.NoteType.Equals(memberNotes[i].type),
                    $"The Group type is not as expected.Expected {note.NoteType} but found {memberNotes[i].type}");

                i++;
            }

            #endregion
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(15)]
        public async Task B_LoyaltyGetNote_SuccessScenario_GetNotesUsingMemberNumber()
        {

            #region Test Data

            string queryForContactNoteDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType order by c.LastUpdatedOn desc";

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

            LoyaltyNote[] memberNotes = JsonConvert.DeserializeObject<LoyaltyNote[]>(loyaltyGetNoteResult.data.loyaltyContactNotes.ToString());

            Assert.True(memberNotes.Any(), "The Response is not as expected. Expected data but found null or empty");

            #region Validations

            string queryForContactNoteDocs = "SELECT * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @customerId";

            QueryDefinition query = new QueryDefinition(query: queryForContactNoteDocs).WithParameter("@docType", "ContactNote").WithParameter("@customerId", customerId);

            IEnumerable<ContactNote> contactNoteDocs = cosmosFixture.CosmosQueryItems<ContactNote>(query).OrderByDescending(c => c.LastUpdatedOn);

            Assert.True(memberNotes.Length == contactNoteDocs.Count(), $"The Contact Notes doc count does not match with the response doc count. Expected {contactNoteDocs.Count()} but found {memberNotes.Length}");

            int i = 0;

            foreach (ContactNote note in contactNoteDocs)
            {

                Assert.True(note.Description.Equals(memberNotes[i].text),
                    $"The Text is not as expected.Expected {note.Description} but found {memberNotes[i].text}");

                Assert.True(note.NoteTypeGroup.Equals(memberNotes[i].group),
                    $"The Group type is not as expected.Expected {note.NoteTypeGroup} but found {memberNotes[i].group}");

                Assert.True(note.NoteType.Equals(memberNotes[i].type),
                    $"The Group type is not as expected.Expected {note.NoteType} but found {memberNotes[i].type}");

                i++;
            }

            #endregion
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(16)]
        public async Task C_LoyaltyGetNote_MandatoryFieldMissingError_WhenBothCustomerIdAndMemberNumberAreNullOrEmpty()
        {

            HttpResponseMessage response = GraphQL.LoyaltyGetNote(tokenFixture.GetToken("graphqlv2"), "", "");

            dynamic loyaltyGetNoteResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Either 'customerId' or memberId must be spefied.";

            Assert.True(loyaltyGetNoteResult.ToString().Contains(expectedResponse), "The Expected Response does not match.");

        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(17)]
        public async Task D_LoyaltyGetNote_InvalidFieldValueError_WhenMemberNumberIsInvalid()
        {

            HttpResponseMessage response = GraphQL.LoyaltyGetNote(tokenFixture.GetToken("graphqlv2"), "", "1111125552");

            dynamic loyaltyGetNoteResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string expectedResponse = "Data is unavailable from the source.";

            Assert.True(loyaltyGetNoteResult.ToString().Contains(expectedResponse), "The Expected Response does not match.");

        }
    }
}
