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

namespace VAA.EAPI.Automation.MyAccount.Tests.GraphQlTests.Mutation;

[Trait("Category", Traits.IdentityMSRelatedGraphqltests)]
public sealed class AccountUpdateEmailAddressTests : IClassFixture<CosmosFixture>, IClassFixture<TokenFixture>
{
    private readonly CosmosFixture _cosmosFixture;
    private readonly TokenFixture _tokenFixture;
    private readonly string _environment = Config.Environment;

    public AccountUpdateEmailAddressTests(CosmosFixture cosmosFixture, TokenFixture tokenFixture)
    {
        _cosmosFixture = cosmosFixture;
        _tokenFixture = tokenFixture;
        string customerCosmosDb = _environment.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;

        // Register each document updated by the Customer microservice's email-update command.
        _cosmosFixture.RegisterClient(Application.CustomerCosmosDbEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));
        _cosmosFixture.RegisterType<Customer>(Application.CustomerCosmosDbEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
        _cosmosFixture.RegisterType<CustomerSearch>(Application.CustomerCosmosDbEndpoint, customerCosmosDb, Application.CustomerSearchCosmosCollection);
        _cosmosFixture.RegisterType<ContactHistory>(Application.CustomerCosmosDbEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
        _tokenFixture.AddTokenForVRedFromKeyVault("vred");
        _tokenFixture.AddTokenForAzureB2CFromKeyVault("azureb2c");
    }

    [Fact]
    [Trait("Category", Traits.MemberAccountDetailsGraphQl)]
    [Trait("Category", Traits.MyAccountGQLRegressionTests)]
    [Trait("Category", Traits.CustomerMSRelatedGraphqltests)]
    public async Task TC01_UpdatesEmailAcrossCustomerDocumentsVRedAndAzureB2C()
    {
        (string customerId, string memberNumber) = await EnrolmentHelper.GraphQLEnrolment();
        QueryDefinition customerQuery = new QueryDefinition("SELECT TOP 1 * FROM CustomerCollection c WHERE c.PartitionKey = @customerId AND c.DocType = @docType").WithParameter("@customerId", customerId).WithParameter("@docType", Application.DocTypeCustomer);
        QueryDefinition customerSearchQuery = new QueryDefinition("SELECT TOP 1 * FROM CustomerSearch c WHERE c.LoyaltyMemberNumber = @memberNumber AND c.DocType = @docType").WithParameter("@memberNumber", memberNumber).WithParameter("@docType", Application.DocTypeCustomerSearch);
        CustomerSearch? customerSearchBeforeUpdate = null;

        // Capture the outgoing email before the Customer command replaces it and writes ContactHistory.
        RetryHelper.RunWithRetry(() =>
        {
            customerSearchBeforeUpdate = _cosmosFixture.CosmosQueryItem<CustomerSearch>(customerSearchQuery);
            Assert.True(customerSearchBeforeUpdate is not null, $"Member '{memberNumber}': expected a CustomerSearch document after enrolment, but found no document.");
        }, maxRetries: 5, delay: 1000);

        string previousEmailAddress = customerSearchBeforeUpdate!.EmailAddress ?? string.Empty;
        Assert.True(!previousEmailAddress.IsNullOrWhiteSpace(), $"Member '{memberNumber}': expected a populated previous email address, but found '{(previousEmailAddress.IsNullOrWhiteSpace() ? "<empty>" : previousEmailAddress)}'.");
        string updatedEmailAddress = $"emailupdate{Guid.NewGuid():N}@example.com";
        QueryDefinition emailAvailabilityQuery = new QueryDefinition("SELECT TOP 1 * FROM CustomerSearch c WHERE c.EmailAddress = @emailAddress AND c.DocType = @docType").WithParameter("@emailAddress", updatedEmailAddress).WithParameter("@docType", Application.DocTypeCustomerSearch);
        Assert.True(_cosmosFixture.CosmosQueryItem<CustomerSearch>(emailAvailabilityQuery) is null, $"Member '{memberNumber}': expected generated email '{updatedEmailAddress}' to be available in CustomerSearch, but found an existing document.");

        string token = await EntraTokenHelper.GetIdToken(_environment, string.Empty, memberNumber, Application.LoginPassword);
        AccountUpdateEmailAddress request = new() { NewEmailAddress = new UpdateEmailRequestInput { EmailAddress = updatedEmailAddress } };

        // Validate the GraphQL result before checking the Customer projections and external update events.
        using HttpResponseMessage response = GraphQL.AccountUpdateEmailAddress(_environment, token, request).EnsureGQLResponseSuccess();
        JObject responseBody = JObject.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(responseBody.RequireToken("data.accountUpdateEmailAddress").ToString().Is("Email Update Successful"), $"Member '{memberNumber}': expected email-update response 'Email Update Successful', but found '{responseBody.RequireToken("data.accountUpdateEmailAddress")}'.");

        Customer? customerAfterUpdate = null;
        CustomerSearch? customerSearchAfterUpdate = null;

        // Customer and CustomerSearch are persisted together before VRed and Azure B2C events are published.
        RetryHelper.RunWithRetry(() =>
        {
            customerAfterUpdate = _cosmosFixture.CosmosQueryItem<Customer>(customerQuery);
            customerSearchAfterUpdate = _cosmosFixture.CosmosQueryItem<CustomerSearch>(customerSearchQuery);
            ContactDetails? primaryEmail = customerAfterUpdate?.ContactDetails?.FirstOrDefault(contact => contact.ContactType.Is("Email") && contact.Primary.Is("Y"));
            Assert.True(primaryEmail?.ContactValue.Is(updatedEmailAddress) == true && customerSearchAfterUpdate?.EmailAddress.Is(updatedEmailAddress) == true, $"Member '{memberNumber}': expected Customer and CustomerSearch email '{updatedEmailAddress}', but found Customer '{primaryEmail?.ContactValue ?? "<missing>"}' and CustomerSearch '{customerSearchAfterUpdate?.EmailAddress ?? "<missing>"}'.");
        }, maxRetries: 5, delay: 1000);

        Customer updatedCustomer = customerAfterUpdate!;
        CustomerSearch updatedCustomerSearch = customerSearchAfterUpdate!;
        ContactDetails? updatedPrimaryEmail = updatedCustomer.ContactDetails?.FirstOrDefault(contact => contact.ContactType.Is("Email") && contact.Primary.Is("Y"));
        Assert.True(updatedPrimaryEmail is not null, $"Member '{memberNumber}': expected a primary email contact after the update, but found no contact.");

        QueryDefinition contactHistoryQuery = new QueryDefinition("SELECT TOP 1 * FROM CustomerCollection c WHERE c.PartitionKey = @customerId AND c.DocType = @docType AND c.ContactType = @contactType AND c.ContactValue = @contactValue ORDER BY c._ts DESC").WithParameter("@customerId", customerId).WithParameter("@docType", "ContactHistory").WithParameter("@contactType", "Email").WithParameter("@contactValue", previousEmailAddress);
        ContactHistory? contactHistory = null;

        // The Customer command saves the previous primary email before external synchronization begins.
        RetryHelper.RunWithRetry(() =>
        {
            contactHistory = _cosmosFixture.CosmosQueryItem<ContactHistory>(contactHistoryQuery);
            Assert.True(contactHistory is not null, $"Member '{memberNumber}': expected ContactHistory for previous email '{previousEmailAddress}', but found no document.");
        }, maxRetries: 5, delay: 1000);

        ContactHistory previousContactHistory = contactHistory!;
        SoftAssertHelper softAssert = new();

        softAssert.True(updatedPrimaryEmail?.ContactValue.Is(updatedEmailAddress) == true, $"Member '{memberNumber}': expected Customer primary email '{updatedEmailAddress}', but found '{updatedPrimaryEmail?.ContactValue ?? "<missing>"}'.");
        softAssert.True(updatedCustomerSearch.EmailAddress.Is(updatedEmailAddress), $"Member '{memberNumber}': expected CustomerSearch email '{updatedEmailAddress}', but found '{updatedCustomerSearch.EmailAddress}'.");
        softAssert.True(previousContactHistory.ContactValue.Is(previousEmailAddress), $"Member '{memberNumber}': expected ContactHistory email '{previousEmailAddress}', but found '{previousContactHistory.ContactValue}'.");
        softAssert.True(previousContactHistory.ContactType.Is("Email"), $"Member '{memberNumber}': expected ContactHistory contact type 'Email', but found '{previousContactHistory.ContactType}'.");
        softAssert.AssertAll();

        // VRed receives the email-update event after Customer persistence succeeds.
        await RetryHelper.RunWithRetryAsync(async () =>
        {
            using HttpResponseMessage vRedResponse = Microservice.Vred(_tokenFixture.GetToken("vred"), memberNumber).EnsureIsSuccessStatusCode();
            JObject vRedResult = JObject.Parse(await vRedResponse.Content.ReadAsStringAsync());
            Assert.True(vRedResult.RequireToken("email").ToString().Is(updatedEmailAddress), $"Member '{memberNumber}': expected VRed email '{updatedEmailAddress}', but found '{vRedResult.RequireToken("email")}'.");
        }, 10, 5000);

        // Azure B2C exposes the new email as the member's sign-in identity.
        await RetryHelper.RunWithRetryAsync(async () =>
        {
            using HttpResponseMessage azureB2CResponse = Microservice.AzureB2C(_tokenFixture.GetToken("azureb2c"), memberNumber).EnsureIsSuccessStatusCode();
            JObject azureB2CResult = JObject.Parse(await azureB2CResponse.Content.ReadAsStringAsync());
            JArray users = azureB2CResult.RequireToken("value") as JArray ?? new JArray();
            JObject? member = users.FirstOrDefault() as JObject;
            string identityEmailAddress = member?.SelectTokens("identities[*]")
                .FirstOrDefault(identity => identity.RequireToken("signInType").ToString().Is("emailAddress"))?
                .RequireToken("issuerAssignedId").ToString() ?? string.Empty;

            Assert.True(users.Count > 0, $"Member '{memberNumber}': expected at least one Azure B2C user after the email update, but found '{users.Count}'.");
            Assert.True(identityEmailAddress.Is(updatedEmailAddress), $"Member '{memberNumber}': expected Azure B2C email identity '{updatedEmailAddress}', but found '{identityEmailAddress}'.");
        }, 10, 5000);
    }

    [Fact]
    [Trait("Category", Traits.MemberAccountDetailsGraphQl)]
    [Trait("Category", Traits.MyAccountGQLRegressionTests)]
    public async Task TC02_RejectsBlacklistedEmailAddress()
    {
        string memberNumber = (await EnrolmentHelper.GraphQLEnrolment()).Item2;
        string token = await EntraTokenHelper.GetIdToken(_environment, string.Empty, memberNumber, Application.LoginPassword);
        AccountUpdateEmailAddress request = new() { NewEmailAddress = new UpdateEmailRequestInput { EmailAddress = "blockmail@testmail.com" } };

        // The resolver validates configured blacklisted emails before invoking Identity or Customer services.
        using HttpResponseMessage response = GraphQL.AccountUpdateEmailAddress(_environment, token, request);
        JObject responseBody = JObject.Parse(await response.Content.ReadAsStringAsync());
        SoftAssertHelper softAssert = new();

        softAssert.True(response.IsSuccessStatusCode, $"Member {memberNumber}: expected HTTP 200 with a GraphQL blacklist error.");
        softAssert.True(responseBody.SelectToken("data.accountUpdateEmailAddress") is null or { Type: JTokenType.Null }, $"Member {memberNumber}: blacklisted email unexpectedly returned data.");
        softAssert.True(responseBody.RequireToken("errors[0].message").ToString().Is("Email BlackListed"), $"Member {memberNumber}: blacklist error message was incorrect.");
        softAssert.True(responseBody.RequireToken("errors[0].extensions.code").ToString().Is("10161b"), $"Member {memberNumber}: blacklist error code was incorrect.");
        softAssert.True(responseBody.RequireToken("errors[0].extensions.message").ToString().Is("Email is blacklisted"), $"Member {memberNumber}: blacklist extension message was incorrect.");
        softAssert.True(responseBody.RequireToken("errors[0].extensions.source").ToString().Is("Consuming application"), $"Member {memberNumber}: blacklist error source was incorrect.");
        softAssert.AssertAll();
    }

    [Theory]
    [InlineData("test")]
    [InlineData("@mail")]
    [InlineData("test@")]
    [InlineData("test#mail.com")]
    [InlineData("test @ mail.com")]
    [InlineData("test  mail.com")]
    [Trait("Category", Traits.MemberAccountDetailsGraphQl)]
    [Trait("Category", Traits.MyAccountGQLRegressionTests)]
    public async Task TC03_RejectsInvalidEmailFormats(string invalidEmailAddress)
    {
        string memberNumber = (await EnrolmentHelper.GraphQLEnrolment()).Item2;
        string token = await EntraTokenHelper.GetIdToken(_environment, string.Empty, memberNumber, Application.LoginPassword);
        AccountUpdateEmailAddress request = new() { NewEmailAddress = new UpdateEmailRequestInput { EmailAddress = invalidEmailAddress } };

        // Identity validates email syntax and returns its configured 10161d contract through GraphQL.
        using HttpResponseMessage response = GraphQL.AccountUpdateEmailAddress(_environment, token, request);
        JObject responseBody = JObject.Parse(await response.Content.ReadAsStringAsync());
        SoftAssertHelper softAssert = new();

        softAssert.True(response.IsSuccessStatusCode, $"Member {memberNumber}: expected HTTP 200 with an invalid-email GraphQL error.");
        softAssert.True(responseBody.SelectToken("data.accountUpdateEmailAddress") is null or { Type: JTokenType.Null }, $"Member {memberNumber}: invalid email {invalidEmailAddress} unexpectedly returned data.");
        softAssert.True(responseBody.RequireToken("errors[0].message").ToString().Is("Email Update failed"), $"Member {memberNumber}: invalid-email GraphQL message was incorrect.");
        softAssert.True(responseBody.RequireToken("errors[0].extensions.code").ToString().Is("10161d"), $"Member {memberNumber}: invalid-email error code was incorrect.");
        softAssert.True(responseBody.RequireToken("errors[0].extensions.message").ToString().Is("Provided email is in invalid format"), $"Member {memberNumber}: invalid-email extension message was incorrect.");
        softAssert.True(responseBody.RequireToken("errors[0].extensions.source").ToString().Is("Consuming application"), $"Member {memberNumber}: invalid-email error source was incorrect.");
        softAssert.AssertAll();
    }

    [Fact]
    [Trait("Category", Traits.MemberAccountDetailsGraphQl)]
    [Trait("Category", Traits.MyAccountGQLRegressionTests)]
    public async Task TC04_RejectsEmailUsedByAnotherAzureB2CMember()
    {
        string firstMemberNumber = (await EnrolmentHelper.GraphQLEnrolment()).Item2;
        string secondMemberNumber = (await EnrolmentHelper.GraphQLEnrolment()).Item2;
        QueryDefinition secondMemberQuery = new QueryDefinition("SELECT TOP 1 * FROM CustomerSearch c WHERE c.LoyaltyMemberNumber = @memberNumber AND c.DocType = @docType").WithParameter("@memberNumber", secondMemberNumber).WithParameter("@docType", Application.DocTypeCustomerSearch);
        CustomerSearch? secondMemberCustomerSearch = null;

        // Retrieve the second member's enrolled email so the request reaches Identity's duplicate-account validation.
        RetryHelper.RunWithRetry(() =>
        {
            secondMemberCustomerSearch = _cosmosFixture.CosmosQueryItem<CustomerSearch>(secondMemberQuery);
            Assert.True(secondMemberCustomerSearch is not null, $"Member {secondMemberNumber}: CustomerSearch document was not found after enrolment.");
        }, maxRetries: 5, delay: 1000);

        string duplicateEmailAddress = secondMemberCustomerSearch!.EmailAddress ?? string.Empty;
        Assert.True(!duplicateEmailAddress.IsNullOrWhiteSpace(), $"Member {secondMemberNumber}: CustomerSearch did not contain an email for duplicate validation.");
        string token = await EntraTokenHelper.GetIdToken(_environment, string.Empty, firstMemberNumber, Application.LoginPassword);
        AccountUpdateEmailAddress request = new() { NewEmailAddress = new UpdateEmailRequestInput { EmailAddress = duplicateEmailAddress } };

        // Identity rejects an email owned by another B2C account before the Customer service receives the update.
        using HttpResponseMessage response = GraphQL.AccountUpdateEmailAddress(_environment, token, request);
        JObject responseBody = JObject.Parse(await response.Content.ReadAsStringAsync());
        SoftAssertHelper softAssert = new();

        softAssert.True(response.IsSuccessStatusCode, $"Member {firstMemberNumber}: expected HTTP 200 with a duplicate-email error for email owned by member {secondMemberNumber}.");
        softAssert.True(responseBody.SelectToken("data.accountUpdateEmailAddress") is null or { Type: JTokenType.Null }, $"Member {firstMemberNumber}: duplicate email unexpectedly returned data.");
        softAssert.True(responseBody.RequireToken("errors[0].message").ToString().Is("Email Update failed"), $"Member {firstMemberNumber}: duplicate-email GraphQL message was incorrect.");
        softAssert.True(responseBody.RequireToken("errors[0].extensions.code").ToString().Is("10161a"), $"Member {firstMemberNumber}: duplicate-email error code was incorrect.");
        softAssert.True(responseBody.RequireToken("errors[0].extensions.message").ToString().Is("Provided email already exists in B2C with other Account"), $"Member {firstMemberNumber}: duplicate-email extension message was incorrect.");
        softAssert.True(responseBody.RequireToken("errors[0].extensions.source").ToString().Is("Consuming application"), $"Member {firstMemberNumber}: duplicate-email error source was incorrect.");
        softAssert.AssertAll();
    }
}
