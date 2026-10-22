using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.Microservices.CustomerTransaction
{
    [Collection("RetrieveFlyingClubMemberDocTests")]
    public class RetrieveFlyingClubMemberDocTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {

        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;

        public string env = Config.Environment;

        private readonly string customerTransactionCosmosDb;

        private static string ocpSubKey = string.Empty;

        public RetrieveFlyingClubMemberDocTests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {
            this.tokenFixture = tokenFixture;
            this.cosmosFixture = cosmosFixture;

            this.tokenFixture.AddTokenForCustomerTxnFromKeyVault("customertxn");

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));
            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            ocpSubKey = Vault.Default.GetSecretValue("TestingCustomerTxnMSSubscriptionKey");
        }

        [Trait("Category", Traits.CustomerTransactionMSRegression)]
        [Fact]
        public async Task A_RetrieveFlyingClubMemberDoc_SuccessScenario()
        {

            #region Test Data

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) order by c._ts desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC");

            FlyingClubMember flyingClubMember = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            string customerId = flyingClubMember.CustomerId;

            #endregion

            HttpResponseMessage response = Microservice.RetrieveFlyingClubMemberDoc(tokenFixture.GetToken("customertxn"), ocpSubKey, customerId).EnsureIsSuccessStatusCode();

            dynamic retrieveFCMDocResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(retrieveFCMDocResult.customerId.ToString().Equals(customerId), $"Expected the CustomerId as {customerId}  but found {retrieveFCMDocResult.customerId}");

            dynamic result = retrieveFCMDocResult.flyingClubMemberDocuments[0];

            Assert.True(result.memberNumber.ToString().Equals(flyingClubMember.MemberNumber), $"Expected the Flying Club Member Number as {flyingClubMember.MemberNumber} but found {result.memberNumber.ToString()}");

            Assert.True(result.status.ToString().Equals(flyingClubMember.Status), $"Expected the Flying Club Member Status as {flyingClubMember.Status} but found {result.status.ToString()}");

            Assert.True(result.memberType.ToString().Equals(flyingClubMember.MemberType), $"Expected the Flying Club Member Type as {flyingClubMember.MemberType} but found {result.memberType.ToString()}");

            Assert.True(result.tierCode.ToString().Equals(flyingClubMember.TierCode), $"Expected the Flying Club Member Tier as {flyingClubMember.TierCode} but found {result.tierCode.ToString()}");

            //Validating the MembershipCard Details 
            Assert.True(result.memberShipCardDetails.active.ToString().Equals(flyingClubMember.MemberShipCardDetails.Active), $"Expected the Flying Club Member MembershipCard Status as {flyingClubMember.MemberShipCardDetails.Active} but found {result.memberShipCardDetails.active.ToString()}");

            Assert.True(result.memberShipCardDetails.cardType.ToString().Equals(flyingClubMember.MemberShipCardDetails.CardType), $"Expected the Flying Club Member MembershipCard CardType as {flyingClubMember.MemberShipCardDetails.CardType} but found {result.memberShipCardDetails.cardType.ToString()}");

            Assert.True(result.memberShipCardDetails.tierCode.ToString().Equals(flyingClubMember.MemberShipCardDetails.TierCode), $"Expected the Flying Club Member MembershipCard Tier Code as {flyingClubMember.MemberShipCardDetails.TierCode} but found {result.memberShipCardDetails.tierCode.ToString()}");

            Assert.True(result.memberShipCardDetails.cardStatusCode.ToString().Equals(flyingClubMember.MemberShipCardDetails.CardStatusCode), $"Expected the Flying Club Member MembershipCard Status Code as {flyingClubMember.MemberShipCardDetails.CardStatusCode} but found {result.memberShipCardDetails.cardStatusCode.ToString()}");

            //Validating the Member Attributes Details
            int count = result.memberAttributes.Count;

            for (int i = 0; i < count; i++)
            {
                string attributeName = result.memberAttributes[i].attributeName.ToString();

                string attributeValue = result.memberAttributes[i].value.ToString();

                Assert.True(attributeName.Equals(flyingClubMember.MemberAttributes[i].AttributeName), $"Expected the attributeName as {attributeName} but found {flyingClubMember.MemberAttributes[i].AttributeName} ");

                Assert.True(attributeValue.Equals(flyingClubMember.MemberAttributes[i].Value), $"Expected the attributeValue as {attributeValue} but found {flyingClubMember.MemberAttributes[i].Value} ");
            }
        }
    }
}
