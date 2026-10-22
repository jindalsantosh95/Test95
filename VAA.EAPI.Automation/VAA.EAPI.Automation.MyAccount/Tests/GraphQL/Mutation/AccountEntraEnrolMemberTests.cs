using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MyAccount.Constants;
using VAA.EAPI.Automation.MyAccount.Helpers;
using VAA.EAPI.Automation.MyAccount.Models.Enrolment;
using VAA.EAPI.Automation.MyAccount.Requests.Endpoint;
using VAA.EAPI.Automation.MyAccount.Requests.Models.GraphQL.Mutation;
using VAA.EAPI.Infrastructure.AzureStorage;

namespace VAA.EAPI.Automation.MyAccount.Tests.GraphQlTests.Mutation;

public sealed class AccountEntraEnrolMemberTests : IClassFixture<CosmosFixture>, IClassFixture<TokenFixture>, IClassFixture<CertificateFixture>
{
    private const string DefaultEnrollmentChannel = "WEB-MYA";
    private const string MediaCode = "Web";
    private const string CountryCode = "GB";
    private const string PhoneCountryCode = "44";
    private const string PostalCode = "RH117DU";
    private const string DeltaSoaCertificateKey = "Delta-SOA-Certificate";
    private const string InvalidPasswordOtpErrorCode = "9002313";
    private const string InvalidPasswordOtpErrorMessage = "Account member password otp validation failed";
    private const string DisallowedPasswordErrorCode = "399249";
    private const string DisallowedPasswordErrorMessage = "New password is on a not allowed list";
    private const string EntraApiErrorSource = "Entra Api";
    private const string UnderEighteenEnrolmentErrorCode = "1027";
    private const string UnderEighteenEnrolmentErrorMessage = "A parent or legal guardian must create their account through the parent’s Flying Club profile.";
    private const string UnderEighteenEnrolmentErrorSource = "Cosuming application";
    private const string UnderEighteenEnrolmentNextAction = "loginToCreateYoungFlyer";
    private static readonly string env = Config.Environment;
    private readonly CosmosFixture cosmosFixture;
    private readonly TokenFixture tokenFixture;
    private readonly CertificateFixture certificateFixture;
    private readonly string customerCosmosDb;
    private readonly string customerTransactionCosmosDb;

