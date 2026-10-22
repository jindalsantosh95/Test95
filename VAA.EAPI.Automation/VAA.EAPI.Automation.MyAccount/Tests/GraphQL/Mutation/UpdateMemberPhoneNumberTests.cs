using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MyAccount.Constants;
using VAA.EAPI.Automation.MyAccount.Helpers;
using VAA.EAPI.Automation.MyAccount.Requests.Endpoint;
using VAA.EAPI.Automation.MyAccount.Requests.Models.GraphQL.Mutation;

namespace VAA.EAPI.Automation.MyAccount.Tests.GraphQlTests.Mutation
{
    [Trait("Category", Traits.IdentityMSRelatedGraphqltests)]
    [Trait("Category", Traits.CustomerMSRelatedGraphqltests)]
    public class UpdateMemberPhoneNumberTests : IClassFixture<CosmosFixture>
    {
        private readonly CosmosFixture cosmosFixture;
        private readonly string env = Config.Environment;

        public UpdateMemberPhoneNumberTests(CosmosFixture cosmosFixture)
        {
            this.cosmosFixture = cosmosFixture;

            string customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;

            // Register the active customer and history documents used to validate phone updates.
            this.cosmosFixture.RegisterClient(Application.CustomerCosmosDbEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));
            this.cosmosFixture.RegisterType<Customer>(Application.CustomerCosmosDbEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
            this.cosmosFixture.RegisterType<ContactHistory>(Application.CustomerCosmosDbEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
        }

        [Theory]
        [InlineData("UK +44", "44", "7")]
        [InlineData("US +1", "1", "2")]
        [InlineData("IN +91", "91", "9")]
        [Trait("Category", Traits.MemberAccountDetailsGraphQl)]
        [Trait("Category", Traits.MyAccountGQLRegressionTests)]
        public async Task TC01_UpdatesMemberPhoneNumberForUKUSAndIN(string countryDescription, string countryCode, string phonePrefix)
        {
            (string customerId, string memberNumber) = await EnrolmentHelper.GraphQLEnrolment();
            QueryDefinition customerQuery = new QueryDefinition("SELECT TOP 1 * FROM CustomerCollection c WHERE c.PartitionKey = @customerId AND c.DocType = @docType")
                .WithParameter("@customerId", customerId)
                .WithParameter("@docType", Application.DocTypeCustomer);
            Customer? customerBeforeUpdate = null;

            // Capture the current phone details so the replacement and history documents can be verified.
            RetryHelper.RunWithRetry(() =>
            {
                customerBeforeUpdate = cosmosFixture.CosmosQueryItem<Customer>(customerQuery);
                Assert.True(customerBeforeUpdate is not null, $"Member {memberNumber}: Customer document was not found for customer {customerId} after enrolment.");
            }, maxRetries: 5, delay: 1000);

            // Assert that enrolment supplied the customer data required by this scenario.
            Assert.True(customerBeforeUpdate is not null, $"Member {memberNumber}: Customer document was not available after enrolment.");
            Customer originalCustomer = customerBeforeUpdate ?? new Customer();
            ContactDetails? phoneDetailsBeforeUpdate = originalCustomer.ContactDetails?.FirstOrDefault(contactDetail => contactDetail.ContactType.Is("Phone") && contactDetail.Primary.Is("Y"));
            Assert.True(phoneDetailsBeforeUpdate is not null, $"Member {memberNumber}: Customer document did not contain a primary phone contact after enrolment.");
            ContactDetails originalPhoneDetails = phoneDetailsBeforeUpdate ?? new ContactDetails();
            string token = await EntraTokenHelper.GetIdToken(env, string.Empty, memberNumber, Application.LoginPassword);
            string phoneNumber = $"{phonePrefix}{Random.Shared.NextInt64(100000000, 999999999)}";

            while (phoneNumber.Is(originalPhoneDetails.ContactValue))
                phoneNumber = $"{phonePrefix}{Random.Shared.NextInt64(100000000, 999999999)}";

            // Verify each country-code scenario provides a valid ten-digit phone number.
            Assert.Equal(10, phoneNumber.Length);

            UpdateMemberPhoneNumber request = new()
            {
                UpdatePhoneNumber = new UpdatePhoneNumberInput
                {
                    CountryCode = countryCode,
                    PhoneNumber = phoneNumber
                }
            };

            // GraphQL can report errors even on HTTP 200, so validate the envelope first.
            using HttpResponseMessage response = GraphQL.UpdateMemberPhoneNumber(env, token, request).EnsureGQLResponseSuccess();
            JObject responseBody = JObject.Parse(await response.Content.ReadAsStringAsync());

            Assert.Equal("Updated Phone Number Successfully", responseBody.RequireToken("data.updateMemberPhoneNumber").ToString());

            Customer? customerAfterUpdate = null;

            // Poll until the Customer service has persisted the asynchronous phone update.
            RetryHelper.RunWithRetry(() =>
            {
                customerAfterUpdate = cosmosFixture.CosmosQueryItem<Customer>(customerQuery);
                ContactDetails? phoneDetails = customerAfterUpdate?.ContactDetails?.FirstOrDefault(contactDetail => contactDetail.ContactType.Is("Phone") && contactDetail.Primary.Is("Y"));
                Assert.True(phoneDetails?.ContactValue.Is(phoneNumber) == true, $"Member {memberNumber}: primary phone number was not updated in Cosmos for customer {customerId}.");
            }, maxRetries: 5, delay: 1000);

            Assert.True(customerAfterUpdate is not null, $"Member {memberNumber}: Customer document was not available after the phone update.");
            Customer updatedCustomer = customerAfterUpdate ?? new Customer();
            ContactDetails? updatedPhoneDetails = updatedCustomer.ContactDetails?.FirstOrDefault(contactDetail => contactDetail.ContactType.Is("Phone") && contactDetail.Primary.Is("Y"));
            Assert.True(updatedPhoneDetails is not null, $"Member {memberNumber}: Customer document did not contain a primary phone contact after the phone update.");
            ContactDetails updatedPrimaryPhone = updatedPhoneDetails ?? new ContactDetails();
            SoftAssertHelper softAssert = new();

            // Validate the primary Customer contact and audit fields set by the Customer update handler.
            softAssert.True(updatedPrimaryPhone.ContactValue.Is(phoneNumber), $"Member {memberNumber}: primary phone number was not updated.");
            softAssert.True(updatedPrimaryPhone.CountryCode.Is(countryCode), $"Member {memberNumber}: primary phone country code was not updated for {countryDescription}.");
            softAssert.True(updatedPrimaryPhone.Primary.Is("Y"), $"Member {memberNumber}: updated phone contact is not primary.");
            softAssert.True(updatedPrimaryPhone.LastUpdatedBy.Is(Application.MyAccountChannelId), $"Member {memberNumber}: phone LastUpdatedBy was not {Application.MyAccountChannelId}.");
            softAssert.True(updatedPrimaryPhone.LastUpdatedOn.HasValue, $"Member {memberNumber}: phone LastUpdatedOn was not populated.");
            softAssert.True(updatedCustomer.LastUpdatedBy.Is(Application.MyAccountChannelId), $"Member {memberNumber}: Customer LastUpdatedBy was not {Application.MyAccountChannelId}.");
            softAssert.True(updatedCustomer.LastUpdatedOn.HasValue, $"Member {memberNumber}: Customer LastUpdatedOn was not populated.");
            softAssert.AssertAll();

            QueryDefinition contactHistoryQuery = new QueryDefinition("SELECT TOP 1 * FROM CustomerCollection c WHERE c.PartitionKey = @customerId AND c.DocType = @docType AND c.ContactType = @contactType AND c.ContactValue = @contactValue ORDER BY c._ts DESC")
                .WithParameter("@customerId", customerId)
                .WithParameter("@docType", "ContactHistory")
                .WithParameter("@contactType", "Phone")
                .WithParameter("@contactValue", originalPhoneDetails.ContactValue);
            ContactHistory? contactHistory = null;

            // The previous phone details must be retained before the current contact is replaced.
            RetryHelper.RunWithRetry(() =>
            {
                contactHistory = cosmosFixture.CosmosQueryItem<ContactHistory>(contactHistoryQuery);
                Assert.True(contactHistory is not null, $"Member {memberNumber}: contact history was not found for customer {customerId} after the phone update.");
            }, maxRetries: 5, delay: 1000);

            Assert.True(contactHistory is not null, $"Member {memberNumber}: contact history was not available after the phone update.");
            ContactHistory previousContactHistory = contactHistory ?? new ContactHistory();

            Assert.True(previousContactHistory.ContactValue.Is(originalPhoneDetails.ContactValue), $"Member {memberNumber}: contact history did not preserve the previous phone number.");
            Assert.True(previousContactHistory.CountryCode.Is(originalPhoneDetails.CountryCode), $"Member {memberNumber}: contact history did not preserve the previous country code.");
        }
    }
}
