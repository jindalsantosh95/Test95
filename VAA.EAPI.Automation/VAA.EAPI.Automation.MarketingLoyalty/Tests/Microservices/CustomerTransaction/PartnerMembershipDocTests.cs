using System.Text.Json;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.Microservices;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.Microservices.CustomerTransaction
{
    public class PartnerMembershipDocTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {

        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;

        public string env = Config.Environment;

        private readonly string customerTransactionCosmosDb;

        private static string ocpSubKey = string.Empty;

        public PartnerMembershipDocTests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {

            this.tokenFixture = tokenFixture;
            this.cosmosFixture = cosmosFixture;

            this.tokenFixture.AddTokenForCustomerTxnFromKeyVault("customertxn");

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<PartnerMembership>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            ocpSubKey = Vault.Default.GetSecretValue("TestingCustomerTxnMSSubscriptionKey");
        }

        [Trait("Category", Traits.CustomerTransactionMSRegression)]
        [Fact, RunOrder(1)]
        public async Task RetrievePartnerMembershipDoc_SuccessScenario()
        {
            #region Test Data

            string queryForPartnerMembershipDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForPartnerMembershipDoc).WithParameter("@docType", "PartnerMembership");

            PartnerMembership partnerMembershipDoc = cosmosFixture.CosmosQueryItem<PartnerMembership>(parameterizedQuery);

            string customerId = partnerMembershipDoc?.CustomerId;

            string queryForPartnerMembershipDocs = "SELECT * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @customerId order by c.CreatedOn desc ";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForPartnerMembershipDocs).WithParameter("@docType", "PartnerMembership").WithParameter("@customerId", customerId);

            IEnumerable<PartnerMembership> partnerMembershipDocs = cosmosFixture.CosmosQueryItems<PartnerMembership>(paramQuery);

            #endregion

            #region API Call

            HttpResponseMessage response = Microservice.RetrievePartnerMembershipDoc(tokenFixture.GetToken("customertxn"), ocpSubKey, customerId).EnsureIsSuccessStatusCode();

            dynamic retrievePartnerMembershipResponse = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(!retrievePartnerMembershipResponse.ToString().Contains("Request failed"),
                         $"Expected a valid response for retrieve award audit graphql operation but found {retrievePartnerMembershipResponse.ToString()}");

            Assert.True(retrievePartnerMembershipResponse.customerId.ToString().Equals(customerId), $"Expected the CustomerId as {customerId}  but found {retrievePartnerMembershipResponse.customerId}");

            PartnerMembershipDocument[] partnerMembershipApiResponse = JsonConvert.DeserializeObject<PartnerMembershipDocument[]>(retrievePartnerMembershipResponse?.partnerMembershipDocuments.ToString());

            PartnerMembershipDocument[] partnerMembershipApiResponses = partnerMembershipApiResponse.OrderByDescending(x => x.createdOn).ToArray();

            #endregion

            #region - Validations

            int i = 0;

            foreach (PartnerMembership partnerMembershipCosmosDoc in partnerMembershipDocs)
            {


                Assert.True(partnerMembershipApiResponses[i].partnerStatusCode == partnerMembershipCosmosDoc.PartnerStatusCode, $"Expected the Doc PartnerStatusCode Attribute value as {partnerMembershipCosmosDoc.PartnerStatusCode} but found {partnerMembershipApiResponses[i].partnerStatusCode}.");

                Assert.True(partnerMembershipApiResponses[i].id == partnerMembershipCosmosDoc.id, $"Expected the Doc id Attribute value as {partnerMembershipCosmosDoc.id} but found {partnerMembershipApiResponses[i].id}");

                Assert.True(partnerMembershipApiResponses[i].memberNumber == partnerMembershipCosmosDoc.MemberNumber, $"Expected the MemberNumber Attribute value as {partnerMembershipCosmosDoc.MemberNumber} but found {partnerMembershipApiResponses[i].memberNumber}.");

                Assert.True(partnerMembershipApiResponses[i].createdBy == partnerMembershipCosmosDoc.CreatedBy, $"Expected the Doc CreatedBy Attribute value as {partnerMembershipCosmosDoc.CreatedBy} but found {partnerMembershipApiResponses[i].createdBy}.");

                Assert.True(partnerMembershipApiResponses[i].brand == partnerMembershipCosmosDoc.Brand, $"Expected the Doc Brand Attribute value as {partnerMembershipCosmosDoc.Brand} but found {partnerMembershipApiResponses[i].brand}.");

                string createdOnDateFromDoc = partnerMembershipCosmosDoc.CreatedOn?.ToString("yyyy-MM-dd");

                Assert.True(partnerMembershipApiResponses[i].createdOn.ToString("yyyy-MM-dd") == createdOnDateFromDoc, $"Expected the Active Doc CreatedOn Date from cosmos document as {createdOnDateFromDoc} but found {partnerMembershipApiResponses[i].createdOn.ToString("yyyy-MM-dd")}.");

                string lastUpdatedOnDateFromDoc = partnerMembershipCosmosDoc.LastUpdatedOn?.ToString("yyyy-MM-dd");

                Assert.True(partnerMembershipApiResponses[i].lastUpdatedOn.ToString("yyyy-MM-dd") == lastUpdatedOnDateFromDoc, $"Expected the Active Doc CreatedOn Date from cosmos document as {lastUpdatedOnDateFromDoc} but found {partnerMembershipApiResponses[i].lastUpdatedOn.ToString("yyyy-MM-dd")}.");

                i++;

                #endregion

            }

        }


        [Trait("Category", Traits.CustomerTransactionMSRegression)]
        [Theory, RunOrder(2)]
        [InlineData("AMXCUK")]
        [InlineData("AMXIC1")]
        public async Task CreatePartnerMembershipDoc_SuccessScenario(string cardType)
        {
            #region Test Data

            string queryForPartnerMembershipDocs = "SELECT TOP 20 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForPartnerMembershipDocs).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC");

            IEnumerable<FlyingClubMember> flyingClubDoucments = cosmosFixture.CosmosQueryItems<FlyingClubMember>(parameterizedQuery);

            string customerId = string.Empty;

            string memberNumber = string.Empty;

            bool flag = false;

            foreach (FlyingClubMember flyingClubDoucment in flyingClubDoucments)
            {

                customerId = flyingClubDoucment.CustomerId;

                memberNumber = flyingClubDoucment.MemberNumber;

                string queryForAmexCenturionCardExistance = "SELECT * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartnerId = @partnerId and c.PartnerStatusCode = @partnerStatusCode and c.CustomerId = @customerId order by c.CreatedOn desc";

                QueryDefinition parameterizedPartnerMembershipQuery = new QueryDefinition(query: queryForAmexCenturionCardExistance).WithParameter("@docType", "PartnerMembership").WithParameter("@partnerId", "1-2LHC").WithParameter("@partnerStatusCode", cardType).WithParameter("@customerId", customerId);

                IEnumerable<PartnerMembership> partnerMembershipDocs = cosmosFixture.CosmosQueryItems<PartnerMembership>(parameterizedPartnerMembershipQuery);

                if (!partnerMembershipDocs.Any())
                {
                    flag = true;

                    break;
                }
            }

            Assert.True(flag, "Loyalty Members retrived via the cosmos query have an Amex Centurion card linked to it. Expected member with no card assocated was not retrieved ");
            #endregion

            #region API Call

            string startDate = DateTime.UtcNow.AddDays(-10).ToString("yyyy-MM-dd");

            string partnerStatusCode = cardType;

            string partnerId = "1-2LHC";

            HttpResponseMessage createPartnerMembershipResponse = Microservice.CreatePartnerMembershipDoc(tokenFixture.GetToken("customertxn"), ocpSubKey, memberNumber, customerId, startDate, partnerStatusCode, partnerId).EnsureIsAcceptedStatusCode();

            string responseUrl = createPartnerMembershipResponse?.Headers?.Location?.ToString();

            HttpResponseMessage actualCreatePartnerMembershipResponse = Microservice.GetCreatePartnerMembershipDoc(tokenFixture.GetToken("customertxn"), ocpSubKey, responseUrl).EnsureIsSuccessStatusCode();

            string content = await actualCreatePartnerMembershipResponse?.Content?.ReadAsStringAsync();

            string requestId = JsonDocument.Parse(content).RootElement.GetProperty("requestId").GetString();

            bool isPartnerMembershipDocCreated = !requestId.IsNullOrEmpty() && !content.Has("There was an unexpected error");

            Assert.True(isPartnerMembershipDocCreated, $"PartnerMembership Document creation failed for the member {memberNumber}. The response returned is \n {actualCreatePartnerMembershipResponse}");

            string partnerMembershipDocumentSectionInResponse = JsonDocument.Parse(content).RootElement.GetProperty("partnerMembershipDocument").ToString();

            string partnerIdResponse = JsonDocument.Parse(partnerMembershipDocumentSectionInResponse).RootElement.GetProperty("partnerId").ToString();

            string partnerStatusCodeResponse = JsonDocument.Parse(partnerMembershipDocumentSectionInResponse).RootElement.GetProperty("partnerStatusCode").ToString();

            string startDateResponse = JsonDocument.Parse(partnerMembershipDocumentSectionInResponse).RootElement.GetProperty("startDate").ToString();

            string customerIdResponse = JsonDocument.Parse(partnerMembershipDocumentSectionInResponse).RootElement.GetProperty("customerId").ToString();

            #endregion

            #region - Validations

            Assert.True(partnerIdResponse.Equals(partnerId), $"the expected Partner Id is {partnerId} and API response is: {partnerIdResponse}");

            Assert.True(partnerStatusCodeResponse.Equals(partnerStatusCode), $"the expected Partner status code is {partnerStatusCode} and API response is: {partnerStatusCodeResponse}");

            string startDateResponseUpdated = DateTime.Parse(startDateResponse).ToUniversalTime().ToString("yyyy-MM-dd");

            Assert.True(startDateResponseUpdated.Equals(startDate), $"the expected Partner Id is {startDate} and API response is: {startDateResponseUpdated}");

            Assert.True(customerIdResponse.Equals(customerId), $"the expected customerId Id is {customerId} and API response is: {customerIdResponse}");

            #endregion
        }

        [Trait("Category", Traits.CustomerTransactionMSRegression)]
        [Fact, RunOrder(3)]
        public async Task CreatePartnerMembershipDoc_DuplicateCheckScenario()
        {
            #region Test Data

            string queryForPartnerMembershipDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForPartnerMembershipDoc).WithParameter("@docType", "FlyingClubMember");

            FlyingClubMember flyingClubDoucment = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            string customerId = flyingClubDoucment.CustomerId;

            string memberNumber = flyingClubDoucment.MemberNumber;

            #endregion

            #region API Called twice for duplicate check 

            string startDate = DateTime.UtcNow.AddDays(-10).ToString("yyyy-MM-dd");

            string partnerStatusCode = "AMXCUK";

            string partnerId = "1-2LHC";

            HttpResponseMessage createPartnerMembershipResponse = Microservice.CreatePartnerMembershipDoc(tokenFixture.GetToken("customertxn"), ocpSubKey, memberNumber, customerId, startDate, partnerStatusCode, partnerId).EnsureIsAcceptedStatusCode();

            string responseUrl = createPartnerMembershipResponse?.Headers?.Location?.ToString();

            HttpResponseMessage actualCreatePartnerMembershipResponse = Microservice.GetCreatePartnerMembershipDoc(tokenFixture.GetToken("customertxn"), ocpSubKey, responseUrl).EnsureIsSuccessStatusCode();

            string content = await actualCreatePartnerMembershipResponse?.Content?.ReadAsStringAsync();

            string requestId = JsonDocument.Parse(content).RootElement.GetProperty("requestId").GetString();

            bool isPartnerMembershipDocCreated = !requestId.IsNullOrEmpty();

            Assert.True(isPartnerMembershipDocCreated, $"Initial PartnerMembership Document creation failed for the member {memberNumber}. The response returned is \n {actualCreatePartnerMembershipResponse}");

            //duplicate txn creation 

            HttpResponseMessage dupResponse = Microservice.CreatePartnerMembershipDoc(tokenFixture.GetToken("customertxn"), ocpSubKey, memberNumber, customerId, startDate, partnerStatusCode, partnerId).EnsureIsAcceptedStatusCode();

            string dupResponseUrl = dupResponse?.Headers?.Location?.ToString();

            HttpResponseMessage dupCreatePartnerMembershipResponse = Microservice.GetCreatePartnerMembershipDoc(tokenFixture.GetToken("customertxn"), ocpSubKey, dupResponseUrl).EnsureIsSuccessStatusCode();

            dynamic PartnerMembershipDupCheckResponse = JsonConvert.DeserializeObject<dynamic>(await dupCreatePartnerMembershipResponse.Content.ReadAsStringAsync());

            #endregion

            //Duplicate card found validation

            Assert.True(PartnerMembershipDupCheckResponse.errors[0].developerMessage.ToString().Contains("Duplicate card found"), $"the expected developer message is Duplicate card found and API response is: {PartnerMembershipDupCheckResponse.ToString()}");

            Assert.True(PartnerMembershipDupCheckResponse.errors[0].message.ToString().Contains("A member can have only one active card of a particular type at a time"), $"the expected message for member with 1 active card is not present and API response is:{PartnerMembershipDupCheckResponse.ToString()}");

        }
    }

}
