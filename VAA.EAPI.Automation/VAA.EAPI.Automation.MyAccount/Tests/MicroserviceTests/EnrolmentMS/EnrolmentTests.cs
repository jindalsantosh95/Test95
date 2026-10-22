using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MyAccount.Constants;
using Xunit.Abstractions;
using Authorization = VAA.EAPI.Automation.Core.DB_Model.Entities.Authorization;

namespace VAA.EAPI.Automation.MyAccount.Tests.MicroserviceTests.EnrolmentMS
{
    public class EnrolmentTests : IClassFixture<CosmosFixture>, IClassFixture<TokenFixture>
    {
        private readonly CosmosFixture cosmosFixture;
        private readonly ITestOutputHelper output;
        private readonly TokenFixture tokenFixture;
        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        public static string env = Config.Environment;

        public EnrolmentTests(ITestOutputHelper output, CosmosFixture cosmosFixture, TokenFixture tokenFixture)
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
        }

        [Theory]
        [Trait("Category", "Enrolment-Regression")]
        [InlineData("delta.com")]
        [InlineData("powerapp")]
        [InlineData("web-mya")]
        public async Task ValidateChannelIdWhenNotCaps_AllChannels(string channel)
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string postCode = "CV459HP";
            string country = "GB";
            string channelID = channel;

            System.Net.Http.HttpResponseMessage enrolmentResponse = Requests.Endpoint.Microservice.Enrolment(env, tokenFixture.GetToken("enrolment"), channelID, firstName, lastName, emailId, postCode, country, output);
            Assert.True(enrolmentResponse.StatusCode == HttpStatusCode.UnprocessableEntity, $"422 Accepted was not returned from enrolment, returned {enrolmentResponse.ReadResponseAsString()}");

