using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.Microservices.Customer
{
    public class VACEnrolmentTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {
        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;
        public string env = Config.Environment;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        private static string ocpSubKey = string.Empty;

        public VACEnrolmentTests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
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

            ocpSubKey = Vault.Default.GetSecretValue("TestingCustomerMSSubscriptionKey");

        }

        [Trait("Category", Traits.CustomerMSRegression)]
        public async Task TC01_VACEnrolment_SuccessScenario()
        {

            #region Test Data

            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string middleName = DataGenerator.RandomFirstName();
            string phoneNumber = DataGenerator.RandomNumber(777777, 888888888);
            string customerType = "VirginAccount";
            string doB = DateTime.Now.AddYears(-25).ToString("yyyy-MM-dd");
            string emailId = firstName.ToLower() + lastName.ToLower() + "@noreply.com";

            #endregion

            #region API Call

            HttpResponseMessage response = Microservice.CreateVAEnrolment(tokenFixture.GetToken("customer"), ocpSubKey, firstName, lastName, middleName, phoneNumber, customerType, doB, emailId).EnsureIsAcceptedStatusCode();

            string responseUrl = response?.Headers?.Location?.ToString();

            HttpResponseMessage actualVACEnrolmentResponse = Microservice.GetVAEnrolment(tokenFixture.GetToken("customer"), ocpSubKey, responseUrl).EnsureIsSuccessStatusCode();

            string content = await actualVACEnrolmentResponse?.Content?.ReadAsStringAsync();

            dynamic vacEnrolmentResponse = JsonConvert.DeserializeObject<dynamic>(content);

            Assert.True(!vacEnrolmentResponse.ToString().Contains("Request failed"), $"Expected a valid response for VAC Enrolment operation but found {vacEnrolmentResponse.ToString()}");

            Assert.True(vacEnrolmentResponse.ToString().Contains("Active"), $"Expected a valid success response for VAC Enrolment with member status as Active {vacEnrolmentResponse.ToString()}");

            string customerId = vacEnrolmentResponse?.value?.customerId;

            Assert.True(!customerId.IsNullOrEmpty(), $"Expected a valid customerId but found {customerId.ToString()}");

            #endregion

            #region Validations for Customer Document in Cosmos DB
            string cosmosQuery = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.CustomerId = @customerId order by c._ts desc ";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: cosmosQuery).WithParameter("@docType", "Customer").WithParameter("@customerId", customerId);

            Core.DB_Model.Entities.Customer customerDocument = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(parameterizedQuery);

            int retry = 5;

            while (!(customerDocument is not null) && retry > 0)
            {
                await Task.Delay(1000);

                customerDocument = cosmosFixture.CosmosQueryItem<Core.DB_Model.Entities.Customer>(parameterizedQuery);

                retry--;
            }

            Assert.True(customerDocument is not null, "Failed to retrieve the Test Data");

            Assert.True(customerDocument.CustomerId.Equals(customerId), $"Expected a valid customerId but found {customerId.ToString()}");

            Assert.True(customerDocument.CreatedBy.Equals("WEB"), $"Expected a valid CreatedBy as WEB but found {customerDocument.CreatedBy.ToString()}");

            Assert.True(customerDocument.FirstName.Equals(firstName), $"Expected a valid FirstName but found {customerDocument.FirstName.ToString()}");

            Assert.True(customerDocument.LastName.Equals(lastName), $"Expected a valid LastName but found {customerDocument.LastName.ToString()}");

            string actualPhoneNumber = customerDocument.ContactDetails?.FirstOrDefault(c => c.ContactType == "Phone")?.ContactValue;

            Assert.True(actualPhoneNumber.Equals(phoneNumber), $"Expected a valid PhoneNumber but found {actualPhoneNumber.ToString()}");

            string actualEmailId = customerDocument.ContactDetails?.FirstOrDefault(c => c.ContactType == "Email")?.ContactValue;

            Assert.True(actualEmailId.Equals(emailId), $"Expected a valid EmailId but found {actualEmailId.ToString()}");

            #endregion

            #region Validations for Flying Club Member  Document in Cosmos DB

            string cosmosQueryForFCDocument = "SELECT TOP 1 * FROM CustomerCollection c where c.DocType = @docType and c.CustomerId = @customerId order by c._ts desc ";

            QueryDefinition parameterizedFCDocumentQuery = new QueryDefinition(query: cosmosQueryForFCDocument).WithParameter("@docType", "FlyingClubMember").WithParameter("@customerId", customerId);

            FlyingClubMember flyingClubMemberDocument = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedFCDocumentQuery);

            Assert.True(flyingClubMemberDocument is not null, "Failed to retrieve the Flying Club member document");

            Assert.True(flyingClubMemberDocument.CustomerId.Equals(customerId), $"Expected a valid customerId but found {customerId.ToString()}");

            #endregion

        }
    }
}
