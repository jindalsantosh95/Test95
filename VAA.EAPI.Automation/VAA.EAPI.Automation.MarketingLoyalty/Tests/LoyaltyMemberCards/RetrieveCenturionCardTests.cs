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

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.LoyaltyMemberCards
{
    [Collection("LoyaltyMemberCardLinkTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class RetrieveCenturionCardTests : IClassFixture<CosmosFixture>, IClassFixture<TokenFixture>
    {
        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;

        public static string env = Config.Environment;

        private readonly string customerTransactionCosmosDb;

        public RetrieveCenturionCardTests(CosmosFixture cosmosFixture, TokenFixture tokenFixture)
        {
            this.cosmosFixture = cosmosFixture;
            this.tokenFixture = tokenFixture;

            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));
            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            this.cosmosFixture.RegisterType<PartnerMembership>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            this.cosmosFixture.RegisterType<MembershipChange>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.tokenFixture.AddTokenForGraphQlFromKeyVault("graphqlv2");
        }

        [Trait("Category", Traits.LoyaltyGraphqlRegression)]
        [Fact, RunOrder(1)]
        public async Task TC01_RetrieveLoyaltyMemberCardLink_SuccessScenario()
        {

            string centurionPartnerId = "1-2LHC";

            string centurionPartnerCode = "ACENT";

            string centurionPartnerName = "Amex Centurion";

            #region PreRequisite to retrieve member with AMEX card details 

            string queryForPartnerMembershipDoc = "SELECT top 1 * FROM CustomerTransaction c where c.DocType = @docType and c.PartnerId = @partnerId order by c.CreatedOn desc ";

            QueryDefinition query = new QueryDefinition(query: queryForPartnerMembershipDoc).WithParameter("@docType", "PartnerMembership").WithParameter("@partnerId", centurionPartnerId);

            PartnerMembership partnerMembership = cosmosFixture.CosmosQueryItem<PartnerMembership>(query);

            Assert.True(partnerMembership is not null, "No Partner Membership document found for test execution.");

            string customerId = partnerMembership.CustomerId;

            string memberNumber = partnerMembership.MemberNumber;

            // retrieve all partnerMembership docs 
            string queryForAllPartnerMembershipDoc = "SELECT * FROM CustomerTransaction c where c.DocType = @docType and c.PartitionKey = @customerId and c.PartnerId = @partnerId";

            QueryDefinition queryForAllDocs = new QueryDefinition(query: queryForAllPartnerMembershipDoc).WithParameter("@docType", "PartnerMembership").WithParameter("@customerId", customerId).WithParameter("@partnerId", centurionPartnerId);

            IEnumerable<PartnerMembership> partnerMembershipDocs = cosmosFixture.CosmosQueryItems<PartnerMembership>(queryForAllDocs).OrderByDescending(x => x.id);

            #endregion

            #region API Call

            HttpResponseMessage response = GraphQL.RetrieveLoyaltyMemberCardLink(tokenFixture.GetToken("graphqlv2"), customerId, memberNumber).EnsureIsSuccessStatusCode();

            dynamic result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(!result?.ToString().Contains("Request failed"), $"Expected a valid response for retrieve award audit graphql operation but found {result.ToString()}");

            LoyaltyMemberCardLinkResponse[] loyaltyMemberCardLinkResponse = JsonConvert.DeserializeObject<LoyaltyMemberCardLinkResponse[]>(result?.data?.loyaltyMemberCardLink?.ToString());

            LoyaltyMemberCardLinkResponse[] loyaltyMemberCardLinkResponses = loyaltyMemberCardLinkResponse.Where(c => c.partnerCode == centurionPartnerCode).OrderByDescending(x => x.documentId).ToArray();

            #endregion

            #region Validations

            Assert.True(partnerMembershipDocs.Count() == loyaltyMemberCardLinkResponses.Length, $"The No of Documents in the response does not match with that of cosmos documents. Cosmos Documents count for partnerId {centurionPartnerId} is {partnerMembershipDocs.Count()} and doc count in the response is {loyaltyMemberCardLinkResponses.Length} ");

            SoftAssertHelper softAssert = new();

            int i = 0;

            foreach (PartnerMembership partnerMembershipDoc in partnerMembershipDocs)
            {
                softAssert.True(partnerMembershipDoc.id == loyaltyMemberCardLinkResponses[i].documentId, $"Expected the partnerMembershipDoc Id to be {partnerMembershipDoc.id} but found {loyaltyMemberCardLinkResponses[i].documentId} ");

                softAssert.True(loyaltyMemberCardLinkResponses[i].partnerName == centurionPartnerName, $"For the Document Id {partnerMembershipDoc.id} of the Customer {customerId} , Expected the Partner name as {centurionPartnerName} but found {loyaltyMemberCardLinkResponses[i].partnerName} ");

                softAssert.True(partnerMembershipDoc.PartnerStatusCode == loyaltyMemberCardLinkResponses[i].cardType, $"For the Document Id {partnerMembershipDoc.id} of the Customer {customerId} , Expected the partnerMembershipDoc PartnerStatusCode to be {partnerMembershipDoc.PartnerStatusCode} but found {loyaltyMemberCardLinkResponses[i].cardType} ");

                string startDateFromDoc = partnerMembershipDoc.StartDate?.ToString("yyyy-MM-dd");

                softAssert.True(startDateFromDoc == loyaltyMemberCardLinkResponses[i].linkedDate?.ToString("yyyy-MM-dd"), $"For the Document Id {partnerMembershipDoc.id} of the Customer {customerId} ,Expected the partnerMembershipDoc LinkedDate to be {startDateFromDoc} but found {loyaltyMemberCardLinkResponses[i].linkedDate?.ToString()}");

                string endDateFromDoc = partnerMembershipDoc.EndDate?.ToString("yyyy-MM-dd");

                softAssert.True(endDateFromDoc == loyaltyMemberCardLinkResponses[i].endDate?.ToString("yyyy-MM-dd"), $"For the Document Id {partnerMembershipDoc.id} of the Customer {customerId} ,Expected the partnerMembershipDoc EndDate  to be {endDateFromDoc} but found {loyaltyMemberCardLinkResponses[i].endDate?.ToString("yyyy-MM-dd")}");

                softAssert.True(partnerMembershipDoc.PartnerId == loyaltyMemberCardLinkResponses[i].pan, $"For the Document Id {partnerMembershipDoc.id} of the Customer {customerId} ,Expected the partnerMembershipDoc partner id to be {partnerMembershipDoc.PartnerId} but found {loyaltyMemberCardLinkResponses[i].pan} ");

                i++;
            }

            softAssert.AssertAll();

            #endregion
        }
    }
}