            JArray enrolmentResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult[0]["code"].ToString() == "1015", $"Validtion message returned was not as expected, returned {enrolmentResult[0]["code"].ToString()} and expected 1015");
            Assert.True(enrolmentResult[0]["message"].ToString() == "Invalid Operation Identifier or Channel Name.", $"Validtion message returned was not as expected, returned {enrolmentResult[0]["message"].ToString()} and expected Invalid Operation Identifier or Channel Name.");
        }

        [Theory]
        [Trait("Category", "Enrolment-Regression")]
        [InlineData("DELTA.COM")]
        [InlineData("POWERAPP")]
        [InlineData("WEB-MYA")]
        public async Task ValidateFirstnameBlank_AllChannels(string channel)
        {
            string firstName = "";
            string lastName = DataGenerator.RandomLastName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string postCode = "CV459HP";
            string country = "GB";
            string channelID = channel;

            System.Net.Http.HttpResponseMessage enrolmentResponse = Requests.Endpoint.Microservice.Enrolment(env, tokenFixture.GetToken("enrolment"), channelID, firstName, lastName, emailId, postCode, country, output);
            Assert.True(enrolmentResponse.StatusCode == HttpStatusCode.UnprocessableEntity, $"422 Accepted was not returned from enrolment, returned {enrolmentResponse.ReadResponseAsString()}");

            JArray enrolmentResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult[0]["code"].ToString() == "G012", $"Validtion message returned was not as expected, returned {enrolmentResult[0]["code"].ToString()} and expected G012");
            Assert.True(enrolmentResult[0]["message"].ToString() == "One of the mandatory field is missing in the request.", $"Validtion message returned was not as expected, returned {enrolmentResult[0]["message"].ToString()} and expected One of the mandatory field is missing in the request.");
        }

        [Theory]
        [Trait("Category", "Enrolment-Regression")]
        [InlineData("DELTA.COM")]
        [InlineData("POWERAPP")]
        [InlineData("WEB-MYA")]
        public async Task ValidateCustomerTypeBlank_AllChannels(string channel)
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string postCode = "CV459HP";
            string country = "GB";
            string channelID = channel;

            System.Net.Http.HttpResponseMessage enrolmentResponse = Requests.Endpoint.Microservice.Enrolment(env, tokenFixture.GetToken("enrolment"), channelID, firstName, lastName, emailId, postCode, country, output, customerType: "");
            Assert.True(enrolmentResponse.StatusCode == HttpStatusCode.UnprocessableEntity, $"422 Accepted was not returned from enrolment, returned {enrolmentResponse.ReadResponseAsString()}");

            JArray enrolmentResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult[0]["code"].ToString() == "G012", $"Validtion message returned was not as expected, returned {enrolmentResult[0]["code"].ToString()} and expected G012");
            Assert.True(enrolmentResult[0]["message"].ToString() == "One of the mandatory field is missing in the request.", $"Validtion message returned was not as expected, returned {enrolmentResult[0]["message"].ToString()} and expected One of the mandatory field is missing in the request.");
        }

        [Theory]
        [Trait("Category", "Enrolment-Regression")]
        [InlineData("DELTA.COM")]
        [InlineData("POWERAPP")]
        [InlineData("WEB-MYA")]
        public async Task ValidateLastnameBlank_AllChannels(string channel)
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = "";
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string postCode = "CV459HP";
            string country = "GB";
            string channelID = channel;

            System.Net.Http.HttpResponseMessage enrolmentResponse = Requests.Endpoint.Microservice.Enrolment(env, tokenFixture.GetToken("enrolment"), channelID, firstName, lastName, emailId, postCode, country, output);
            Assert.True(enrolmentResponse.StatusCode == HttpStatusCode.UnprocessableEntity, $"422 Accepted was not returned from enrolment, returned {enrolmentResponse.ReadResponseAsString()}");

            JArray enrolmentResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult[0]["code"].ToString() == "G012", $"Validtion message returned was not as expected, returned {enrolmentResult[0]["code"].ToString()} and expected G012");
            Assert.True(enrolmentResult[0]["message"].ToString() == "One of the mandatory field is missing in the request.", $"Validtion message returned was not as expected, returned {enrolmentResult[0]["message"].ToString()} and expected One of the mandatory field is missing in the request.");
        }

        [Theory]
        [Trait("Category", "Enrolment-Regression")]
        [InlineData("DELTA.COM")]
        [InlineData("POWERAPP")]
        [InlineData("WEB-MYA")]
        public async Task ValidateDateOfBirthBlank_AllChannels(string channel)
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string postCode = "CV459HP";
            string country = "GB";
            string channelID = channel;

            System.Net.Http.HttpResponseMessage enrolmentResponse = Requests.Endpoint.Microservice.Enrolment(env, tokenFixture.GetToken("enrolment"), channelID, firstName, lastName, emailId, postCode, country, output, dateOfBirth: "");
            Assert.True(enrolmentResponse.StatusCode == HttpStatusCode.UnprocessableEntity, $"422 Accepted was not returned from enrolment, returned {enrolmentResponse.ReadResponseAsString()}");

            JArray enrolmentResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult[0]["code"].ToString() == "G012", $"Validtion message returned was not as expected, returned {enrolmentResult[0]["code"].ToString()} and expected G012");
            Assert.True(enrolmentResult[0]["message"].ToString() == "One of the mandatory field is missing in the request.", $"Validtion message returned was not as expected, returned {enrolmentResult[0]["message"].ToString()} and expected One of the mandatory field is missing in the request.");
        }

        //Removed the Snapx channel for which fix is required & story also there #426387
        [Theory]
        [Trait("Category", "Enrolment-Regression")]
        [InlineData("DELTA.COM")]
        [InlineData("POWERAPP")]
        [InlineData("WEB-MYA")]
        public async Task ValidateAgeLessThan12AndGuardianTCsFalse_AllChannels(string channel)
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string postCode = "CV459HP";
            string country = "GB";
            string channelID = channel;
            string under12 = DateTime.Now.AddYears(-5).ToString("yyyy-MM-dd");

            System.Net.Http.HttpResponseMessage enrolmentResponse = Requests.Endpoint.Microservice.Enrolment(env, tokenFixture.GetToken("enrolment"), channelID, firstName, lastName, emailId, postCode, country, output, dateOfBirth: under12, guardianTermsAndConditionsAccepted: false);

            if (channelID == "DELTA.COM" || channelID == "WEB-MYA")
                Assert.True(enrolmentResponse.StatusCode == HttpStatusCode.Accepted, $"202 Accepted was not returned from enrolment, returned {enrolmentResponse.ReadResponseAsString()}");
            else
            {
                Assert.True(enrolmentResponse.StatusCode == HttpStatusCode.UnprocessableEntity, $"422 Accepted was not returned from enrolment, returned {enrolmentResponse.ReadResponseAsString()}");

                JArray enrolmentResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

                Assert.True(enrolmentResult[0]["code"].ToString() == "1027", $"Validtion message returned was not as expected, returned {enrolmentResult[0]["code"].ToString()} and expected 1027");

                Assert.True(enrolmentResult[0]["message"].ToString() == "Either Customer age < 18 and/or parental consent is missing in the request", $"Validtion message returned was not as expected, returned {enrolmentResult[0]["message"].ToString()} and expected Either Customer age < 18 and/or parental consent is missing in the request");
            }
        }

        //Removed the Snapx channel for which fix is required & story also there #426387
        [Theory]
        [Trait("Category", "Enrolment-Regression")]
        [InlineData("DELTA.COM")]
        [InlineData("POWERAPP")]
        [InlineData("WEB-MYA")]
        public async Task ValidateAgeLessThan12AndGuardianTCsTrue_AllChannels(string channel)
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string postCode = "CV459HP";
            string country = "GB";
            string channelID = channel;
            string under12 = DateTime.Now.AddYears(-5).ToString("yyyy-MM-dd");

            System.Net.Http.HttpResponseMessage enrolmentResponse = Requests.Endpoint.Microservice.Enrolment(env, tokenFixture.GetToken("enrolment"), channelID, firstName, lastName, emailId, postCode, country, output, dateOfBirth: under12, guardianTermsAndConditionsAccepted: true);

            if (channelID == "DELTA.COM" || channelID == "POWERAPP" || channelID == "WEB-MYA")
            {
                Assert.True(enrolmentResponse.StatusCode == HttpStatusCode.Accepted, $"202 Accepted was not returned from enrolment, returned {enrolmentResponse.ReadResponseAsString()}");

                JObject enrolmentResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

                Assert.True(enrolmentResult["customerId"] != null && !enrolmentResult["customerId"].ToString().Equals(string.Empty), "customer id should be not blank");
            }
            else
            {
                Assert.True(enrolmentResponse.StatusCode == HttpStatusCode.UnprocessableEntity, $"422 Accepted was not returned from enrolment, returned {enrolmentResponse.ReadResponseAsString()}");

                JArray enrolmentResultValidation = (JArray)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

                Assert.True(enrolmentResultValidation[0]["code"].ToString() == "1015", $"Validtion message returned was not as expected, returned {enrolmentResultValidation[0]["code"].ToString()} and expected 1027");

                Assert.True(enrolmentResultValidation[0]["message"].ToString() == "Either Customer age < 18 and/or parental consent is missing in the request", $"Validtion message returned was not as expected, returned {enrolmentResultValidation[0]["message"].ToString()} and expected Either Customer age < 18 and/or parental consent is missing in the request");
            }
        }

        [Theory]
        [Trait("Category", "Enrolment-Regression")]
        [InlineData("DELTA.COM")]
        [InlineData("POWERAPP")]
        [InlineData("WEB-MYA")]
        public async Task ValidateBlankCity_AllChannels(string channel)
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string postCode = "CV459HP";
            string country = "GB";
            string channelID = channel;

            System.Net.Http.HttpResponseMessage enrolmentResponse = Requests.Endpoint.Microservice.Enrolment(env, tokenFixture.GetToken("enrolment"), channelID, firstName, lastName, emailId, postCode, country, output, city: "");
            Assert.True(enrolmentResponse.StatusCode == HttpStatusCode.UnprocessableEntity, $"422 Accepted was not returned from enrolment, returned {enrolmentResponse.ReadResponseAsString()}");

            JArray enrolmentResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult[0]["code"].ToString() == "G012", $"Validtion message returned was not as expected, returned {enrolmentResult[0]["code"].ToString()} and expected G012");
            Assert.True(enrolmentResult[0]["message"].ToString() == "One of the mandatory field is missing in the request.", $"Validtion message returned was not as expected, returned {enrolmentResult[0]["message"].ToString()} and expected One of the mandatory field is missing in the request.");
        }

        //Removed the Snapx channel for which fix is required & story also there #426387
        [Theory]
        [Trait("Category", "Enrolment-Regression")]
        [InlineData("DELTA.COM")]
        [InlineData("POWERAPP")]
        [InlineData("WEB-MYA")]
        public void ValidateBlankAddressVerified_AllChannels(string channel)
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string postCode = "CV459HP";
            string country = "GB";
            string channelID = channel;

            System.Net.Http.HttpResponseMessage enrolmentResponse = Requests.Endpoint.Microservice.Enrolment(env, tokenFixture.GetToken("enrolment"), channelID, firstName, lastName, emailId, postCode, country, output, addressVerified: "");

            Assert.True(enrolmentResponse.StatusCode == HttpStatusCode.Accepted, $"202 Accepted was not returned from enrolment, returned {enrolmentResponse.ReadResponseAsString()}");
        }

        [Theory]
        [Trait("Category", "Enrolment-Regression")]
        [InlineData("DELTA.COM")]
        [InlineData("POWERAPP")]
        [InlineData("WEB-MYA")]
        public async Task ValidateBlankAddressLine1_AllChannels(string channel)
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string postCode = "CV459HP";
            string country = "GB";
            string channelID = channel;

            System.Net.Http.HttpResponseMessage enrolmentResponse = Requests.Endpoint.Microservice.Enrolment(env, tokenFixture.GetToken("enrolment"), channelID, firstName, lastName, emailId, postCode, country, output, addressLine1: "");
            Assert.True(enrolmentResponse.StatusCode == HttpStatusCode.UnprocessableEntity, $"422 Accepted was not returned from enrolment, returned {enrolmentResponse.ReadResponseAsString()}");

            JArray enrolmentResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult[0]["code"].ToString() == "G012", $"Validtion message returned was not as expected, returned {enrolmentResult[0]["code"].ToString()} and expected G012");
            Assert.True(enrolmentResult[0]["message"].ToString() == "One of the mandatory field is missing in the request.", $"Validtion message returned was not as expected, returned {enrolmentResult[0]["message"].ToString()} and expected One of the mandatory field is missing in the request.");
        }

        [Theory]
        [Trait("Category", "Enrolment-Regression")]
        [InlineData("DELTA.COM")]
        [InlineData("POWERAPP")]
        [InlineData("WEB-MYA")]
        public async Task ValidateBlankAddressType_AllChannels(string channel)
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string postCode = "CV459HP";
            string country = "GB";
            string channelID = channel;

            System.Net.Http.HttpResponseMessage enrolmentResponse = Requests.Endpoint.Microservice.Enrolment(env, tokenFixture.GetToken("enrolment"), channelID, firstName, lastName, emailId, postCode, country, output, addressType: "");
            Assert.True(enrolmentResponse.StatusCode == HttpStatusCode.UnprocessableEntity, $"422 Accepted was not returned from enrolment, returned {enrolmentResponse.ReadResponseAsString()}");

            JArray enrolmentResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult[0]["code"].ToString() == "G012", $"Validtion message returned was not as expected, returned {enrolmentResult[0]["code"].ToString()} and expected G012");
            Assert.True(enrolmentResult[0]["message"].ToString() == "One of the mandatory field is missing in the request.", $"Validtion message returned was not as expected, returned {enrolmentResult[0]["message"].ToString()} and expected One of the mandatory field is missing in the request.");
        }

        //Removed the Snapx channel for which fix is required & story also there #426387
        [Theory]
        [Trait("Category", "Enrolment-Regression")]
        [InlineData("DELTA.COM")]
        [InlineData("POWERAPP")]
        [InlineData("WEB-MYA")]
        public async Task ValidateMemberTypeAccount_AllChannels(string channel)
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string postCode = "CV459HP";
            string country = "GB";
            string channelID = channel;

            System.Net.Http.HttpResponseMessage enrolmentResponse = Requests.Endpoint.Microservice.Enrolment(env, tokenFixture.GetToken("enrolment"), channelID, firstName, lastName, emailId, postCode, country, output, memberType: "Account");
            Assert.True(enrolmentResponse.StatusCode == HttpStatusCode.UnprocessableEntity, $"422 Accepted was not returned from enrolment, returned {enrolmentResponse.ReadResponseAsString()}");

            JArray enrolmentResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult[0]["code"].ToString() == "1025", $"Validtion message returned was not as expected, returned {enrolmentResult[0]["code"].ToString()} and expected 1025");
            Assert.True(enrolmentResult[0]["message"].ToString() == "The member type in the request is invalid", $"Validtion message returned was not as expected, returned {enrolmentResult[0]["message"].ToString()} and expected One of the mandatory field is missing in the request.");
        }

        //Removed the Snapx channel for which fix is required & story also there #426387
        [Theory]
        [Trait("Category", "Enrolment-Regression")]
        [InlineData("DELTA.COM")]
        [InlineData("POWERAPP")]
        [InlineData("WEB-MYA")]
        public async Task ValidateMemberTypeHousehold_AllChannels(string channel)
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string postCode = "CV459HP";
            string country = "GB";
            string channelID = channel;

            System.Net.Http.HttpResponseMessage enrolmentResponse = Requests.Endpoint.Microservice.Enrolment(env, tokenFixture.GetToken("enrolment"), channelID, firstName, lastName, emailId, postCode, country, output, memberType: "Household");
            Assert.True(enrolmentResponse.StatusCode == HttpStatusCode.UnprocessableEntity, $"422 Accepted was not returned from enrolment, returned {enrolmentResponse.ReadResponseAsString()}");

            JArray enrolmentResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult[0]["code"].ToString() == "1025", $"Validtion message returned was not as expected, returned {enrolmentResult[0]["code"].ToString()} and expected 1025");
            Assert.True(enrolmentResult[0]["message"].ToString() == "The member type in the request is invalid", $"Validtion message returned was not as expected, returned {enrolmentResult[0]["message"].ToString()} and expected One of the mandatory field is missing in the request.");
        }

        //Removed the VRed channel for which fix is required & story also there #426387
        [Theory]
        [Trait("Category", "Enrolment-Regression")]
        [InlineData("DELTA.COM")]
        [InlineData("POWERAPP")]
        [InlineData("WEB-MYA")]
        public async Task ValidatePhoneTypeBlank_AllChannels(string channel)
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string postCode = "CV459HP";
            string country = "GB";
            string channelID = channel;

            System.Net.Http.HttpResponseMessage enrolmentResponse = Requests.Endpoint.Microservice.Enrolment(env, tokenFixture.GetToken("enrolment"), channelID, firstName, lastName, emailId, postCode, country, output, phoneType: "");
            Assert.True(enrolmentResponse.StatusCode == HttpStatusCode.UnprocessableEntity, $"422 Accepted was not returned from enrolment, returned {enrolmentResponse.ReadResponseAsString()}");

            JArray enrolmentResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult[0]["code"].ToString() == "G012", $"Validtion message returned was not as expected, returned {enrolmentResult[0]["code"].ToString()} and expected G012");
            Assert.True(enrolmentResult[0]["message"].ToString() == "One of the mandatory field is missing in the request.", $"Validtion message returned was not as expected, returned {enrolmentResult[0]["message"].ToString()} and expected One of the mandatory field is missing in the request.");
        }

        [Fact]
        [Trait("Category", "Enrolment-Regression")]
        public async Task TC01_PostCode_NonEmpty_ValidValue_Check_For_US_DELTACHANNEL()
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string postCode = "90210";
            string country = "US";
            string channelID = "DELTA.COM";

            System.Net.Http.HttpResponseMessage enrolmentResponse = Requests.Endpoint.Microservice.Enrolment(env, tokenFixture.GetToken("enrolment"), channelID, firstName, lastName, emailId, postCode, country, output);

            Assert.True(enrolmentResponse.StatusCode == HttpStatusCode.Accepted, $"202 Accepted was not returned from enrolment, response was: {enrolmentResponse.ReadResponseAsString()}");

            JObject enrolmentResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult["customerId"] != null && !enrolmentResult["customerId"].ToString().Equals(string.Empty), "customer id should be not blank");
        }

        [Fact]
        [Trait("Category", "Enrolment-Regression")]
        public async Task TC02_PostCode_Null_Check_For_US_DELTACHANNEL()
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string postCode = null;
            string country = "US";
            string channelID = "DELTA.COM";

            System.Net.Http.HttpResponseMessage enrolmentResponse = Requests.Endpoint.Microservice.Enrolment(env, tokenFixture.GetToken("enrolment"), channelID, firstName, lastName, emailId, postCode, country, output);

            Assert.True(enrolmentResponse.StatusCode == HttpStatusCode.UnprocessableEntity, "422 Accepted was not returned from  enrolment");

            JArray enrolmentResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult[0]["code"].ToString().Equals("G012"), "code should be G012 for empty post code US country code");
        }

        [Fact]
        [Trait("Category", "Enrolment-Regression")]
        public async Task TC03_PostCode_OnlyNumbers_Check_For_US_DELTACHANNEL()
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string postCode = "5678";
            string country = "US";
            string channelID = "DELTA.COM";

            System.Net.Http.HttpResponseMessage enrolmentResponse = Requests.Endpoint.Microservice.Enrolment(env, tokenFixture.GetToken("enrolment"), channelID, firstName, lastName, emailId, postCode, country, output);

            Assert.True(enrolmentResponse.StatusCode == HttpStatusCode.Accepted, $"202 Accepted was not returned from enrolment, response was: {enrolmentResponse.ReadResponseAsString()}");

            JObject enrolmentResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult["customerId"] != null && !enrolmentResult["customerId"].ToString().Equals(string.Empty), "customer id should be not blank");
        }

        [Fact]
        [Trait("Category", "Enrolment-Regression")]
        public async Task TC04_PostCode_SpecialCharecters_Check_For_US_DELTACHANNEL()
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string postCode = "*&%789";
            string country = "US";
            string channelID = "DELTA.COM";

            System.Net.Http.HttpResponseMessage enrolmentResponse = Requests.Endpoint.Microservice.Enrolment(env, tokenFixture.GetToken("enrolment"), channelID, firstName, lastName, emailId, postCode, country, output);

            Assert.True(enrolmentResponse.StatusCode == HttpStatusCode.Accepted, $"202 Accepted was not returned from enrolment, response was: {enrolmentResponse.ReadResponseAsString()}");

            JObject enrolmentResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult["customerId"] != null && !enrolmentResult["customerId"].ToString().Equals(string.Empty), "customer id should be not blank");
        }

        [Fact]
        [Trait("Category", "Enrolment-Regression")]
        public async Task TC06_PostCode_Empty_Check_For_US_DELTACHANNEL()
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string postCode = string.Empty;
            string country = "US";
            string channelID = "DELTA.COM";

            System.Net.Http.HttpResponseMessage enrolmentResponse = Requests.Endpoint.Microservice.Enrolment(env, tokenFixture.GetToken("enrolment"), channelID, firstName, lastName, emailId, postCode, country, output);

            Assert.True(enrolmentResponse.StatusCode == HttpStatusCode.UnprocessableEntity, "422 Accepted was not returned from enrolment");

            JArray enrolmentResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult[0]["code"].ToString().Equals("G012"), "code should be G012 for empty post code US country code");
        }

        [Fact]
        [Trait("Category", "Enrolment-Regression")]
        public async Task TC07_PostCode_NonEmpty_Check_For_US_DELTACHANNEL()
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string postCode = "50309";
            string country = "US";
            string channelID = "DELTA.COM";

            System.Net.Http.HttpResponseMessage enrolmentResponse = Requests.Endpoint.Microservice.Enrolment(env, tokenFixture.GetToken("enrolment"), channelID, firstName, lastName, emailId, postCode, country, output);
            output.WriteLine($"Response from enrolment facade {Environment.NewLine} {enrolmentResponse}");

            Assert.True(enrolmentResponse.StatusCode == HttpStatusCode.Accepted, $"202 Accepted was not returned from enrolment, response was: {enrolmentResponse.ReadResponseAsString()}");

            JObject enrolmentResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult["customerId"] != null && !enrolmentResult["customerId"].ToString().Equals(string.Empty), "customer id should be not blank");
        }

        [Fact]
        [Trait("Category", "Enrolment-Regression")]
        public async Task TC09_PostCode_Empty_Check_For_US_POWERAPPCHANNEL()
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string postCode = string.Empty;
            string country = "US";
            string channelID = "POWERAPP";

            System.Net.Http.HttpResponseMessage enrolmentResponse = Requests.Endpoint.Microservice.Enrolment(env, tokenFixture.GetToken("enrolment"), channelID, firstName, lastName, emailId, postCode, country, output);

            Assert.True(enrolmentResponse.StatusCode == HttpStatusCode.UnprocessableEntity, "422 Accepted was not returned from enrolment");

            JArray enrolmentResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult[0]["code"].ToString().Equals("G012"), "code should be G012 for empty post code US country code");
        }

        [Fact]
        [Trait("Category", "Enrolment-Regression")]
        public async Task TC10_PostCode_Empty_Check_For_GB_DELTACHANNEL()
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string postCode = string.Empty;
            string country = "GB";
            string channelID = "DELTA.COM";

            System.Net.Http.HttpResponseMessage enrolmentResponse = Requests.Endpoint.Microservice.Enrolment(env, tokenFixture.GetToken("enrolment"), channelID, firstName, lastName, emailId, postCode, country, output);

            Assert.True(enrolmentResponse.StatusCode == HttpStatusCode.UnprocessableEntity, "422 Accepted was not returned from enrolment");

            JArray enrolmentResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult[0]["code"].ToString().Equals("G012"), "code should be G012 for empty post code US country code");
        }

        [Fact]
        [Trait("Category", "Enrolment-Regression")]
        public async Task TC11_PostCode_NonEmpty_Check_For_GB_DELTACHANNEL()
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string postCode = "RH11 9LG";
            string country = "GB";
            string channelID = "DELTA.COM";

            System.Net.Http.HttpResponseMessage enrolmentResponse = Requests.Endpoint.Microservice.Enrolment(env, tokenFixture.GetToken("enrolment"), channelID, firstName, lastName, emailId, postCode, country, output);

            Assert.True(enrolmentResponse.StatusCode == HttpStatusCode.Accepted, $"202 Accepted was not returned from enrolment, response was: {enrolmentResponse.ReadResponseAsString()}");

            JObject enrolmentResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult["customerId"] != null && !enrolmentResult["customerId"].ToString().Equals(string.Empty), "customer id should be not blank");
        }

        [Fact]
        [Trait("Category", "Enrolment-Regression")]
        public async Task TC13PostCode_Empty_Check_For_GB_POWERAPPCHANNEL()
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string postCode = string.Empty;
            string country = "GB";
            string channelID = "POWERAPP";

            System.Net.Http.HttpResponseMessage enrolmentResponse = Requests.Endpoint.Microservice.Enrolment(env, tokenFixture.GetToken("enrolment"), channelID, firstName, lastName, emailId, postCode, country, output);

            Assert.True(enrolmentResponse.StatusCode == HttpStatusCode.UnprocessableEntity, "422 Accepted was not returned from enrolment");

            JArray enrolmentResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult[0]["code"].ToString().Equals("G012"), "code should be G012 for empty post code US country code");
        }

        //Changed test following discussion with Fayyaz "basically for all countries, except UK and US, postcode is optional now for DELTA CHANNEL"
        [Fact]
        [Trait("Category", "Enrolment-Regression")]
        public async Task TC14_PostCode_Empty_Check_For_OtherCountry_DELTACHANNEL()
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string postCode = string.Empty;
            string country = "IL";
            string channelID = "DELTA.COM";

            System.Net.Http.HttpResponseMessage enrolmentResponse = Requests.Endpoint.Microservice.Enrolment(env, tokenFixture.GetToken("enrolment"), channelID, firstName, lastName, emailId, postCode, country, output);

            Assert.True(enrolmentResponse.StatusCode == HttpStatusCode.Accepted, $"202 Accepted was not returned from enrolment, response was: {enrolmentResponse.ReadResponseAsString()}");

            JObject enrolmentResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult["customerId"] != null && !enrolmentResult["customerId"].ToString().Equals(string.Empty), "customer id should be not blank");
        }

        [Fact]
        [Trait("Category", "Enrolment-Regression")]
        public async Task TC015PostCode_NonEmpty_Check_For_OtherCountry_DELTACHANNEL()
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string postCode = "500090";
            string country = "IN";
            string channelID = "DELTA.COM";

            System.Net.Http.HttpResponseMessage enrolmentResponse = Requests.Endpoint.Microservice.Enrolment(env, tokenFixture.GetToken("enrolment"), channelID, firstName, lastName, emailId, postCode, country, output);

            Assert.True(enrolmentResponse.StatusCode == HttpStatusCode.Accepted, $"202 Accepted was not returned from enrolment, response was: {enrolmentResponse.ReadResponseAsString()}");

            JObject enrolmentResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult["customerId"] != null && !enrolmentResult["customerId"].ToString().Equals(string.Empty), "customer id should be not blank");
        }

        [Fact]
        [Trait("Category", "Enrolment-Regression")]
        public async Task TC017_PostCode_NonEmpty_Check_For_OtherCountry_POWERCHANNEL()
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string postCode = "4483800";
            string country = "IL";
            string channelID = "POWERAPP";

            System.Net.Http.HttpResponseMessage enrolmentResponse = Requests.Endpoint.Microservice.Enrolment(env, tokenFixture.GetToken("enrolment"), channelID, firstName, lastName, emailId, postCode, country, output);

            Assert.True(enrolmentResponse.StatusCode == HttpStatusCode.Accepted, $"202 Accepted was not returned from enrolment, response was: {enrolmentResponse.ReadResponseAsString()}");

            JObject enrolmentResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult["customerId"] != null && !enrolmentResult["customerId"].ToString().Equals(string.Empty), "customer id should be not blank");
        }

        [Fact]
        [Trait("Category", "Enrolment-Regression")]
        public async Task TC018_Validate_CustomerId_Generation_WEB_MYA_Channel()
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

            string strInputQueryForCustomerSearch = $"SELECT * FROM CustomerCollection c WHERE c.PartitionKey = '{enrolmentResult["customerId"]}' and c.DocType='Customer'";

            Customer objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomerSearch);

            int retry = 5;
            while (retry > 0 && objCustomer == null)
            {
                await Task.Delay(500);
                objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomerSearch);
                retry--;
            }

            Assert.True(objCustomer.EnrollmentChannel.Equals(channelID), "Channel Id is not same in Cosmos DB");
        }

        [Fact]
        [Trait("Category", "Enrolment-Regression")]
        public async Task ValidateCustomerIdInAzureB2CForPowerappChannel()
        {
            string flynigClubCardNumber = await CreateEnrolmentForPowerappChannel();

            System.Net.Http.HttpResponseMessage azureB2CResponse = Requests.Endpoint.Microservice.AzureB2C(tokenFixture.GetToken("azureb2c"), flynigClubCardNumber);

            Assert.True(azureB2CResponse.StatusCode == HttpStatusCode.OK, "200 Ok was not returned from Azure B2C");

            JObject azureB2CResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await azureB2CResponse.Content.ReadAsStringAsync());

            int retryForAzureB2C = 10;
            while (retryForAzureB2C > 0 && !(azureB2CResult["value"].Count() > 0))
            {
                await Task.Delay(5000);
                azureB2CResponse = Requests.Endpoint.Microservice.AzureB2C(tokenFixture.GetToken("azureb2c"), flynigClubCardNumber);

                azureB2CResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await azureB2CResponse.Content.ReadAsStringAsync());

                retryForAzureB2C--;
            }

            Assert.True(azureB2CResult["value"][0]["userPrincipalName"].ToString().Contains(flynigClubCardNumber), $"Flyning Club Member number not matched in userPrincipalName, FC no is {flynigClubCardNumber}");
            Assert.True(azureB2CResult["value"][0]["accountEnabled"].ToString().Equals("True"), $"Account enabled value is not True, the value is {azureB2CResult["value"][0]["accountEnabled"]}");
        }


        [Fact]
        [Trait("Category", "Enrolment-Regression")]
        public async Task ValidateCustomerIdInVRedForPowerappChannel()
        {
            string flynigClubCardNumber = await CreateEnrolmentForPowerappChannel();

            System.Net.Http.HttpResponseMessage vRedResponse = Requests.Endpoint.Microservice.Vred(tokenFixture.GetToken("vred"), flynigClubCardNumber);

            int retry = 10;
            while (retry > 0 && vRedResponse.StatusCode != HttpStatusCode.OK)
            {
                await Task.Delay(5000);
                vRedResponse = Requests.Endpoint.Microservice.Vred(tokenFixture.GetToken("vred"), flynigClubCardNumber);
                retry--;
            }

            Assert.True(vRedResponse.StatusCode == HttpStatusCode.OK, "200 Ok was not returned from Vred");

            JObject vRedResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await vRedResponse.Content.ReadAsStringAsync());

            Assert.True(vRedResult["partnerUserId"].ToString().Equals(flynigClubCardNumber), $"Flyning Club Member number not matched, FC no is {flynigClubCardNumber}");
        }

        private async Task<string> CreateEnrolmentForPowerappChannel()
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string postCode = "SO14 0LT";
            string country = "GB";
            string channelID = "POWERAPP";
            System.Net.Http.HttpResponseMessage enrolmentResponse = Requests.Endpoint.Microservice.Enrolment(env, tokenFixture.GetToken("enrolment"), channelID, firstName, lastName, emailId, postCode, country, output);

            Assert.True(enrolmentResponse.StatusCode == HttpStatusCode.Accepted, $"202 Accepted was not returned from enrolment, response was: {enrolmentResponse.ReadResponseAsString()}");

            JObject enrolmentResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult["customerId"] != null && !enrolmentResult["customerId"].ToString().Equals(string.Empty), "customer id should be not blank");

            string strInputQueryForCustomerSearch = $"SELECT * FROM CustomerCollection c WHERE c.PartitionKey = '{enrolmentResult["customerId"]}' and c.DocType='Customer'";

            Customer objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomerSearch);
            int retry = 5;
            while (retry > 0 && objCustomer == null)
            {
                await Task.Delay(500);
                objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomerSearch);
                retry--;
            }

            string flynigClubCardNumber = objCustomer.LoyaltyMemberNumber;

            return flynigClubCardNumber;
        }

        [Fact]
        [Trait("Category", "Enrolment-Regression")]
        public async Task CreateEnrolmentForPowerappChannel_CustomerSearchCollection()
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string postCode = "SO14 0LT";
            string country = "GB";
            string channelID = "POWERAPP";
            System.Net.Http.HttpResponseMessage enrolmentResponse = Requests.Endpoint.Microservice.Enrolment(env, tokenFixture.GetToken("enrolment"), channelID, firstName, lastName, emailId, postCode, country, output);

            Assert.True(enrolmentResponse.StatusCode == HttpStatusCode.Accepted, $"202 Accepted was not returned from enrolment, response was: {enrolmentResponse.ReadResponseAsString()}");

            JObject enrolmentResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult["customerId"] != null && !enrolmentResult["customerId"].ToString().Equals(string.Empty), "customer id should be not blank");

            string strInputQueryForCustomerCollection = "SELECT Top 1 * FROM CustomerCollection c WHERE c.FirstName = '" + firstName + "' and c.DocType='CustomerSearch' ORDER BY c._ts DESC";

            Customer objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomerCollection);
            int retry = 5;
            while (retry > 0 && objCustomer == null)
            {
                await Task.Delay(500);
                objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomerCollection);
                retry--;
            }

            Assert.True(objCustomer != null, "Query from Cosmos DB for Customer collection is null after 5 re tries");

            string flynigClubCardNumber = objCustomer.LoyaltyMemberNumber;

            string strInputQueryForCustomerSearchCollection = $"SELECT Top 1 * FROM CustomerSearch c WHERE c.FirstName = '" + firstName + "' and c.DocType='CustomerSearch' ORDER BY c._ts DESC";

            CustomerSearch objCustomerSearchCollection = cosmosFixture.QueryItem<CustomerSearch>(strInputQueryForCustomerSearchCollection);
            retry = 5;
            while (retry > 0 && objCustomerSearchCollection == null)
            {
                await Task.Delay(500);
                objCustomerSearchCollection = cosmosFixture.QueryItem<CustomerSearch>(strInputQueryForCustomerSearchCollection);
                retry--;
            }
            Assert.True(objCustomerSearchCollection != null, "Query from Cosmos DB for Customer Search collection is null after 5 re tries. Please verify feature flag : CustDB:CustSearchColCntnrForTest");

            string flynigClubCardNumberOfCustomerSearchCollection = objCustomerSearchCollection.LoyaltyMemberNumber;

            Assert.True(flynigClubCardNumber == flynigClubCardNumberOfCustomerSearchCollection, "LoyaltyMemberNumber is different from customer collection to customer search collection");

        }

        [Fact]
        [Trait("Category", "Enrolment-Regression")]
        public async Task ValidateActivePendingStatusForDeltaChannelEnrolment()
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string postCode = "RH11 9LG";
            string country = "GB";
            string channelID = "DELTA.COM";
            string under12 = DateTime.Now.AddYears(-10).ToString("yyyy-MM-dd");

            System.Net.Http.HttpResponseMessage enrolmentResponse = Requests.Endpoint.Microservice.Enrolment(env, tokenFixture.GetToken("enrolment"), channelID, firstName, lastName, emailId, postCode, country, output, dateOfBirth: under12);

            Assert.True(enrolmentResponse.StatusCode == HttpStatusCode.Accepted, $"202 Accepted was not returned from enrolment, response was: {enrolmentResponse.ReadResponseAsString()}");

            JObject enrolmentResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult["customerId"] != null && !enrolmentResult["customerId"].ToString().Equals(string.Empty), "customer id should be not blank");

            string strInputQueryForCustomerSearch = $"SELECT * FROM CustomerCollection c WHERE c.PartitionKey = '{enrolmentResult["customerId"]}' and c.DocType='Customer'";

            Customer objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomerSearch);

            int retry = 10;
            while (retry > 0 && objCustomer == null)
            {
                await Task.Delay(5000);
                objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomerSearch);
                retry--;
            }

            Assert.True(objCustomer != null, "Query from Cosmos DB for Customer Search is null after 10 re tries");
            Assert.True(objCustomer.EnrollmentChannel.Equals(channelID), "Channel Id is not same in Cosmos DB");
            Assert.True(objCustomer.FirstName.Equals(firstName), $"first Name is not same in Cosmos DB, actual is {objCustomer.FirstName} and expected {firstName}");
            Assert.True(objCustomer.LastName.Equals(lastName), $"Last Name is not same in Cosmos DB, actual is {objCustomer.LastName} and expected {lastName}");

            Assert.True(objCustomer.Status.Equals("Active Pending"), $"Status is wrong in Cosmos DB FC No is {objCustomer.LoyaltyMemberNumber} & status is {objCustomer.Status}");

        }

        [Theory]
        [Trait("Category", "Enrolment-Regression")]
        [InlineData("DELTA.COM")]
        [InlineData("POWERAPP")]
        [InlineData("VRED")]
        [InlineData("WEB-MYA")]
        public async Task ValidateDedupeCheck_AllChannels(string channel)
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string postCode = "RH11 9LG";
            string country = "GB";

            System.Net.Http.HttpResponseMessage enrolmentResponse = Requests.Endpoint.Microservice.Enrolment(env, tokenFixture.GetToken("enrolment"), channel, firstName, lastName, emailId, postCode, country, output);

            Assert.True(enrolmentResponse.StatusCode == HttpStatusCode.Accepted, $"202 Accepted was not returned from enrolment, response was: {enrolmentResponse.ReadResponseAsString()}");

            JObject enrolmentResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult["customerId"] != null && !enrolmentResult["customerId"].ToString().Equals(string.Empty), "customer id should be not blank");

            string strInputQueryForCustomerSearch = $"SELECT * FROM CustomerCollection c WHERE c.PartitionKey = '{enrolmentResult["customerId"]}' and c.DocType='Customer'";

            Customer objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomerSearch);

            int retry = 10;
            while (retry > 0 && objCustomer == null)
            {
                await Task.Delay(5000);
                objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomerSearch);
                retry--;
            }

            Assert.True(objCustomer != null, "Query from Cosmos DB for Customer Search is null after 10 re tries");
            Assert.True(objCustomer.EnrollmentChannel.Equals(channel), "Channel Id is not same in Cosmos DB");

            System.Net.Http.HttpResponseMessage deDupeEnrolmentResponse = Requests.Endpoint.Microservice.Enrolment(env, tokenFixture.GetToken("enrolment"), channel, firstName, lastName, emailId, postCode, country, output);

            Assert.True(deDupeEnrolmentResponse.StatusCode == HttpStatusCode.UnprocessableEntity, "422 Accepted was not returned from enrolment");

            JArray deDupeEnrolmentResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await deDupeEnrolmentResponse.Content.ReadAsStringAsync());

            //De dupe validation error message 
            if (channel.Equals("VRED"))
            {
                Assert.True(deDupeEnrolmentResult[0]["code"].ToString() == "1038", $"Error code returned was not as expected, returned {deDupeEnrolmentResult[0]["code"].ToString()} and expected 1038");

                Assert.True(deDupeEnrolmentResult[0]["message"].ToString().Contains("Matching account exists"), $"Validtion message returned was not as expected, returned {deDupeEnrolmentResult[0]["message"].ToString()}");
            }
            else
            {
                Assert.True(deDupeEnrolmentResult[0]["code"].ToString() == "1013", $"Error code returned was not as expected, returned {deDupeEnrolmentResult[0]["code"].ToString()} and expected 1013");

                Assert.True(deDupeEnrolmentResult[0]["message"].ToString().Contains("Duplicate member found."), $"Validtion message returned was not as expected, returned {deDupeEnrolmentResult[0]["message"].ToString()}");
            }
        }

        [Theory]
        [Trait("Category", "Enrolment-Regression")]
        [InlineData("9106302426")]
        [InlineData("9106381537")]
        [InlineData("9106383582")]
        public async Task ValidateCumulativeVirginPoints(string fcNumber)
        {
            System.Net.Http.HttpResponseMessage vRedResponse = Requests.Endpoint.Microservice.VredPoints(tokenFixture.GetToken("vred"), fcNumber);

            int retry = 10;
            while (retry > 0 && vRedResponse.StatusCode != HttpStatusCode.OK)
            {
                await Task.Delay(5000);
                vRedResponse = Requests.Endpoint.Microservice.VredPoints(tokenFixture.GetToken("vred"), fcNumber);
                retry--;
            }

            Assert.True(vRedResponse.StatusCode == HttpStatusCode.OK, "200 Ok was not returned from Vred");

            JObject vRedResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await vRedResponse.Content.ReadAsStringAsync());

            System.Net.Http.HttpResponseMessage vRedPointsMicroServiceResponse = Requests.Endpoint.Microservice.VredPointsMsEndPoint(env, tokenFixture.GetToken("virginPoints"), fcNumber);

            string vRedPointsMicroServiceResponseBody = await vRedPointsMicroServiceResponse.Content.ReadAsStringAsync();
            Assert.True(vRedPointsMicroServiceResponse.StatusCode == HttpStatusCode.OK, $"200 Ok was not returned from MS end response is {vRedPointsMicroServiceResponseBody}");

            JObject virginPointsResult = (JObject)JsonConvert.DeserializeObject<dynamic>(vRedPointsMicroServiceResponseBody);

            Assert.True(vRedResult["current"].ToString().Equals(virginPointsResult["Current"].ToString()), $"Both the current point values are not same response from vred is {vRedResult}, MS endpoint response is {virginPointsResult}");
            Assert.True(vRedResult["usable"].ToString().Equals(virginPointsResult["Usable"].ToString()), $"Both the usable point values are not same response from vred is {vRedResult}, MS endpoint response is {virginPointsResult}");
        }

        [Theory]
        [Trait("Category", "Enrolment-Regression")]
        [InlineData("POWERAPP")]
        [InlineData("DELTA.COM")]
        public async Task ValidateEnrolmentDataInCustomerDoc(string channelID)
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string postCode = "SO14 0LT";
            string country = "GB";

            System.Net.Http.HttpResponseMessage enrolmentResponse = Requests.Endpoint.Microservice.Enrolment(env, tokenFixture.GetToken("enrolment"), channelID, firstName, lastName, emailId, postCode, country, output);

            Assert.True(enrolmentResponse.StatusCode == HttpStatusCode.Accepted, $"202 Accepted was not returned from enrolment, response was: {enrolmentResponse.ReadResponseAsString()}");

            JObject enrolmentResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult["customerId"] != null && !enrolmentResult["customerId"].ToString().Equals(string.Empty), "customer id should be not blank");

            string strInputQueryForCustomerDoc = $"SELECT * FROM CustomerCollection c WHERE c.PartitionKey = '{enrolmentResult["customerId"]}' and c.DocType='Customer'";

            Customer objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomerDoc);

            int retry = 5;
            while (retry > 0 && objCustomer == null)
            {
                await Task.Delay(1000);
                objCustomer = cosmosFixture.QueryItem<Customer>(strInputQueryForCustomerDoc);
                retry--;
            }

            Address address = objCustomer.AddressDetails[0];
            IEnumerable<ContactDetails> contactDetails = objCustomer.ContactDetails;
            ContactDetails emailDetails = contactDetails.FirstOrDefault(c => c.ContactType.Equals("Email"));
            ContactDetails phoneDetails = contactDetails.FirstOrDefault(c => c.ContactType.Equals("Phone"));
            Authorization authDetails = objCustomer.Authorizations[0];
            Consent consentDetails = authDetails.Consents[0];
            Assert.True(objCustomer.EnrollmentChannel.Equals(channelID), $"Channel Id is not same in Cosmos DB,expected {channelID}, actual {objCustomer.EnrollmentChannel}");
            Assert.True(objCustomer.Active.Equals("Y"), $"Active value is not showing in Customer Document, expected Y, actual {objCustomer.Active}");
            Assert.True(objCustomer.CreatedOn != null, $"Created On field is null in Customer Document actual result {objCustomer.CreatedOn}");
            Assert.True(objCustomer.LastUpdatedOn != null, $"LastUpdatedOn field is null in Customer Document {objCustomer.LastUpdatedOn}");
            Assert.True(objCustomer.LastUpdatedOn.Equals(objCustomer.CreatedOn), $"LastUpdatedOn & Created On dates are not same in Customer Document expected{objCustomer.CreatedOn}");
            Assert.True(objCustomer.Brand.Equals("VAA"), $"Brand value is not showing in Customer Document, expected VAA actual {objCustomer.Brand}");
            channelID = channelID.Equals("POWERAPP") ? "456321" : channelID;
            Assert.True(objCustomer.CreatedBy.Equals(channelID), $"Credated By value is wrong in Customer Document expected {channelID}, actual {objCustomer.CreatedBy}");
            Assert.True(objCustomer.LastUpdatedBy.Equals(channelID), $"Last updated By is not same in Customer Document expected {channelID}, actual {objCustomer.LastUpdatedBy}");
            Assert.True(address.CreatedBy.Equals(channelID), $"Created By under address details is not same in Customer Document expected {channelID}, actual {address.CreatedBy}");
            Assert.True(emailDetails.CreatedBy.Equals(channelID), $"Created By under email contact details is not same in Customer Document expected {channelID},actual {emailDetails.CreatedBy}");
            Assert.True(phoneDetails.CreatedBy.Equals(channelID), $"Created By under phone contact details is not same in Customer Document expected {channelID}, actual {phoneDetails.CreatedBy}");
            Assert.True(consentDetails.CreatedBy.Equals(channelID), $"Created By under Consent Details is not same in Customer Document expected {channelID}, actual {consentDetails.CreatedBy}");
            Assert.True(objCustomer.DateOfBirth.Value.ToString("yyyy-MM-dd").Equals("1997-11-01"), $"Date of Birth value is not same in Customer Document expected 1997-11-01, actual {objCustomer.DateOfBirth}");
            Assert.True(objCustomer.CustomerId.Equals(enrolmentResult["customerId"].ToString()), $"Customer Id is not same in Customer Document expected {enrolmentResult["customerId"]} actual {objCustomer.CustomerId}");
            Assert.True(objCustomer.FirstName.Equals(firstName), $"First name is not same in Customer Document expected {firstName} actual {objCustomer.FirstName}");
            Assert.True(objCustomer.Gender.Equals("M"), $"Gender is not same in Customer Document expected M actual {objCustomer.Gender}");
            Assert.True(objCustomer.LastName.Equals(lastName), $"Last name is not same in Customer Document expected {lastName} actual {objCustomer.LastName}");
            if (channelID.Equals("POWERAPP"))
            {
                Assert.True(objCustomer.Title.Equals("Mr"), $"Title is not same in Customer Search Document expected Mr but found {objCustomer.Title}");
            }
            Assert.True(objCustomer.Status.Equals("Active"), $"Customer status is not active in Customer Document actual {objCustomer.Status}");
            Assert.True(address.AddressId != null, $"Address Id field is null in Customer Document actual {address.AddressId}");
            Assert.True(address.AddressType.Equals("Work"), $"Address Type field is not matched in Customer Document expected Work actual {address.AddressType}");
            Assert.True(address.AddressLine1.Equals("104 Test address1"), $"Address Id field is not matched in Customer Document expected 104 Test address1 actual {address.AddressLine1}");
            Assert.True(address.City.Equals("Tel Aviv"), $"City field is not matched in Customer Document expected Tel Aviv, actual {address.City}");
            Assert.True(address.PostCode.Equals(postCode), $"Post code field is not matched in Customer Document expected {postCode}, actual {address.PostCode}");
            Assert.True(address.Country.Equals("United Kingdom"), $"Country field is not matched in Customer Document expected United Kingdom actual {address.Country}");
            Assert.True(address.Province.Equals("NY"), $"Province field is not matched in Customer Document expected NY, actual {address.Province}");
            Assert.True(address.AddressVerified.Equals("Y"), $"Address verified flag is not Y in Customer Document actual {address.AddressVerified}");
            Assert.True(address.Valid.Equals("Y"), $"Valid flag under address details is not Y in Customer Document actual {address.Valid}");
            Assert.True(address.Primary.Equals("Y"), $"Primary flag under address details is not Y in Customer Document actual {address.Primary}");
            Assert.True(address.CreatedBy.Equals(address.LastUpdatedBy), "Created By & Lastupdated by under address details is not same in Customer Document");
            Assert.True(address.CreatedOn != null, $"Created On field is null in Customer Document actual value is {address.CreatedOn}");
            Assert.True(address.LastUpdatedOn != null, $"LastUpdatedOn field is null in Customer Document actual value is {address.LastUpdatedOn}");
            Assert.True(address.LastUpdatedOn.Equals(address.CreatedOn), $"LastUpdatedOn & Created On dates are not same in Customer Document expected{address.CreatedOn}");
            Assert.True(emailDetails != null, "No email details found in Customer Document");
            Assert.True(emailDetails.ContactId != null, $"Contact Id is null in Customer Document actual {emailDetails.ContactId}");
            Assert.True(emailDetails.ContactValue.Equals(emailId), $"Email id is not matched in Customer Document expected {emailId} actual {emailDetails.ContactValue}");
            Assert.True(emailDetails.Valid.Equals("Y"), $"Valid flag under email details is not Y in Customer Document actual {emailDetails.Valid}");
            Assert.True(emailDetails.Primary.Equals("Y"), $"Primary flag under email details is not Y in Customer Document actual {emailDetails.Primary}");
            Assert.True(emailDetails.CreatedBy.Equals(emailDetails.LastUpdatedBy), "Created By & Lastupdated by is not same under email details in Customer Document");
            Assert.True(emailDetails.CreatedOn != null, "Created On field under email details is null in Customer Document");
            Assert.True(emailDetails.LastUpdatedOn != null, "LastUpdatedOn field under email details is null in Customer Document");
            Assert.True(emailDetails.LastUpdatedOn.Equals(emailDetails.CreatedOn), $"LastUpdatedOn & Created On dates are not same under email details in Customer Document expected{emailDetails.CreatedOn}");
            Assert.True(emailDetails.Usage.Equals("Personal"), $"Email usage field is wrong expected Personal, actual {emailDetails.Usage}");
            Assert.True(phoneDetails != null, "No phone details found in Customer Document");
            Assert.True(phoneDetails.ContactId != null, $"Contact Id under phone details is null in Customer Document actual {phoneDetails.ContactId}");
            Assert.True(phoneDetails.ContactValue != null, $"Phone no is  null in Customer Document actual {phoneDetails.ContactValue}");
            Assert.True(phoneDetails.ContactSubType.Equals("Mobile"), $"Contact Sub type is not matched in Customer Document expected Mobile actual {phoneDetails.ContactSubType}");
            Assert.True(phoneDetails.Valid.Equals("Y"), $"Valid flag under phone details is not Y in Customer Document actual {phoneDetails.Valid}");
            Assert.True(phoneDetails.Primary.Equals("Y"), $"Primary flag under phone details is not Y in Customer Document actual {phoneDetails.Primary}");
            Assert.True(phoneDetails.CreatedBy.Equals(phoneDetails.LastUpdatedBy), "Created By & Lastupdated by is not same under phone details in Customer Document");
            Assert.True(phoneDetails.CreatedOn != null, "Created On under phone details field is null in Customer Document");
            Assert.True(phoneDetails.LastUpdatedOn != null, "LastUpdatedOn field under phone details is null in Customer Document");
            Assert.True(phoneDetails.LastUpdatedOn.Equals(phoneDetails.CreatedOn), $"LastUpdatedOn & Created On dates under phone details are not same in Customer Document expected{phoneDetails.CreatedOn}");
            Assert.True(phoneDetails.Usage.Equals("Personal"), $"Phone usage field is wrong expected Personal, actual {phoneDetails.Usage}");
            Assert.True(authDetails != null, $"No Authorization details found in Customer Document actual {authDetails}");
            Assert.True(authDetails.RelationType.Equals("FlyingClubAuthorizedUserAccount"), $"Relation type value is wrong in Customer Document expected FlyingClubAuthorizedUserAccount actual {authDetails.RelationType}");
            Assert.True(consentDetails != null, $"Consent details not found in Customer Document actual {consentDetails}");
            Assert.True(consentDetails.ConsentFor.Equals("FlyingClubGuardianship"), $"Consent For value is wrong in Customer Document expected FlyingClubGuardianship actual {consentDetails.ConsentFor}");
            Assert.True(consentDetails.CreatedBy.Equals(consentDetails.LastUpdatedBy), "Created By & Lastupdated by under Consent details is not same in Customer Document");
            Assert.True(consentDetails.CreatedOn != null, $"Created On under consent details field is null in Customer Document actual {consentDetails.CreatedOn}");
            Assert.True(consentDetails.LastUpdatedOn != null, $"LastUpdatedOn field under consent details is null in Customer Document actual {consentDetails.LastUpdatedOn}");
            Assert.True(consentDetails.LastUpdatedOn.Equals(consentDetails.CreatedOn), $"LastUpdatedOn & Created On dates are not same under consent details in Customer Document expected{consentDetails.CreatedOn}");

            string strInputQueryForCustomerSearchDoc = $"SELECT * FROM CustomerCollection c WHERE c.DocType='CustomerSearch' and c.LoyaltyMemberNumber = '{enrolmentResult["memberdetails"]["memberNumber"]}'";

            CustomerSearch objCustomerSearchCollection = cosmosFixture.QueryItem<CustomerSearch>(strInputQueryForCustomerSearchDoc);

            retry = 5;

            while (retry > 0 && objCustomerSearchCollection == null)
            {
                await Task.Delay(1000);
                objCustomerSearchCollection = cosmosFixture.QueryItem<CustomerSearch>(strInputQueryForCustomerSearchDoc);
                retry--;
            }
            Assert.True(objCustomerSearchCollection != null, "Query from Cosmos DB for Customer collection is null after 5 re tries");
            Assert.True(objCustomerSearchCollection.DateOfBirth.Value.ToString("yyyy-MM-dd").Equals("1997-11-01"), $"Date of Birth is not same in Customer Sedarch Document expected 1997-11-01 actual {objCustomerSearchCollection.DateOfBirth}");
            Assert.True(objCustomerSearchCollection.CustomerId.Equals(enrolmentResult["customerId"].ToString()), $"Customer Id is not same in Customer Search Document expected {enrolmentResult["customerId"]} actual {objCustomerSearchCollection.CustomerId}");
            Assert.True(objCustomerSearchCollection.FirstName.Equals(firstName), $"First name is not same in Customer Search Document  expected {firstName} actual {objCustomerSearchCollection.FirstName}");
            Assert.True(objCustomerSearchCollection.Gender.Equals("M"), $"Gender is not same in Customer Search Document expected M actual {objCustomerSearchCollection.Gender}");
            Assert.True(objCustomerSearchCollection.LastName.Equals(lastName), $"Last name is not same in Customer Search Document expected {lastName} expected {objCustomerSearchCollection.LastName}");
            if (channelID.Equals("POWERAPP"))
            {
                Assert.True(objCustomerSearchCollection.Title.Equals("Mr"), $"Title is not same in Customer Search Document expected Mr but found {objCustomer.Title}");
            }
            Assert.True(objCustomerSearchCollection.EmailAddress.Equals(emailId), $"Email id is not matched in Customer Search Document expected {emailId} actual {objCustomerSearchCollection.EmailAddress}");
            Assert.True(objCustomerSearchCollection.LoyaltyMemberNumber.Equals(enrolmentResult["memberdetails"]["memberNumber"].ToString()), $"FC no is not matched in Customer Search Document expected {enrolmentResult["memberdetails"]["memberNumber"]} actual {objCustomerSearchCollection.LoyaltyMemberNumber}");
            Assert.True(objCustomerSearchCollection.PostCode.Equals(postCode), $"Post code field is not matched in Customer Search Document expected {postCode} actual {objCustomerSearchCollection.PostCode}");
            Assert.True(objCustomerSearchCollection.Country.Equals("United Kingdom"), $"Country field is not matched in Customer Search Document expected United Kingdom actual {objCustomerSearchCollection.Country}");
        }

        [Theory]
        [Trait("Category", "Enrolment-Regression")]
        [InlineData("POWERAPP")]
        [InlineData("DELTA.COM")]
        public async Task ValidateEnrolmentDataInCustomerTransactionDoc(string channelID)
        {
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string emailId = firstName.ToLower() + lastName.ToLower() + "@example.com";
            string postCode = "SO14 0LT";
            string country = "GB";

            System.Net.Http.HttpResponseMessage enrolmentResponse = Requests.Endpoint.Microservice.Enrolment(env, tokenFixture.GetToken("enrolment"), channelID, firstName, lastName, emailId, postCode, country, output);

            Assert.True(enrolmentResponse.StatusCode == HttpStatusCode.Accepted, $"202 Accepted was not returned from enrolment, response was: {enrolmentResponse.ReadResponseAsString()}");

            JObject enrolmentResult = (JObject)JsonConvert.DeserializeObject<dynamic>(await enrolmentResponse.Content.ReadAsStringAsync());

            Assert.True(enrolmentResult["customerId"] != null && !enrolmentResult["customerId"].ToString().Equals(string.Empty), "customer id should be not blank");

            string strInputQueryForFlyingClubDoc = $"SELECT * FROM CustomerDetailsCollection c WHERE c.PartitionKey = '{enrolmentResult["customerId"]}' and c.DocType='FlyingClubMember'";

            FlyingClubMember objFcmDoc = cosmosFixture.QueryItem<FlyingClubMember>(strInputQueryForFlyingClubDoc);

            int retry = 5;
            while (retry > 0 && objFcmDoc == null)
            {
                await Task.Delay(1000);
                objFcmDoc = cosmosFixture.QueryItem<FlyingClubMember>(strInputQueryForFlyingClubDoc);
                retry--;
            }
            MemberAttributes memberAttributes = objFcmDoc.MemberAttributes[0];
            Assert.True(objFcmDoc != null, "Query from Cosmos DB for Flying Club member is null after 5 re tries");
            Assert.True(memberAttributes.createdBy.Equals(memberAttributes.lastUpdatedBy), "Created By & Lastupdated by is not same under member attributes in Flying club member Document");
            Assert.True(memberAttributes.createdOn != null, "Created On field under Member attributes is null in Flying Club member Document");
            Assert.True(memberAttributes.lastUpdatedOn != null, "LastUpdatedOn field under Member attributes is null in Flying Club member Document");
            Assert.True(memberAttributes.lastUpdatedOn.Equals(memberAttributes.createdOn), $"LastUpdatedOn & Created On dates are not same under Member attributes is null in Flying Club member Document expected{memberAttributes.createdOn}");
            Assert.True(objFcmDoc.Brand.Equals("VAA"), $"Brand value is not showing in Flying Club member Document, expected VAA actual {objFcmDoc.Brand}");
            Assert.True(objFcmDoc.Id.Equals(enrolmentResult["memberdetails"]["memberNumber"].ToString()), $"Id value is not same with member no in Flying Club member Document, expected {enrolmentResult["memberdetails"]["memberNumber"]} actual {objFcmDoc.Id}");
            Assert.True(objFcmDoc.PartitionKey.Equals(objFcmDoc.CustomerId), $"Partion key & CustomerId are not same expected {enrolmentResult["customerId"]} actual {objFcmDoc.CustomerId} & partion key {objFcmDoc.PartitionKey}");
            Assert.True(objFcmDoc.CreatedBy.Equals(objFcmDoc.LastUpdatedBy), "Created By & Lastupdated by is not same under Flying Club member Document");
            Assert.True(objFcmDoc.CreatedOn != null, "Created On field under Flying Club member Document is null");
            Assert.True(objFcmDoc.LastUpdatedOn != null, "LastUpdatedOn field under Flying Club member Document is null");
            Assert.True(objFcmDoc.LastUpdatedOn.Equals(objFcmDoc.CreatedOn), $"LastUpdatedOn & Created On dates are not same under Flying Club member Document expected{objFcmDoc.CreatedOn}");
            Assert.True(objFcmDoc.MemberNumber.Equals(enrolmentResult["memberdetails"]["memberNumber"].ToString()), $"Member no is not same in Flying Club member doc, expected {enrolmentResult["memberdetails"]["memberNumber"]}, actual {objFcmDoc.MemberNumber}");
            Assert.True(objFcmDoc.TierCode.Equals("Red"), $"Tier code is worng in Flying Club member doc , expected Red , actual {objFcmDoc.TierCode}");
            Assert.True(objFcmDoc.Status.Equals("Active"), $"Status is worng in Flying Club member doc , expected Active , actual {objFcmDoc.Status}");
            Assert.True(objFcmDoc.MediaCode.Equals("A50"), $"Media code is worng in Flying Club member doc , expected A50 , actual {objFcmDoc.MediaCode}");
            Assert.True(objFcmDoc.TestMemberFlag.Equals("N"), $"Test member flag is worng in Flying Club member doc , expected N , actual {objFcmDoc.TestMemberFlag}");
            Assert.True(objFcmDoc.TermsAndConditionsFlag.Equals("Y"), $"Terms and conditoin is worng in Flying Club member doc , expected Y , actual {objFcmDoc.TermsAndConditionsFlag}");
            Assert.True(objFcmDoc.NameOnCard.Equals(firstName + " " + lastName), $"Name on card is wrong in Flying Club member doc expected {firstName + " " + lastName} actual {objFcmDoc.NameOnCard}");
            Assert.True(objFcmDoc.MemberShipCardDetails.CardNumber.Equals(objFcmDoc.MemberNumber), $"Card number is nor same with the FC No expected {objFcmDoc.MemberNumber}, actual {objFcmDoc.MemberShipCardDetails.CardID}");
            Assert.True(objFcmDoc.MemberShipCardDetails.Active.Equals("Y"), $"Active value under Membership Card details is wrong expected Y, actual {objFcmDoc.MemberShipCardDetails.Active}");
            Assert.True(objFcmDoc.MemberShipCardDetails.CardType.Equals(objFcmDoc.TierCode), $"Card type is wrong expected {objFcmDoc.TierCode}, actual {objFcmDoc.MemberShipCardDetails.Active}");
            Assert.True(objFcmDoc.MemberShipCardDetails.IssueDate.Equals(objFcmDoc.MemberShipCardDetails.StartDate), $"Start date & issue date are not same under Membership Card details start date{objFcmDoc.MemberShipCardDetails.StartDate} & issue date {objFcmDoc.MemberShipCardDetails.IssueDate}");

            string strInputQueryForPartnerMembershipDoc = $"SELECT * FROM CustomerDetailsCollection c WHERE c.PartitionKey = '{enrolmentResult["customerId"]}' and c.DocType='PartnerMembership'";

            PartnerMembership objPartnerMembershipDoc = cosmosFixture.QueryItem<PartnerMembership>(strInputQueryForPartnerMembershipDoc);

            retry = 5;
            while (retry > 0 && objPartnerMembershipDoc == null)
            {
                await Task.Delay(1000);
                objPartnerMembershipDoc = cosmosFixture.QueryItem<PartnerMembership>(strInputQueryForPartnerMembershipDoc);
                retry--;
            }
            Assert.True(objPartnerMembershipDoc != null, "Query from Cosmos DB for Partner Membership is null after 5 re tries");
            Assert.True(objPartnerMembershipDoc.CustomerId.Equals(objPartnerMembershipDoc.PartitionKey), $"Customer Id & Partition Key is not same in Partner membership doc customer id is {objPartnerMembershipDoc.CustomerId} & partition key is {objPartnerMembershipDoc.PartitionKey}");
            Assert.True(objPartnerMembershipDoc.StartDate.Equals(objPartnerMembershipDoc.CreatedOn), $"Start date & Created on is not same in Partner membership doc start date is {objPartnerMembershipDoc.StartDate} & partition key is {objPartnerMembershipDoc.CreatedOn}");
            Assert.True(objPartnerMembershipDoc.Brand.Equals("VAA"), $"Brand value is not showing in Partner membership Document, expected VAA actual {objPartnerMembershipDoc.Brand}");
            Assert.True(objPartnerMembershipDoc.CreatedBy.Equals(objPartnerMembershipDoc.LastUpdatedBy), "Created By & Lastupdated by is not same under Partner membership Document");
            Assert.True(objPartnerMembershipDoc.CreatedOn != null, "Created On field under Partner membership Document is null");
            Assert.True(objPartnerMembershipDoc.LastUpdatedOn != null, "LastUpdatedOn field under Partner membership Document is null");
            Assert.True(objPartnerMembershipDoc.LastUpdatedOn.Equals(objPartnerMembershipDoc.CreatedOn), $"LastUpdatedOn & Created On dates are not same under Partner membership Document expected{objPartnerMembershipDoc.CreatedOn}");
            Assert.True(objPartnerMembershipDoc.MemberNumber.Equals(enrolmentResult["memberdetails"]["memberNumber"].ToString()), $"Member no is not same in Partner membership document, expected {enrolmentResult["memberdetails"]["memberNumber"]}, actual {objPartnerMembershipDoc.MemberNumber}");

            string strInputQueryForMembershipChangeDoc = $"SELECT * FROM CustomerDetailsCollection c WHERE c.PartitionKey = '{enrolmentResult["customerId"]}' and c.DocType='MembershipChange'";

            MembershipChange objMembershipChangeDoc = cosmosFixture.QueryItem<MembershipChange>(strInputQueryForMembershipChangeDoc);

            retry = 5;
            while (retry > 0 && objMembershipChangeDoc == null)
            {
                await Task.Delay(1000);
                objMembershipChangeDoc = cosmosFixture.QueryItem<MembershipChange>(strInputQueryForMembershipChangeDoc);
                retry--;
            }

            Assert.True(objMembershipChangeDoc != null, "Query from Cosmos DB for Membership Change is null after 5 re tries");
            Assert.True(objMembershipChangeDoc.CustomerId.Equals(objMembershipChangeDoc.PartitionKey), $"Customer Id & Partition Key is not same in Membership change doc customer id is {objMembershipChangeDoc.CustomerId} & partition key is {objMembershipChangeDoc.PartitionKey}");
            Assert.True(objMembershipChangeDoc.Brand.Equals("VAA"), $"Brand value is not showing in Membership change Document, expected VAA actual {objMembershipChangeDoc.Brand}");
            Assert.True(objMembershipChangeDoc.CreatedBy.Equals(objMembershipChangeDoc.LastUpdatedBy), "Created By & Lastupdated by is not same under Membership change Document");
            Assert.True(objMembershipChangeDoc.CreatedOn != null, "Created On field under Membership change Document is null");
            Assert.True(objMembershipChangeDoc.LastUpdatedOn != null, "LastUpdatedOn field under Membership change Document is null");
            Assert.True(objMembershipChangeDoc.LastUpdatedOn.Equals(objMembershipChangeDoc.CreatedOn), $"LastUpdatedOn & Created On dates are not same under Membership change Document expected{objMembershipChangeDoc.CreatedOn}");
            Assert.True(objMembershipChangeDoc.MemberNumber.Equals(enrolmentResult["memberdetails"]["memberNumber"].ToString()), $"Member no is not same in Membership change document, expected {enrolmentResult["memberdetails"]["memberNumber"]}, actual {objMembershipChangeDoc.MemberNumber}");
            Assert.True(objMembershipChangeDoc.ApprovalDate.Equals(objMembershipChangeDoc.CreatedOn), $"Approval date & Created on date is not same, approval date {objMembershipChangeDoc.ApprovalDate}, created on {objMembershipChangeDoc.CreatedOn}");

            string strInputQueryForCustomerProfileDoc = $"SELECT * FROM CustomerDetailsCollection c WHERE c.PartitionKey = '{enrolmentResult["customerId"]}' and c.DocType='CustomerProfile'";

            CustomerProfile objCustomerProfileDoc = cosmosFixture.QueryItem<CustomerProfile>(strInputQueryForCustomerProfileDoc);

            retry = 5;
            while (retry > 0 && objCustomerProfileDoc == null)
            {
                await Task.Delay(1000);
                objCustomerProfileDoc = cosmosFixture.QueryItem<CustomerProfile>(strInputQueryForCustomerProfileDoc);
                retry--;
            }

            Assert.True(objCustomerProfileDoc != null, "Query from Cosmos DB for Customer Profile is null after 5 re tries");
            Assert.True(objCustomerProfileDoc.CustomerId.Equals(objCustomerProfileDoc.PartitionKey), $"Customer Id & Partition Key is not same in Customer Profile doc customer id is {objCustomerProfileDoc.CustomerId} & partition key is {objCustomerProfileDoc.PartitionKey}");
            Assert.True(objCustomerProfileDoc.Brand.Equals("VAA"), $"Brand value is not showing in Customer Profile Document, expected VAA actual {objCustomerProfileDoc.Brand}");
            Assert.True(objCustomerProfileDoc.CreatedBy.Equals(objCustomerProfileDoc.LastUpdatedBy), "Created By & Lastupdated by is not same under Customer Profile document");
            Assert.True(objCustomerProfileDoc.CreatedOn != null, "Created On field under Customer Profile document is null");
            Assert.True(objCustomerProfileDoc.LastUpdatedOn != null, "LastUpdatedOn field under Customer Profile document is null");
            Assert.True(objCustomerProfileDoc.LastUpdatedOn.Equals(objCustomerProfileDoc.CreatedOn), $"LastUpdatedOn & Created On dates are not same under Customer Profile document Created on time{objCustomerProfileDoc.CreatedOn}, last updated on time {objCustomerProfileDoc.LastUpdatedOn}");
            IEnumerable<Preference> preferences = objCustomerProfileDoc.Preferences;
            Preference partnerMarketingPreference = preferences.FirstOrDefault(c => c.PreferenceType.Equals("VAPartnersMarketingPermissions"));
            Preference marketingPreference = preferences.FirstOrDefault(c => c.PreferenceType.Equals("VAAMarketingPermissions"));
            Assert.True(partnerMarketingPreference.CreatedBy.Equals(partnerMarketingPreference.LastUpdatedBy), "Created By & Lastupdated by are not same under Partner preference");
            Assert.True(partnerMarketingPreference.CreatedOn != null, "Created On field under Partner preference is null");
            Assert.True(partnerMarketingPreference.LastUpdatedOn != null, "LastUpdatedOn field under Partner preference is null");
            Assert.True(partnerMarketingPreference.LastUpdatedOn.Equals(partnerMarketingPreference.CreatedOn), $"LastUpdatedOn & Created On dates are not same under Partner preference expected{partnerMarketingPreference.CreatedOn}");
            Assert.True(partnerMarketingPreference.Active.Equals("Y"), $"Active value is wrong under Partner Marketing preference expected Y, actual {partnerMarketingPreference.Active}");
            Assert.True(partnerMarketingPreference.PreferenceValue.Equals("Y"), $"Preference value is wrong under Partner Marketing preference expected Y, actual {partnerMarketingPreference.PreferenceValue}");
            Assert.True(partnerMarketingPreference.PreferenceId != null, $"Prefernce id is null under Partner Marketing preference");
            Assert.True(marketingPreference.CreatedBy.Equals(marketingPreference.LastUpdatedBy), "Created By & Lastupdated by are not same under Marketing preference");
            Assert.True(marketingPreference.CreatedOn != null, "Created On field under Marketing preference is null");
            Assert.True(marketingPreference.LastUpdatedOn != null, "LastUpdatedOn field under Marketing preference is null");
            Assert.True(marketingPreference.LastUpdatedOn.Equals(marketingPreference.CreatedOn), $"LastUpdatedOn & Created On dates are not same under Marketing preference expected{marketingPreference.CreatedOn}");
            Assert.True(marketingPreference.Active.Equals("Y"), $"Active value is wrong under Marketing preference expected Y, actual {marketingPreference.Active}");
            Assert.True(marketingPreference.PreferenceValue.Equals("Y"), $"Preference value is wrong under Marketing preference expected Y, actual {marketingPreference.PreferenceValue}");
            Assert.True(marketingPreference.PreferenceId != null, $"Prefernce id is null under Marketing preference");
        }
    }
}
