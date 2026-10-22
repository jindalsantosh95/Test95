using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.Microservices.Customer
{

    public class RetrieveContactHistoryTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {
        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;
        public string env = Config.Environment;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        private static string ocpSubKey = string.Empty;

        public RetrieveContactHistoryTests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
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
            this.cosmosFixture.RegisterType<ContactDetails>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
            this.cosmosFixture.RegisterType<ContactHistory>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);

            ocpSubKey = Vault.Default.GetSecretValue("TestingCustomerMSSubscriptionKey");

        }

        [Trait("Category", Traits.CustomerMSRegression)]
        public async Task TC01_ValidateContactAuditHistory__SuccessScenario()
        {

            #region Test Data

            string queryForTestData = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType order by c._ts desc ";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForTestData).WithParameter("@docType", "ContactHistory");

            ContactHistory contactHistory = cosmosFixture.CosmosQueryItem<ContactHistory>(parameterizedQuery);

            Assert.True(contactHistory is not null, "Failed to retrieve the Test Data");

            string customerId = contactHistory.CustomerId;

            string queryForContactHistoryDoc = "SELECT * FROM CustomerCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey ";

            QueryDefinition query = new QueryDefinition(query: queryForContactHistoryDoc).WithParameter("@docType", "ContactHistory").WithParameter("@partitionKey", customerId);

            IEnumerable<ContactHistory> contactHistoryDocs = cosmosFixture.CosmosQueryItems<ContactHistory>(query);

            Assert.True(contactHistoryDocs.Any(), "Failed to retrieve the Contact History Docs");

            //Contact History Documents  - Email docs
            ContactHistory[] emailHistoryDocs = contactHistoryDocs.Where(x => x.ContactType.Equals("Email")).OrderByDescending(x => x.LastUpdatedOn).ToArray();

            //Contact History Documents  - Phone docs
            ContactHistory[] phoneHistoryDocs = contactHistoryDocs.Where(x => x.ContactType.Equals("Phone")).OrderByDescending(x => x.LastUpdatedOn).ToArray();


            #endregion

            #region API Call

            HttpResponseMessage response = Microservice.RetrieveContactHistory(tokenFixture.GetToken("customer"), ocpSubKey, customerId).EnsureIsSuccessStatusCode();

            dynamic result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            IEnumerable<ContactHistory> contactHistories = JsonConvert.DeserializeObject<IEnumerable<ContactHistory>>(result.contactHistoryAudit.ToString());

            Assert.True(contactHistories.Any(), $"No Contact History Documents Found for the CustomerId:{customerId}");

            IEnumerable<ContactHistory> emailHistories = contactHistories.Where(x => x.ContactType.Equals("Email")).OrderByDescending(x => x.LastUpdatedOn);

            IEnumerable<ContactHistory> phoneHistories = contactHistories.Where(x => x.ContactType.Equals("Phone")).OrderByDescending(x => x.LastUpdatedOn);

            Assert.True(contactHistories.Count() == emailHistoryDocs.Count() + phoneHistoryDocs.Count(),
                $"Expected the total docs in the response to be {emailHistoryDocs.Count() + phoneHistoryDocs.Count()} but found {contactHistories.Count()}");

            #endregion

            #region Validations

            int i = 0;

            foreach (ContactHistory emailHistory in emailHistories) // Validation of Email history documents
            {

                Assert.True(emailHistoryDocs[i].ContactSubType == emailHistory.ContactSubType,
                     $"For the Contact History doc Id : {emailHistoryDocs[i].id} ,Expected the Email Subtype to be {emailHistoryDocs[i].ContactSubType} but found {emailHistory.ContactSubType} in the response");

                Assert.True(emailHistoryDocs[i].ContactValue == emailHistory.ContactValue,
                    $"For the Contact History doc Id : {emailHistoryDocs[i].id} ,Expected the Email Value to be {emailHistoryDocs[i].ContactValue} but found {emailHistory.ContactValue} in the response");

                i++;
            }


            int j = 0;

            foreach (ContactHistory phoneHistory in phoneHistories) // Validation of Phone history documents
            {

                Assert.True(phoneHistoryDocs[j].ContactSubType == phoneHistory.ContactSubType,
                     $"For the Contact History doc Id : {phoneHistoryDocs[j].id} ,Expected the Phone Subtype to be {phoneHistoryDocs[j].ContactSubType} but found {phoneHistory.ContactSubType} in the response");

                Assert.True(phoneHistoryDocs[j].ContactValue == phoneHistory.ContactValue,
                    $"For the Contact History doc Id : {phoneHistoryDocs[j].id} ,Expected the Phone Value to be {phoneHistoryDocs[j].ContactValue} but found {phoneHistory.ContactValue} in the response");

                j++;
            }


            #endregion

        }
    }
}
