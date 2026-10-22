using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MyAccount.Constants;
using VAA.EAPI.Automation.MyAccount.Requests.Endpoint;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MyAccount.Tests.MicroserviceTests.AutomatchingCustomerMS
{
    public class AutomatchingCustomerMS : IClassFixture<CosmosFixture>, IClassFixture<TokenFixture>
    {
        private readonly CosmosFixture cosmosFixture;
        private readonly ITestOutputHelper output;
        private readonly TokenFixture tokenFixture;
        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        public static string env = Config.Environment;
        private static string ocpSubKey = string.Empty;

        public AutomatchingCustomerMS(ITestOutputHelper output, CosmosFixture cosmosFixture, TokenFixture tokenFixture)
        {
            this.output = output;
            this.cosmosFixture = cosmosFixture;
            this.tokenFixture = tokenFixture;

            string cosmosEndpoint = "https://avadeuncmdbcustomer.documents.azure.com:443/";
            string cosmosEndpointForCustomerTrx = "https://avadeuncmdbcustomertransactions.documents.azure.com:443/";

            this.cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));
            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            this.cosmosFixture.RegisterType<CustomerSearch>(cosmosEndpoint, customerCosmosDb, Application.CustomerSearchCosmosCollection);
            this.cosmosFixture.RegisterType<Customer>(cosmosEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
            this.cosmosFixture.RegisterType<CustomerProfile>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            this.cosmosFixture.RegisterType<MembershipChange>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            this.cosmosFixture.RegisterType<PartnerMembership>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.tokenFixture.AddTokenForGraphQlFromKeyVault("graphql");
            this.tokenFixture.AddTokenForVRedFromKeyVault("vred");
            this.tokenFixture.AddTokenForAzureB2CFromKeyVault("azureb2c");
            this.tokenFixture.AddTokenForDynamicsEnrolmentFromKeyVault("enrolment");
            this.tokenFixture.AddTokenForVirginPointsFromKeyVault("virginPoints");
            this.tokenFixture.AddTokenForCustomerMSFromKeyVault("customer");

            ocpSubKey = Vault.Default.GetSecretValue("TestingCustomerMSSubscriptionKey");
        }


        [Fact]
        [Trait("Category", "Automatching-Regression")]

        public async Task TC01_Validate_AutomatchingCustomerMS_email()
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "+@example.com";
            string postCode = "SO14 0LT";
            string country = "GB";
            string channelID = "WEB-MYA";


            System.Net.Http.HttpResponseMessage enrolmentResponse = Requests.Endpoint.Microservice.Enrolment(env, tokenFixture.GetToken("enrolment"), channelID, firstName, lastName, emailId, postCode, country, output);

            Assert.True(enrolmentResponse.StatusCode == HttpStatusCode.Accepted, $"202 Accepted was not returned from enrolment, response was: {enrolmentResponse.ReadResponseAsString()}");

            JObject enrolmentResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            output.WriteLine($"Response from enrolment facade {Environment.NewLine} {enrolmentResult}");

            Assert.True(enrolmentResult["customerId"] != null && !enrolmentResult["customerId"].ToString().Equals(string.Empty), "customer id should be not blank");
            string loyaltyNumber = enrolmentResult["memberdetails"]["memberNumber"].ToString();
            string strInputQueryForCustomerSearch = $"SELECT * FROM CustomerCollection c WHERE c.PartitionKey = '{enrolmentResult["customerId"]}' and c.DocType='Customer'";

            Customer objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomerSearch);

            RetryHelper.RunWithRetry(() =>
            {
                objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomerSearch);
            }, 3);

            Assert.True(objCustomer.EnrollmentChannel.Equals(channelID), "Channel Id is not same in Cosmos DB");

            //Validate new CustomerMS endpoint for automatching

            System.Net.Http.HttpResponseMessage responseAutomatch = Microservice.AutomatchingCustomerMS(env, tokenFixture.GetToken("customer"), ocpSubKey, emailId, "", "").EnsureIsSuccessStatusCode();

            dynamic automatchMSResult = JsonConvert.DeserializeObject<dynamic>(await responseAutomatch.Content.ReadAsStringAsync());
            dynamic customer = automatchMSResult[0];
            string customerMSLoyaltyNumber = customer.loyaltyMemberNumber;

            Assert.Equal(loyaltyNumber, customerMSLoyaltyNumber);


        }

        [Fact]
        [Trait("Category", "Automatching-Regression")]

        public async Task TC02_Validate_AutomatchingCustomerMS_customerid()
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string postCode = "SO14 0LT";
            string country = "GB";
            string channelID = "WEB-MYA";


            System.Net.Http.HttpResponseMessage enrolmentResponse = Requests.Endpoint.Microservice.Enrolment(env, tokenFixture.GetToken("enrolment"), channelID, firstName, lastName, emailId, postCode, country, output);

            Assert.True(enrolmentResponse.StatusCode == HttpStatusCode.Accepted, $"202 Accepted was not returned from enrolment, response was: {enrolmentResponse.ReadResponseAsString()}");

            JObject enrolmentResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            output.WriteLine($"Response from enrolment facade {Environment.NewLine} {enrolmentResult}");

            Assert.True(enrolmentResult["customerId"] != null && !enrolmentResult["customerId"].ToString().Equals(string.Empty), "customer id should be not blank");
            string customerid = enrolmentResult["customerId"].ToString();
            string strInputQueryForCustomerSearch = $"SELECT * FROM CustomerCollection c WHERE c.PartitionKey = '{enrolmentResult["customerId"]}' and c.DocType='Customer'";

            Customer objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomerSearch);

            RetryHelper.RunWithRetry(() =>
            {
                objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomerSearch);
            }, 3);

            Assert.True(objCustomer.EnrollmentChannel.Equals(channelID), "Channel Id is not same in Cosmos DB");

            //Validate new CustomerMS endpoint for automatching with customerId input

            System.Net.Http.HttpResponseMessage response = Microservice.AutomatchingCustomerMS(env, tokenFixture.GetToken("customer"), ocpSubKey, "", customerid, "").EnsureIsSuccessStatusCode();

            dynamic customerMSresult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
            dynamic customer = customerMSresult[0];
            string customerMSCustomerId = customer.customerId;

            Assert.Equal(customerid, customerMSCustomerId);


        }

        [Fact]
        [Trait("Category", "Automatching-Regression")]

        public async Task TC03_Validate_AutomatchingCustomerMS_fcnumber()
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string postCode = "SO14 0LT";
            string country = "GB";
            string channelID = "WEB-MYA";


            System.Net.Http.HttpResponseMessage enrolmentResponse = Requests.Endpoint.Microservice.Enrolment(env, tokenFixture.GetToken("enrolment"), channelID, firstName, lastName, emailId, postCode, country, output);

            Assert.True(enrolmentResponse.StatusCode == HttpStatusCode.Accepted, $"202 Accepted was not returned from enrolment, response was: {enrolmentResponse.ReadResponseAsString()}");

            JObject enrolmentResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            output.WriteLine($"Response from enrolment facade {Environment.NewLine} {enrolmentResult}");

            Assert.True(enrolmentResult["customerId"] != null && !enrolmentResult["customerId"].ToString().Equals(string.Empty), "customer id should be not blank");
            string loyaltyNumber = enrolmentResult["memberdetails"]["memberNumber"].ToString();
            string strInputQueryForCustomerSearch = $"SELECT * FROM CustomerCollection c WHERE c.PartitionKey = '{enrolmentResult["customerId"]}' and c.DocType='Customer'";

            Customer objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomerSearch);

            RetryHelper.RunWithRetry(() =>
                {
                    objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomerSearch);
                }, 3);

            Assert.True(objCustomer.EnrollmentChannel.Equals(channelID), "Channel Id is not same in Cosmos DB");

            //Validate new CustomerMS endpoint for automatching

            System.Net.Http.HttpResponseMessage response = Microservice.AutomatchingCustomerMS(env, tokenFixture.GetToken("customer"), ocpSubKey, "", "", loyaltyNumber).EnsureIsSuccessStatusCode();

            dynamic customerMSresult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
            dynamic customer = customerMSresult[0];
            string customerMSLoyaltyNumber = customer.loyaltyMemberNumber;

            Assert.Equal(loyaltyNumber, customerMSLoyaltyNumber);


        }


    }
}
