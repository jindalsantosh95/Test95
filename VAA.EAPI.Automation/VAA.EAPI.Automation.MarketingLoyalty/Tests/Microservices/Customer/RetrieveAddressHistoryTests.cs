using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;
using Address = VAA.EAPI.Automation.Core.DB_Model.Entities.Address;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.Microservices.Customer
{

    [Collection("RetrieveAddressHistoryTests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class RetrieveAddressHistoryTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {
        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;
        public string env = Config.Environment;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        private static string ocpSubKey = string.Empty;

        public RetrieveAddressHistoryTests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {

            this.tokenFixture = tokenFixture;
            this.cosmosFixture = cosmosFixture;

            this.tokenFixture.AddTokenForCustomerMSFromKeyVault("customer");

            string cosmosEndpoint = Application.CustomerCosmosDbEndpoint;
            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            this.cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));
            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            this.cosmosFixture.RegisterType<Core.DB_Model.Entities.Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
            this.cosmosFixture.RegisterType<Address>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
            this.cosmosFixture.RegisterType<AddressHistory>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

            ocpSubKey = Vault.Default.GetSecretValue("TestingCustomerMSSubscriptionKey");

        }

        [Trait("Category", Traits.CustomerMSRegression)]
        [Fact, RunOrder(1)]
        public async Task TC01_ValidateAddressAuditHistory_SuccessScenario()
        {
            #region Test Data

            string queryForTestData = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType order by c._ts desc ";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForTestData).WithParameter("@docType", "AddressHistory");

            AddressHistory addressHistoryDoc = cosmosFixture.CosmosQueryItem<AddressHistory>(parameterizedQuery);

            Assert.True(addressHistoryDoc is not null, "Failed to retrieve the Test Data");

            string customerId = addressHistoryDoc.CustomerId;

            string queryForAddressHistoryDoc = "SELECT * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey ";

            QueryDefinition query = new QueryDefinition(query: queryForAddressHistoryDoc).WithParameter("@docType", "AddressHistory").WithParameter("@partitionKey", customerId);

            //Address History Documents 
            AddressHistory[] addressHistoryDocs = cosmosFixture.CosmosQueryItems<AddressHistory>(query).OrderByDescending(x => x.LastUpdatedOn).ToArray();

            #endregion

            #region API Call

            HttpResponseMessage response = Microservice.RetrieveAddressHistory(tokenFixture.GetToken("customer"), ocpSubKey, customerId).EnsureIsSuccessStatusCode();

            IEnumerable<AddressHistory> addressHistories = JsonConvert.DeserializeObject<IEnumerable<AddressHistory>>(await response.Content.ReadAsStringAsync());

            addressHistories = addressHistories.OrderByDescending(x => x.LastUpdatedOn);

            Assert.True(addressHistories.Count() == addressHistoryDocs.Count(),
                $"Expected the total docs in the response to be {addressHistoryDocs.Count()} but found {addressHistories.Count()}");

            #endregion

            #region Validation            

            int i = 0;

            foreach (AddressHistory addressHistory in addressHistories) // Validation of only history documents
            {

                Assert.True(addressHistoryDocs[i].AddressType == addressHistory.AddressType,
                     $"For the Contact History doc Id : {addressHistoryDocs[i].id},Expected the AddressLine1 to be {addressHistoryDocs[i].AddressType} but found {addressHistory.AddressType} in the response");

                Assert.True(addressHistoryDocs[i].AddressLine1 == addressHistory.AddressLine1,
                    $"For the Contact History doc Id : {addressHistoryDocs[i].id},Expected the AddressLine1 to be {addressHistoryDocs[i].AddressLine1} but found {addressHistory.AddressLine1} in the response");

                Assert.True(addressHistoryDocs[i].AddressLine2 == addressHistory.AddressLine2,
                    $"For the Contact History doc Id : {addressHistoryDocs[i].id},Expected the AddressLine2 to be {addressHistoryDocs[i].AddressLine2} but found {addressHistory.AddressLine2} in the response");

                Assert.True(addressHistoryDocs[i].AddressLine3 == addressHistory.AddressLine3,
                    $"For the Contact History doc Id : {addressHistoryDocs[i].id},Expected the AddressLine3 to be {addressHistoryDocs[i].AddressLine3} but found {addressHistory.AddressLine3} in the response");

                Assert.True(addressHistoryDocs[i].City.Equals(addressHistory.City),
                   $"For the Contact History doc Id : {addressHistoryDocs[i].id},Expected the Address City to be {addressHistoryDocs[i].City} but found {addressHistory.City} in the response");

                Assert.True(addressHistoryDocs[i].Country.Equals(addressHistory.Country),
                   $"For the Contact History doc Id : {addressHistoryDocs[i].id},Expected the Address Country to be {addressHistoryDocs[i].Country} but found {addressHistory.Country} in the response");

                Assert.True(addressHistoryDocs[i].PostCode.Equals(addressHistory.PostCode),
                   $"For the Contact History doc Id : {addressHistoryDocs[i].id},Expected the Address PostCode to be {addressHistoryDocs[i].Country} but found {addressHistory.PostCode} in the response");

                i++;
            }

            #endregion

        }
    }
}