    public AccountEntraEnrolMemberTests(CosmosFixture cosmosFixture, TokenFixture tokenFixture, CertificateFixture certificateFixture)
    {
        this.cosmosFixture = cosmosFixture;
        this.tokenFixture = tokenFixture;
        this.certificateFixture = certificateFixture;
        customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
        customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

        // Register all enrolment documents so the test verifies the complete asynchronous Cosmos outcome.
        this.cosmosFixture.RegisterClient(Application.CustomerCosmosDbEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));
        this.cosmosFixture.RegisterType<Customer>(Application.CustomerCosmosDbEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
        this.cosmosFixture.RegisterType<CustomerSearch>(Application.CustomerCosmosDbEndpoint, customerCosmosDb, Application.CustomerSearchCosmosCollection);
        this.cosmosFixture.RegisterClient(Application.CustomerTransactionCosmosDbEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));
        this.cosmosFixture.RegisterType<FlyingClubMember>(Application.CustomerTransactionCosmosDbEndpoint, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
        this.cosmosFixture.RegisterType<CustomerProfile>(Application.CustomerTransactionCosmosDbEndpoint, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
        this.cosmosFixture.RegisterType<MembershipChange>(Application.CustomerTransactionCosmosDbEndpoint, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
        this.tokenFixture.AddTokenForAzureB2CFromKeyVault("azureb2c");
        this.tokenFixture.AddTokenForVRedFromKeyVault("vred");
    }

    [Theory]
    [InlineData("WEB-MYA", "Web")]
    [InlineData("APP-MYA", "App")]
    [Trait("Category", Traits.MyAccountGQLRegressionTests)]
    [Trait("Category", Traits.GqlEntraAuthentication)]
    [Trait("Category", Traits.IdentityMSRelatedGraphqltests)]
    [Trait("Category", Traits.MemberManagementMSRelatedGraphqltests)]
    [Trait("Category", Traits.VRedMSRelatedGraphqltests)]
    [Trait("Category", Traits.CustomerMSRelatedGraphqltests)]
    [Trait("Category", Traits.CustomerTransactionMSRelatedGraphqltests)]
    public async Task TC01_EnrolsMemberAndSynchronizesDownstreamSystems(string enrollmentChannel, string mediaCode)
    {
        string firstName = DataGenerator.RandomFirstName();
        string lastName = DataGenerator.RandomLastName();
        string emailId = $"{firstName.ToLowerInvariant()}{lastName.ToLowerInvariant()}{Random.Shared.NextInt64(100000000000, 999999999999)}@example.com";
        string dateOfBirth = DateTime.UtcNow.Date.AddYears(-30).ToString("yyyy-MM-dd");
        string gender = "M";
        string phoneNumber = Random.Shared.NextInt64(7000000000, 9999999999).ToString();
        TableStorageService<OtpRequestTableEntity> otpRequestTableStorageService = new() { ConnectionString = Vault.Default.GetSecretValue("IdentityStorageAccountConnectionString-sit"), TableName = "OTPRequest" };
        string enrolmentToken = await EnrolmentHelper.ReturnEntraEnrolmentToken(otpRequestTableStorageService, emailId);
        AccountEntraEnrolMemberRequest enrolmentRequest = CreateEnrolmentRequest(enrolmentToken, firstName, lastName, emailId, dateOfBirth, gender, phoneNumber, enrollmentChannel, mediaCode, Application.LoginPassword);

        // Avoid shared diagnostics because the enrolment request contains the configured member password.
        using HttpResponseMessage enrolmentResponse = GraphQL.AccountEntraEnrolMember(env, enrolmentRequest);
        string enrolmentBody = await enrolmentResponse.Content.ReadAsStringAsync();
        JObject enrolmentResult = JObject.Parse(enrolmentBody);
        bool enrolmentHasErrors = enrolmentResult["errors"] is JArray { Count: > 0 };

        Assert.True(enrolmentResponse.IsSuccessStatusCode, $"Email '{emailId}': expected accountEntraEnrolMember HTTP 200, but found HTTP {(int)enrolmentResponse.StatusCode}.");
        Assert.True(!enrolmentHasErrors, $"Email '{emailId}': expected no GraphQL errors, but found errors present '{enrolmentHasErrors}'.");

        JToken enrolment = enrolmentResult.RequireToken("data.accountEntraEnrolMember");
        string memberNumber = enrolment.SelectToken("memberNumber")?.ToString() ?? "<missing>";
        string idToken = enrolment.SelectToken("idToken")?.ToString() ?? string.Empty;

        SoftAssertHelper softAssert = new();

        // The GraphQL response returns the created member and a complete Entra sign-in token set.
        softAssert.True(enrolment.SelectToken("firstName")?.ToString().Is(firstName) == true, $"Member '{memberNumber}': expected enrolment first name '{firstName}', but found '{enrolment.SelectToken("firstName")?.ToString() ?? "<missing>"}'.");
        softAssert.True(enrolment.SelectToken("lastName")?.ToString().Is(lastName) == true, $"Member '{memberNumber}': expected enrolment last name '{lastName}', but found '{enrolment.SelectToken("lastName")?.ToString() ?? "<missing>"}'.");
        softAssert.True(!memberNumber.Is("<missing>"), "Enrolment response: expected a populated member number, but found '<missing>'.");
        softAssert.True((enrolment.SelectToken("tokenType")?.ToString() ?? string.Empty).Is("Bearer"), $"Member '{memberNumber}': expected token type 'Bearer', but found '{enrolment.SelectToken("tokenType")?.ToString() ?? "<missing>"}'.");
        softAssert.True(!(enrolment.SelectToken("scope")?.ToString() ?? string.Empty).IsNullOrWhiteSpace(), $"Member '{memberNumber}': expected a populated token scope, but found '{enrolment.SelectToken("scope")?.ToString() ?? "<missing>"}'.");
        softAssert.True((enrolment.SelectToken("expiresIn")?.Value<int>() ?? 0) > 0, $"Member '{memberNumber}': expected token expiry greater than zero, but found '{enrolment.SelectToken("expiresIn")?.ToString() ?? "<missing>"}'.");
        softAssert.True(!(enrolment.SelectToken("accessToken")?.ToString() ?? string.Empty).IsNullOrWhiteSpace(), $"Member '{memberNumber}': expected a populated access token, but found '{(enrolment.SelectToken("accessToken")?.ToString().IsNullOrWhiteSpace() == false ? "<populated>" : "<empty>")}'.");
        softAssert.True(!(enrolment.SelectToken("refreshToken")?.ToString() ?? string.Empty).IsNullOrWhiteSpace(), $"Member '{memberNumber}': expected a populated refresh token, but found '{(enrolment.SelectToken("refreshToken")?.ToString().IsNullOrWhiteSpace() == false ? "<populated>" : "<empty>")}'.");
        softAssert.True(!idToken.IsNullOrWhiteSpace(), $"Member '{memberNumber}': expected a populated ID token, but found '{(idToken.IsNullOrWhiteSpace() ? "<empty>" : "<populated>")}'.");
        softAssert.AssertAll();

        // Use the returned ID token to obtain the customer identifier created by the mutation.
        using HttpResponseMessage memberDetailsResponse = GraphQL.AccountMemberDetails(env, idToken).EnsureGQLResponseSuccess();
        JObject memberDetailsResult = JObject.Parse(await memberDetailsResponse.Content.ReadAsStringAsync());
        string customerId = memberDetailsResult.SelectToken("data.accountMemberDetails.flyingClubMemberDetails.customerID")?.ToString() ?? string.Empty;
        Assert.True(!customerId.IsNullOrWhiteSpace(), $"Member '{memberNumber}': expected a populated customer identifier from accountMemberDetails, but found '{(customerId.IsNullOrWhiteSpace() ? "<empty>" : customerId)}'.");

        QueryDefinition customerQuery = new QueryDefinition("SELECT TOP 1 * FROM CustomerCollection c WHERE c.PartitionKey = @customerId AND c.DocType = @docType").WithParameter("@customerId", customerId).WithParameter("@docType", Application.DocTypeCustomer);
        QueryDefinition customerSearchQuery = new QueryDefinition("SELECT TOP 1 * FROM CustomerSearch c WHERE c.CustomerId = @customerId AND c.DocType = @docType").WithParameter("@customerId", customerId).WithParameter("@docType", Application.DocTypeCustomerSearch);
        QueryDefinition flyingClubMemberQuery = new QueryDefinition("SELECT TOP 1 * FROM CustomerDetailsCollection c WHERE c.PartitionKey = @customerId AND c.DocType = @docType").WithParameter("@customerId", customerId).WithParameter("@docType", Application.DocTypeFlyingClubMember);
        QueryDefinition customerProfileQuery = new QueryDefinition("SELECT TOP 1 * FROM CustomerDetailsCollection c WHERE c.PartitionKey = @customerId AND c.DocType = @docType").WithParameter("@customerId", customerId).WithParameter("@docType", Application.DocTypeCustomerProfile);
        QueryDefinition membershipChangeQuery = new QueryDefinition("SELECT TOP 1 * FROM CustomerDetailsCollection c WHERE c.PartitionKey = @customerId AND c.DocType = @docType").WithParameter("@customerId", customerId).WithParameter("@docType", Application.DocTypeMembershipChange);
        Customer? customerDocument = null;
        CustomerSearch? customerSearchDocument = null;
        FlyingClubMember? flyingClubMemberDocument = null;
        CustomerProfile? customerProfileDocument = null;
        MembershipChange? membershipChangeDocument = null;

        // Wait until the account-creation events persist all five documents before checking their values.
        await RetryHelper.RunWithRetryAsync(() =>
        {
            customerDocument = cosmosFixture.CosmosQueryItem<Customer>(customerQuery);
            customerSearchDocument = cosmosFixture.CosmosQueryItem<CustomerSearch>(customerSearchQuery);
            flyingClubMemberDocument = cosmosFixture.CosmosQueryItem<FlyingClubMember>(flyingClubMemberQuery);
            customerProfileDocument = cosmosFixture.CosmosQueryItem<CustomerProfile>(customerProfileQuery);
            membershipChangeDocument = cosmosFixture.CosmosQueryItem<MembershipChange>(membershipChangeQuery);
            Assert.True(customerDocument is not null, $"Member '{memberNumber}': expected a Customer document, but found no document.");
            Assert.True(customerSearchDocument is not null, $"Member '{memberNumber}': expected a CustomerSearch document, but found no document.");
            Assert.True(flyingClubMemberDocument is not null, $"Member '{memberNumber}': expected a FlyingClubMember document, but found no document.");
            Assert.True(customerProfileDocument is not null, $"Member '{memberNumber}': expected a CustomerProfile document, but found no document.");
            Assert.True(membershipChangeDocument is not null, $"Member '{memberNumber}': expected a MembershipChange document, but found no document.");
            return Task.CompletedTask;
        }, maxRetries: 8, delay: 2000);

        Customer customer = Assert.IsType<Customer>(customerDocument);
        CustomerSearch customerSearch = Assert.IsType<CustomerSearch>(customerSearchDocument);
        FlyingClubMember flyingClubMember = Assert.IsType<FlyingClubMember>(flyingClubMemberDocument);
        CustomerProfile customerProfile = Assert.IsType<CustomerProfile>(customerProfileDocument);
        MembershipChange membershipChange = Assert.IsType<MembershipChange>(membershipChangeDocument);
        ContactDetails? primaryEmail = customer.ContactDetails?.FirstOrDefault(contact => contact.ContactType.Is("Email") && contact.Primary.Is("Y"));
        ContactDetails? primaryPhone = customer.ContactDetails?.FirstOrDefault(contact => contact.ContactType.Is("Phone") && contact.Primary.Is("Y"));
        Address? primaryAddress = customer.AddressDetails?.FirstOrDefault(address => address.Primary.Is("Y"));
        VAA.EAPI.Automation.Core.DB_Model.Entities.Preference? marketingPreference = customerProfile.Preferences?.FirstOrDefault(preference => preference.PreferenceType.Is("VaaMarketingPermissions"));
        VAA.EAPI.Automation.Core.DB_Model.Entities.Preference? partnerMarketingPreference = customerProfile.Preferences?.FirstOrDefault(preference => preference.PreferenceType.Is("VaPartnersMarketingPermissions"));
        VAA.EAPI.Automation.Core.DB_Model.Entities.Preference? groupMarketingPreference = customerProfile.Preferences?.FirstOrDefault(preference => preference.PreferenceType.Is("VaGroupMarketingPermissions"));
        VAA.EAPI.Automation.Core.DB_Model.Entities.Preference? preferredPartnerPreference = customerProfile.Preferences?.FirstOrDefault(preference => preference.PreferenceType.Is("PreferredPartnerFFP"));

        // Verify the five Cosmos documents contain the business values supplied to the mutation.
        softAssert.True(customer.CustomerId.Is(customerId) && customer.PartitionKey.Is(customerId), $"Member '{memberNumber}': expected Customer customerId and partitionKey '{customerId}', but found customerId '{customer.CustomerId}' and partitionKey '{customer.PartitionKey}'.");
        softAssert.True(customer.FirstName.Is(firstName) && customer.LastName.Is(lastName), $"Member '{memberNumber}': expected Customer first name '{firstName}' and last name '{lastName}', but found first name '{customer.FirstName}' and last name '{customer.LastName}'.");
        softAssert.True(customer.Gender.Is(gender), $"Member '{memberNumber}': expected Customer gender '{gender}', but found '{customer.Gender}'.");
        softAssert.True(customer.DateOfBirth?.Date == DateTime.Parse(dateOfBirth).Date, $"Member '{memberNumber}': expected Customer date of birth '{dateOfBirth}', but found '{customer.DateOfBirth?.ToString("yyyy-MM-dd") ?? "<missing>"}'.");
        softAssert.True(customer.EnrollmentChannel.Is(enrollmentChannel), $"Member '{memberNumber}': expected Customer enrolment channel '{enrollmentChannel}', but found '{customer.EnrollmentChannel}'.");
        softAssert.True((primaryEmail?.ContactValue ?? string.Empty).Is(emailId), $"Member '{memberNumber}': expected Customer primary email '{emailId}', but found '{primaryEmail?.ContactValue ?? "<missing>"}'.");
        softAssert.True((primaryPhone?.ContactValue ?? string.Empty).Is(phoneNumber) && (primaryPhone?.CountryCode ?? string.Empty).Is(PhoneCountryCode), $"Member '{memberNumber}': expected Customer primary phone '{phoneNumber}' with country code '{PhoneCountryCode}', but found phone '{primaryPhone?.ContactValue ?? "<missing>"}' with country code '{primaryPhone?.CountryCode ?? "<missing>"}'.");
        softAssert.True((primaryAddress?.AddressLine1 ?? string.Empty).Is("Test Address Line 1") && (primaryAddress?.City ?? string.Empty).Is("London") && (primaryAddress?.PostCode ?? string.Empty).Is(PostalCode), $"Member '{memberNumber}': expected Customer primary address 'Test Address Line 1, London, {PostalCode}', but found '{primaryAddress?.AddressLine1 ?? "<missing>"}, {primaryAddress?.City ?? "<missing>"}, {primaryAddress?.PostCode ?? "<missing>"}'.");
        softAssert.True(customerSearch.CustomerId.Is(customerId) && customerSearch.LoyaltyMemberNumber.Is(memberNumber), $"Member '{memberNumber}': expected CustomerSearch customerId '{customerId}' and member number '{memberNumber}', but found customerId '{customerSearch.CustomerId}' and member number '{customerSearch.LoyaltyMemberNumber}'.");
        softAssert.True(customerSearch.FirstName.Is(firstName) && customerSearch.LastName.Is(lastName) && customerSearch.EmailAddress.Is(emailId), $"Member '{memberNumber}': expected CustomerSearch first name '{firstName}', last name '{lastName}', and email '{emailId}', but found first name '{customerSearch.FirstName}', last name '{customerSearch.LastName}', and email '{customerSearch.EmailAddress}'.");
        softAssert.True(flyingClubMember.CustomerId.Is(customerId) && flyingClubMember.PartitionKey.Is(customerId), $"Member '{memberNumber}': expected FlyingClubMember customerId and partitionKey '{customerId}', but found customerId '{flyingClubMember.CustomerId}' and partitionKey '{flyingClubMember.PartitionKey}'.");
        softAssert.True(flyingClubMember.MemberNumber.Is(memberNumber) && flyingClubMember.Status.Is("Active") && flyingClubMember.TierCode.Is("Red"), $"Member '{memberNumber}': expected FlyingClubMember member number '{memberNumber}', status 'Active', and tier 'Red', but found member number '{flyingClubMember.MemberNumber}', status '{flyingClubMember.Status}', and tier '{flyingClubMember.TierCode}'.");
        softAssert.True(flyingClubMember.MediaCode.Is(mediaCode), $"Member '{memberNumber}': expected FlyingClubMember media code '{mediaCode}', but found '{flyingClubMember.MediaCode}'.");
        softAssert.True(customerProfile.CustomerId.Is(customerId) && customerProfile.PartitionKey.Is(customerId), $"Member '{memberNumber}': expected CustomerProfile customerId and partitionKey '{customerId}', but found customerId '{customerProfile.CustomerId}' and partitionKey '{customerProfile.PartitionKey}'.");
        softAssert.True(marketingPreference?.PreferenceValue.Is("Y") == true && partnerMarketingPreference?.PreferenceValue.Is("Y") == true && groupMarketingPreference?.PreferenceValue.Is("Y") == true && preferredPartnerPreference?.PreferenceValue.Is("DL") == true, $"Member '{memberNumber}': expected CustomerProfile preferences VAA 'Y', partner 'Y', group 'Y', and preferred partner 'DL', but found VAA '{marketingPreference?.PreferenceValue ?? "<missing>"}', partner '{partnerMarketingPreference?.PreferenceValue ?? "<missing>"}', group '{groupMarketingPreference?.PreferenceValue ?? "<missing>"}', and preferred partner '{preferredPartnerPreference?.PreferenceValue ?? "<missing>"}'.");
        softAssert.True(membershipChange.CustomerId.Is(customerId) && membershipChange.PartitionKey.Is(customerId) && membershipChange.MemberNumber.Is(memberNumber), $"Member '{memberNumber}': expected MembershipChange customerId and partitionKey '{customerId}' with member number '{memberNumber}', but found customerId '{membershipChange.CustomerId}', partitionKey '{membershipChange.PartitionKey}', and member number '{membershipChange.MemberNumber}'.");
        softAssert.True(membershipChange.TierCode.Is("Red") && membershipChange.ApprovalDate == membershipChange.CreatedOn, $"Member '{memberNumber}': expected MembershipChange tier 'Red' with approval date equal to created date, but found tier '{membershipChange.TierCode}', approval date '{membershipChange.ApprovalDate}', and created date '{membershipChange.CreatedOn}'.");
        softAssert.AssertAll();

        // Reuse the shared retrying checks for the downstream VRed and Entra B2C enrolment records.
        await EnrolmentHelper.ValidateVRedEnrolmentAsync(tokenFixture, memberNumber);
        await EnrolmentHelper.ValidateAzureB2CEnrolmentAsync(tokenFixture, memberNumber);

        // OCDB accepts either its success advisory or the valid existing-account acknowledgement for a synchronized member.
        EnrolmentHelper.ValidateOcdbEnrolment(certificateFixture, DeltaSoaCertificateKey, memberNumber, dateOfBirth, firstName, lastName, gender, emailId);
    }

    [Fact]
    [Trait("Category", Traits.MyAccountGQLRegressionTests)]
    [Trait("Category", Traits.GqlEntraAuthentication)]
    [Trait("Category", Traits.IdentityMSRelatedGraphqltests)]
    [Trait("Category", Traits.CustomerMSRelatedGraphqltests)]
    public async Task TC02_RejectsInvalidPasswordOtpToken()
    {
        string firstName = DataGenerator.RandomFirstName();
        string lastName = DataGenerator.RandomLastName();
        string emailId = $"{firstName.ToLowerInvariant()}{lastName.ToLowerInvariant()}{Random.Shared.NextInt64(100000000000, 999999999999)}@example.com";
        AccountEntraEnrolMemberRequest enrolmentRequest = CreateEnrolmentRequest("invalid-password-otp-token", firstName, lastName, emailId, DateTime.UtcNow.Date.AddYears(-30).ToString("yyyy-MM-dd"), "M", Random.Shared.NextInt64(7000000000, 9999999999).ToString(), DefaultEnrollmentChannel, MediaCode, Application.LoginPassword);

        // IdentityMS validates the OTP-derived token and exposes its documented error through the GraphQL envelope.
        using HttpResponseMessage enrolmentResponse = GraphQL.AccountEntraEnrolMember(env, enrolmentRequest);
        JObject enrolmentResult = JObject.Parse(await enrolmentResponse.Content.ReadAsStringAsync());
        SoftAssertHelper softAssert = new();
        // Negative GraphQL contracts deliberately return a null payload, which RequireToken rejects.
        JToken? enrolmentPayload = enrolmentResult.SelectToken("data.accountEntraEnrolMember");
        JToken error = enrolmentResult.RequireToken("errors[0]");
        softAssert.True(enrolmentResponse.IsSuccessStatusCode, $"Email '{emailId}': expected GraphQL HTTP 200 with an invalid password-OTP error envelope, but found HTTP {(int)enrolmentResponse.StatusCode}.");
        softAssert.True(enrolmentPayload is { Type: JTokenType.Null }, $"Email '{emailId}': expected a null enrolment payload for an invalid password-OTP token, but found token type '{enrolmentPayload?.Type.ToString() ?? "<missing>"}'.");
        softAssert.True(error.RequireToken("message").ToString().Is("Account Creation failed"), $"Email '{emailId}': expected error message 'Account Creation failed', but found '{error.RequireToken("message")}'.");
        softAssert.True(error.RequireToken("extensions.code").ToString().Is(InvalidPasswordOtpErrorCode), $"Email '{emailId}': expected password-OTP error code '{InvalidPasswordOtpErrorCode}', but found '{error.RequireToken("extensions.code")}'.");
        softAssert.True(error.RequireToken("extensions.message").ToString().Is(InvalidPasswordOtpErrorMessage), $"Email '{emailId}': expected password-OTP error message '{InvalidPasswordOtpErrorMessage}', but found '{error.RequireToken("extensions.message")}'.");
        softAssert.True(error.RequireToken("extensions.source").ToString().Is(EntraApiErrorSource), $"Email '{emailId}': expected password-OTP error source '{EntraApiErrorSource}', but found '{error.RequireToken("extensions.source")}'.");
        softAssert.AssertAll();
    }

    [Fact]
    [Trait("Category", Traits.MyAccountGQLRegressionTests)]
    [Trait("Category", Traits.GqlEntraAuthentication)]
    [Trait("Category", Traits.IdentityMSRelatedGraphqltests)]
    [Trait("Category", Traits.CustomerMSRelatedGraphqltests)]
    public async Task TC03_RejectsPasswordOnNotAllowedList()
    {
        string firstName = DataGenerator.RandomFirstName();
        string lastName = DataGenerator.RandomLastName();
        string emailId = $"{firstName.ToLowerInvariant()}{lastName.ToLowerInvariant()}{Random.Shared.NextInt64(100000000000, 999999999999)}@example.com";
        TableStorageService<OtpRequestTableEntity> otpRequestTableStorageService = new() { ConnectionString = Vault.Default.GetSecretValue("IdentityStorageAccountConnectionString-sit"), TableName = "OTPRequest" };
        string enrolmentToken = await EnrolmentHelper.ReturnEntraEnrolmentToken(otpRequestTableStorageService, emailId);
        AccountEntraEnrolMemberRequest enrolmentRequest = CreateEnrolmentRequest(enrolmentToken, firstName, lastName, emailId, DateTime.UtcNow.Date.AddYears(-30).ToString("yyyy-MM-dd"), "M", Random.Shared.NextInt64(7000000000, 9999999999).ToString(), DefaultEnrollmentChannel, MediaCode, "Password123");

        // IdentityMS rejects passwords that appear on Entra's disallowed-password list through the GraphQL error envelope.
        using HttpResponseMessage enrolmentResponse = GraphQL.AccountEntraEnrolMember(env, enrolmentRequest);
        JObject enrolmentResult = JObject.Parse(await enrolmentResponse.Content.ReadAsStringAsync());
        SoftAssertHelper softAssert = new();
        JToken? enrolmentPayload = enrolmentResult.SelectToken("data.accountEntraEnrolMember");
        JToken error = enrolmentResult.RequireToken("errors[0]");
        softAssert.True(enrolmentResponse.IsSuccessStatusCode, $"Email '{emailId}': expected GraphQL HTTP 200 with a disallowed-password error envelope, but found HTTP {(int)enrolmentResponse.StatusCode}.");
        softAssert.True(enrolmentPayload is { Type: JTokenType.Null }, $"Email '{emailId}': expected a null enrolment payload for a password on the not-allowed list, but found token type '{enrolmentPayload?.Type.ToString() ?? "<missing>"}'.");
        softAssert.True(error.RequireToken("message").ToString().Is("Account Creation failed"), $"Email '{emailId}': expected error message 'Account Creation failed', but found '{error.RequireToken("message")}'.");
        softAssert.True(error.RequireToken("extensions.code").ToString().Is(DisallowedPasswordErrorCode), $"Email '{emailId}': expected disallowed-password error code '{DisallowedPasswordErrorCode}', but found '{error.RequireToken("extensions.code")}'.");
        softAssert.True(error.RequireToken("extensions.message").ToString().Is(DisallowedPasswordErrorMessage), $"Email '{emailId}': expected disallowed-password error message '{DisallowedPasswordErrorMessage}', but found '{error.RequireToken("extensions.message")}'.");
        softAssert.True(error.RequireToken("extensions.source").ToString().Is(EntraApiErrorSource), $"Email '{emailId}': expected disallowed-password error source '{EntraApiErrorSource}', but found '{error.RequireToken("extensions.source")}'.");
        softAssert.AssertAll();
    }

    [Fact]
    [Trait("Category", Traits.MyAccountGQLRegressionTests)]
    [Trait("Category", Traits.GqlEntraAuthentication)]
    public async Task TC04_RejectsDirectUnderEighteenEnrolment()
    {
        string firstName = DataGenerator.RandomFirstName();
        string lastName = DataGenerator.RandomLastName();
        string emailId = $"{firstName.ToLowerInvariant()}{lastName.ToLowerInvariant()}{Random.Shared.NextInt64(100000000000, 999999999999)}@example.com";
        TableStorageService<OtpRequestTableEntity> otpRequestTableStorageService = new() { ConnectionString = Vault.Default.GetSecretValue("IdentityStorageAccountConnectionString-sit"), TableName = "OTPRequest" };
        string enrolmentToken = await EnrolmentHelper.ReturnEntraEnrolmentToken(otpRequestTableStorageService, emailId);
        AccountEntraEnrolMemberRequest enrolmentRequest = CreateEnrolmentRequest(enrolmentToken, firstName, lastName, emailId, DateTime.UtcNow.Date.AddYears(-17).ToString("yyyy-MM-dd"), "M", Random.Shared.NextInt64(7000000000, 9999999999).ToString(), DefaultEnrollmentChannel, MediaCode, Application.LoginPassword);

        // Direct Entra enrolment for an under-18 member returns the Young Flyer error envelope.
        using HttpResponseMessage enrolmentResponse = GraphQL.AccountEntraEnrolMember(env, enrolmentRequest);
        JObject enrolmentResult = JObject.Parse(await enrolmentResponse.Content.ReadAsStringAsync());
        SoftAssertHelper softAssert = new();
        JToken? enrolmentPayload = enrolmentResult.SelectToken("data.accountEntraEnrolMember");
        JToken error = enrolmentResult.RequireToken("errors[0]");
        softAssert.True(enrolmentResponse.IsSuccessStatusCode, $"Email '{emailId}': expected GraphQL HTTP 200 with an under-18 error envelope, but found HTTP {(int)enrolmentResponse.StatusCode}.");
        softAssert.True(enrolmentPayload is { Type: JTokenType.Null }, $"Email '{emailId}': expected a null enrolment payload for an under-18 member, but found token type '{enrolmentPayload?.Type.ToString() ?? "<missing>"}'.");
        softAssert.True(error.RequireToken("message").ToString().Is(UnderEighteenEnrolmentErrorMessage), $"Email '{emailId}': expected under-18 error message '{UnderEighteenEnrolmentErrorMessage}', but found '{error.RequireToken("message")}'.");
        softAssert.True(error.RequireToken("extensions.code").ToString().Is(UnderEighteenEnrolmentErrorCode), $"Email '{emailId}': expected under-18 error code '{UnderEighteenEnrolmentErrorCode}', but found '{error.RequireToken("extensions.code")}'.");
        softAssert.True(error.RequireToken("extensions.message").ToString().Is(UnderEighteenEnrolmentErrorMessage), $"Email '{emailId}': expected under-18 error detail '{UnderEighteenEnrolmentErrorMessage}', but found '{error.RequireToken("extensions.message")}'.");
        softAssert.True(error.RequireToken("extensions.source").ToString().Is(UnderEighteenEnrolmentErrorSource), $"Email '{emailId}': expected under-18 error source '{UnderEighteenEnrolmentErrorSource}', but found '{error.RequireToken("extensions.source")}'.");
        // The frontend uses this action to route an under-18 applicant to the Young Flyer journey.
        softAssert.True(error.RequireToken("extensions.nextAction").ToString().Is(UnderEighteenEnrolmentNextAction), $"Email '{emailId}': expected under-18 next action '{UnderEighteenEnrolmentNextAction}', but found '{error.RequireToken("extensions.nextAction")}'.");
        softAssert.AssertAll();
    }

    // Build the full mutation contract once because the positive and negative cases submit the same required enrolment shape.
    private static AccountEntraEnrolMemberRequest CreateEnrolmentRequest(string enrolmentToken, string firstName, string lastName, string emailId, string dateOfBirth, string gender, string phoneNumber, string enrollmentChannel, string mediaCode, string password)
    {
        return new AccountEntraEnrolMemberRequest
        {
            ChannelId = enrollmentChannel,
            EnrolmentToken = enrolmentToken,
            CustomerType = Application.CustomerType.Loyalty,
            Customer = new EntraCustomer { FirstName = firstName, LastName = lastName, DateOfBirth = dateOfBirth, Gender = gender, Password = password },
            Address = new EntraAddress { AddressLine1 = "Test Address Line 1", City = "London", PostalCode = PostalCode, CountryCode = CountryCode },
            Contact = new EntraContact { EmailId = emailId, PhoneCountryCode = PhoneCountryCode, PhoneNumber = phoneNumber },
            LoyaltyMember = new EntraLoyaltyMember { EnrollmentChannel = enrollmentChannel, MediaCode = mediaCode },
            Preferences = new EntraPreferences { VaaMarketingPermission = "Y", VaPartnersMarketingPermission = "Y", VaGroupMarketingPermissions = "Y", PreferredPartnerFFP = "DL" }
        };
    }
}
