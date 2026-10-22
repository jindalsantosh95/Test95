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
using VAA.EAPI.Automation.MyAccount.Requests.Endpoint;

namespace VAA.EAPI.Automation.MyAccount.Tests.GraphQlTests.Mutation;

[Collection("Sequential")]
public sealed class AccountCreateYoungFlyerAccountTests : IClassFixture<CosmosFixture>, IClassFixture<TokenFixture>, IClassFixture<CertificateFixture>
{
    private const string EnrollmentChannel = "WEB-MYA";
    private const string MediaCode = "Web";
    private const string AppEnrollmentChannel = "APP-MYA";
    private const string AppMediaCode = "App";
    private const string HouseholdGroupName = "Household";
    private const string HouseholdInviteSuccessMessage = "Successfully sent household invitation";
    private const string HouseholdInviteAcceptedMessage = "Successfully Accepted HouseHold Invite";
    private const string GroupDocumentType = "Group";
    private const string YoungFlyerEmailDomain = "U18.virgin.com";
    private const string YoungFlyerDummyEmailValidity = "N";
    private const string YoungFlyerAgeLimitErrorMessage = "The child must be below the Young Flyer age limit of 18.";
    private const string YoungFlyerValidationErrorCode = "15001";
    private const string UnauthenticatedParentErrorMessage = "An authenticated parent is required.";
    private const string UnauthenticatedParentErrorCode = "G064";
    private const string GraphQlConsumingApplicationSource = "Cosuming application";
    private const string DeltaSoaCertificateKey = "Delta-SOA-Certificate";
    private static readonly string ParentPassword = Application.LoginPassword;
    private static readonly string env = Config.Environment;
    private readonly CosmosFixture cosmosFixture;
    private readonly TokenFixture tokenFixture;
    private readonly CertificateFixture certificateFixture;
    private readonly string customerCosmosDb;
    private readonly string customerTransactionCosmosDb;

    public AccountCreateYoungFlyerAccountTests(CosmosFixture cosmosFixture, TokenFixture tokenFixture, CertificateFixture certificateFixture)
    {
        this.cosmosFixture = cosmosFixture;
        this.tokenFixture = tokenFixture;
        this.certificateFixture = certificateFixture;
        customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
        customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

        // Register the customer and household documents required to verify the asynchronous enrolment outcome.
        this.cosmosFixture.RegisterClient(Application.CustomerCosmosDbEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));
        this.cosmosFixture.RegisterType<Customer>(Application.CustomerCosmosDbEndpoint, customerCosmosDb, Application.CustomerCosmosCollection);
        this.cosmosFixture.RegisterClient(Application.CustomerTransactionCosmosDbEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));
        this.cosmosFixture.RegisterType<FlyingClubMember>(Application.CustomerTransactionCosmosDbEndpoint, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
        this.cosmosFixture.RegisterType<Group>(Application.CustomerTransactionCosmosDbEndpoint, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
        this.tokenFixture.AddTokenForAzureB2CFromKeyVault("azureb2c");
        this.tokenFixture.AddTokenForVRedMicroserviceFromKeyVault("VRedMS");
    }

    [Fact]
    [Trait("Category", Traits.MyAccountGQLRegressionTests)]
    [Trait("Category", Traits.IdentityMSRelatedGraphqltests)]
    [Trait("Category", Traits.MemberManagementMSRelatedGraphqltests)]
    [Trait("Category", Traits.VRedMSRelatedGraphqltests)]
    [Trait("Category", Traits.CustomerMSRelatedGraphqltests)]
    [Trait("Category", Traits.CustomerTransactionMSRelatedGraphqltests)]
    public async Task TC01_AuthenticatedParentCreatesAndLinksManagedYoungFlyer()
    {
        // Create isolated synthetic parent data because this mutation creates a household relationship.
        (string parentEmailId, string parentMemberNumber, string parentCustomerId, _, _) = await EnrolmentHelper.GraphQLEnrolment(EnrollmentChannel,
            MediaCode, forHousehold: true);

        string parentToken = await EntraTokenHelper.GetIdToken(env, parentEmailId, parentMemberNumber, ParentPassword);

        // The mutation accepts only an active parent, so establish that precondition from the customer record it reads.
        QueryDefinition parentCustomerQuery = new QueryDefinition("SELECT TOP 1 * FROM CustomerCollection c WHERE c.PartitionKey = @customerId AND c.DocType = @docType")
            .WithParameter("@customerId", parentCustomerId)
            .WithParameter("@docType", Application.DocTypeCustomer);
        Customer? parentCustomer = null;

        // Wait for the parent customer record because the mutation checks that document's active status.
        RetryHelper.RunWithRetry(() =>
        {
            parentCustomer = cosmosFixture.CosmosQueryItem<Customer>(parentCustomerQuery);
            Assert.True(parentCustomer is not null, $"Parent member '{parentMemberNumber}': expected an active customer document, but no document was found.");
        }, maxRetries: 8, delay: 2000);

        Customer persistedParentCustomer = Assert.IsType<Customer>(parentCustomer);
        Assert.True(persistedParentCustomer.Status.Is("Active"), $"Parent member '{parentMemberNumber}': expected customer status 'Active', found '{persistedParentCustomer.Status}'.");

        string childFirstName = DataGenerator.RandomFirstName();
        string childLastName = $"Automation{Random.Shared.Next(100000, 999999)}";

        using HttpResponseMessage createChildResponse = GraphQL.AccountCreateYoungFlyerAccount(
            env, parentToken, childFirstName, childLastName, DateTime.UtcNow.AddYears(-10).ToString("yyyy-MM-dd"), "M", EnrollmentChannel).EnsureGQLResponseSuccess();

        JObject createChildResult = JObject.Parse(await createChildResponse.Content.ReadAsStringAsync());
        string childCustomerId = createChildResult.RequireToken("data.accountCreateYoungFlyerAccount.customerId").ToString();
        string childMemberNumber = createChildResult.RequireToken("data.accountCreateYoungFlyerAccount.memberDetails.memberNumber").ToString();
        string childStatus = createChildResult.RequireToken("data.accountCreateYoungFlyerAccount.memberDetails.status").ToString();

        Assert.False(childCustomerId.IsNullOrWhiteSpace(), $"Child member '{childMemberNumber}': expected a non-empty customer identifier, but no identifier was returned.");
        Assert.False(childMemberNumber.IsNullOrWhiteSpace(), "Expected a non-empty member number for the created Young Flyer, found '<empty>'.");
        Assert.True(childStatus.Is("Active"), $"Child member '{childMemberNumber}': expected membership status 'Active', found '{childStatus}'.");

        // Managed Young Flyer identities must remain disabled rather than allowing a child sign-in.
        await EnrolmentHelper.ValidateAzureB2CEnrolmentAsync(tokenFixture, childMemberNumber, expectedAccountEnabled: false);

        // Poll the eventual stores until address inheritance and household document updates are fully persisted.
        AssertYoungFlyerDocuments(parentCustomerId, parentMemberNumber, childCustomerId, childMemberNumber, childFirstName, childLastName);

        await RetryHelper.RunWithRetryAsync(async () =>
        {
            using HttpResponseMessage householdResponse = GraphQL.RetrieveHouseHoldAccount(env, parentToken, string.Empty, HouseholdGroupName).EnsureGQLResponseSuccess();
            JObject householdResult = JObject.Parse(await householdResponse.Content.ReadAsStringAsync());
            // Match the created child by FC number so the Young Flyer contract is not coupled to response ordering.
            JArray linkedMembers = Assert.IsType<JArray>(householdResult.RequireToken("data.retrieveHouseHoldAccount.linkedMemberInfo"));
            JObject linkedYoungFlyer = Assert.IsType<JObject>(linkedMembers.Single(member =>
                childMemberNumber.Is(member.RequireToken("fCNumber").ToString())));
            bool isYoungFlyer = linkedYoungFlyer.RequireToken("isYoungFlyer").Value<bool>();

            Assert.True(isYoungFlyer,
                $"Parent member '{parentMemberNumber}': expected child member '{childMemberNumber}' to return isYoungFlyer=true.");
        }, maxRetries: 8, delay: 2000);
    }

    [Fact]
    [Trait("Category", Traits.MyAccountGQLRegressionTests)]
    public async Task TC02_MissingParentAuthenticationReturnsControlledGraphQlError()
    {
        // Missing parent authentication is a negative-path fixture; no customer or household state must be created.
        string rejectedFirstName = $"Unauthorised{Random.Shared.Next(100000, 999999)}";
        string rejectedLastName = $"YoungFlyer{Random.Shared.Next(100000, 999999)}";

        using HttpResponseMessage createChildResponse = GraphQL.AccountCreateYoungFlyerAccount(env, string.Empty, rejectedFirstName,
            rejectedLastName, DateTime.UtcNow.AddYears(-10).ToString("yyyy-MM-dd"), "M", EnrollmentChannel);

        JObject createChildResult = JObject.Parse(await createChildResponse.Content.ReadAsStringAsync());

        Assert.True(createChildResponse.IsSuccessStatusCode, $"Missing parent authentication: expected GraphQL HTTP status 200 with an error envelope, found HTTP {(int)createChildResponse.StatusCode}.");
        bool missingParentAuthenticationCreatedAccount = createChildResult.SelectToken("data.accountCreateYoungFlyerAccount") is { Type: not JTokenType.Null and not JTokenType.Undefined };
        Assert.False(missingParentAuthenticationCreatedAccount, $"Missing parent authentication: expected no Young Flyer account payload, found account payload '{missingParentAuthenticationCreatedAccount}'.");
        string missingParentAuthenticationErrorMessage = createChildResult.RequireToken("errors[0].message").ToString();
        string missingParentAuthenticationErrorCode = createChildResult.RequireToken("errors[0].extensions.code").ToString();
        string missingParentAuthenticationExtensionMessage = createChildResult.RequireToken("errors[0].extensions.message").ToString();
        string missingParentAuthenticationErrorSource = createChildResult.RequireToken("errors[0].extensions.source").ToString();

        // Missing parent authentication must return the documented GraphQL error envelope.
        Assert.True(missingParentAuthenticationErrorMessage.Is(UnauthenticatedParentErrorMessage), $"Missing parent authentication: expected message '{UnauthenticatedParentErrorMessage}', found '{missingParentAuthenticationErrorMessage}'.");
        Assert.True(missingParentAuthenticationErrorCode.Is(UnauthenticatedParentErrorCode), $"Missing parent authentication: expected error code '{UnauthenticatedParentErrorCode}', found '{missingParentAuthenticationErrorCode}'.");
        Assert.True(missingParentAuthenticationExtensionMessage.Is(UnauthenticatedParentErrorMessage), $"Missing parent authentication: expected extension message '{UnauthenticatedParentErrorMessage}', found '{missingParentAuthenticationExtensionMessage}'.");
        Assert.True(missingParentAuthenticationErrorSource.Is(GraphQlConsumingApplicationSource), $"Missing parent authentication: expected error source '{GraphQlConsumingApplicationSource}', found '{missingParentAuthenticationErrorSource}'.");

        QueryDefinition rejectedCustomerQuery = new QueryDefinition("SELECT TOP 1 * FROM CustomerCollection c WHERE c.FirstName = @firstName AND c.LastName = @lastName AND c.DocType = @docType")
            .WithParameter("@firstName", rejectedFirstName)
            .WithParameter("@lastName", rejectedLastName)
            .WithParameter("@docType", Application.DocTypeCustomer);

        // Missing authentication must not leave a child customer document behind in Cosmos DB.
        Customer? rejectedCustomer = cosmosFixture.CosmosQueryItem<Customer>(rejectedCustomerQuery);
        Assert.True(rejectedCustomer is null, $"Missing parent authentication: expected no customer document for rejected child '{rejectedFirstName} {rejectedLastName}', but one was found.");
    }

    // Reject dates of birth at or above the supported Young Flyer age limit.
    [Theory]
    [InlineData(18, "M")]
    [InlineData(19, "M")]
    [InlineData(24, "F")]
    [Trait("Category", Traits.MyAccountGQLRegressionTests)]
    [Trait("Category", Traits.IdentityMSRelatedGraphqltests)]
    public async Task TC03_InvalidYoungFlyerRequestReturnsGraphQlErrorWithoutCreatingCustomer(int childAgeInYears, string childGender)
    {
        // Create an eligible parent so the validation outcome is isolated to the invalid child request.
        (string parentEmailId, string parentMemberNumber, string parentCustomerId, _, _) = await EnrolmentHelper.GraphQLEnrolment(EnrollmentChannel, MediaCode, forHousehold: true);

        string parentToken = await EntraTokenHelper.GetIdToken(env, parentEmailId, parentMemberNumber, ParentPassword);

        // The parent must be persisted as active so this negative path tests only the child request data.
        QueryDefinition parentCustomerQuery = new QueryDefinition("SELECT TOP 1 * FROM CustomerCollection c WHERE c.PartitionKey = @customerId AND c.DocType = @docType")
            .WithParameter("@customerId", parentCustomerId)
            .WithParameter("@docType", Application.DocTypeCustomer);

        Customer? parentCustomer = null;

        RetryHelper.RunWithRetry(() =>
        {
            parentCustomer = cosmosFixture.CosmosQueryItem<Customer>(parentCustomerQuery);
            Assert.True(parentCustomer is not null, $"Parent member '{parentMemberNumber}': expected an active customer document, but no document was found.");
        }, maxRetries: 8, delay: 2000);

        Customer persistedParentCustomer = Assert.IsType<Customer>(parentCustomer);
        Assert.True(persistedParentCustomer.Status.Is("Active"), $"Parent member '{parentMemberNumber}': expected customer status 'Active', found '{persistedParentCustomer.Status}'.");

        string rejectedFirstName = DataGenerator.RandomFirstName();
        string rejectedLastName = $"Rejected{Random.Shared.Next(100000, 999999)}";

        // Invalid Young Flyer details must be returned through the GraphQL error envelope.
        using HttpResponseMessage createInvalidChildResponse = GraphQL.AccountCreateYoungFlyerAccount(
            env,
            parentToken,
            rejectedFirstName,
            rejectedLastName,
            DateTime.UtcNow.AddYears(-childAgeInYears).ToString("yyyy-MM-dd"),
            childGender,
            EnrollmentChannel);

        JObject createInvalidChildResult = JObject.Parse(await createInvalidChildResponse.Content.ReadAsStringAsync());

        Assert.True(createInvalidChildResponse.IsSuccessStatusCode, $"Parent member '{parentMemberNumber}': expected GraphQL HTTP status 200 with a validation error envelope, found HTTP {(int)createInvalidChildResponse.StatusCode}.");
        bool invalidRequestCreatedAccount = createInvalidChildResult.SelectToken("data.accountCreateYoungFlyerAccount") is { Type: not JTokenType.Null and not JTokenType.Undefined };
        Assert.False(invalidRequestCreatedAccount, $"Parent member '{parentMemberNumber}': expected no Young Flyer account for invalid child data (age '{childAgeInYears}', gender '{childGender}'), but found account payload '{invalidRequestCreatedAccount}'.");
        string invalidRequestErrorMessage = createInvalidChildResult.RequireToken("errors[0].message").ToString();
        Assert.False(invalidRequestErrorMessage.IsNullOrWhiteSpace(), $"Parent member '{parentMemberNumber}': expected a non-empty validation message for invalid child data (age '{childAgeInYears}', gender '{childGender}'), but found '<empty>'.");
        string invalidRequestErrorCode = createInvalidChildResult.RequireToken("errors[0].extensions.code").ToString();
        Assert.False(invalidRequestErrorCode.IsNullOrWhiteSpace(), $"Parent member '{parentMemberNumber}': expected a non-empty validation error code for invalid child data (age '{childAgeInYears}', gender '{childGender}'), but found '<empty>'.");
        string invalidRequestExtensionMessage = createInvalidChildResult.RequireToken("errors[0].extensions.message").ToString();
        string invalidRequestErrorSource = createInvalidChildResult.RequireToken("errors[0].extensions.source").ToString();

        if (childAgeInYears >= 18)
        {
            // Adult dates of birth must return the documented age-limit error rather than a generic validation failure.
            Assert.True(invalidRequestErrorMessage.Is(YoungFlyerAgeLimitErrorMessage), $"Parent member '{parentMemberNumber}': expected age-limit message '{YoungFlyerAgeLimitErrorMessage}' for child age '{childAgeInYears}', found '{invalidRequestErrorMessage}'.");
            Assert.True(invalidRequestErrorCode.Is(YoungFlyerValidationErrorCode), $"Parent member '{parentMemberNumber}': expected age-limit error code '{YoungFlyerValidationErrorCode}' for child age '{childAgeInYears}', found '{invalidRequestErrorCode}'.");
            Assert.True(invalidRequestExtensionMessage.Is(YoungFlyerAgeLimitErrorMessage), $"Parent member '{parentMemberNumber}': expected age-limit extension message '{YoungFlyerAgeLimitErrorMessage}' for child age '{childAgeInYears}', found '{invalidRequestExtensionMessage}'.");
            Assert.True(invalidRequestErrorSource.Is(GraphQlConsumingApplicationSource), $"Parent member '{parentMemberNumber}': expected age-limit error source '{GraphQlConsumingApplicationSource}' for child age '{childAgeInYears}', found '{invalidRequestErrorSource}'.");
        }

        QueryDefinition rejectedCustomerQuery = new QueryDefinition("SELECT TOP 1 * FROM CustomerCollection c WHERE c.FirstName = @firstName AND c.LastName = @lastName AND c.DocType = @docType")
            .WithParameter("@firstName", rejectedFirstName)
            .WithParameter("@lastName", rejectedLastName)
            .WithParameter("@docType", Application.DocTypeCustomer);

        // A rejected child request must not create a matching customer document in Cosmos DB.
        Customer? rejectedCustomer = cosmosFixture.CosmosQueryItem<Customer>(rejectedCustomerQuery);
        Assert.True(rejectedCustomer is null, $"Parent member '{parentMemberNumber}': expected no customer document after rejecting child data (age '{childAgeInYears}', gender '{childGender}'), but a matching document was found.");
    }

    // The final case verifies that the same account-creation contract works when the parent already has a linked member.
    [Theory]
    [InlineData("M", false)]
    [InlineData("F", false)]
    [InlineData("X", false)]
    [InlineData("U", false)]
    [InlineData("M", true)]
    [Trait("Category", Traits.MyAccountGQLRegressionTests)]
    [Trait("Category", Traits.IdentityMSRelatedGraphqltests)]
    [Trait("Category", Traits.MemberManagementMSRelatedGraphqltests)]
    [Trait("Category", Traits.VRedMSRelatedGraphqltests)]
    [Trait("Category", Traits.CustomerMSRelatedGraphqltests)]
    [Trait("Category", Traits.CustomerTransactionMSRelatedGraphqltests)]
    public async Task TC04_EntraParentCreatesManagedYoungFlyerAcrossDownstreamSystems(string childGender, bool parentHasExistingHouseholdMember)
    {
        // Reuse the Entra flow so this scenario focuses only on the parent-authorized Young Flyer journey.
        (
            string parentEmailId, string parentMemberNumber, string parentCustomerId, string parentFirstName, string parentLastName,
            string parentToken, DateTime parentDateOfBirth, string parentPhoneNumber, string parentCountryCode) = await EnrolmentHelper.EntraEnrolment(EnrollmentChannel, MediaCode, forHousehold: true);

        string? existingHouseholdMemberNumber = parentHasExistingHouseholdMember
            ? await AddExistingHouseholdMemberAsync(parentMemberNumber, parentToken)
            : null;

        string childFirstName = $"{DataGenerator.RandomFirstName()}Young";
        string childLastName = $"{DataGenerator.RandomLastName()}Flyer";
        string childDateOfBirth = DateTime.UtcNow.Date.AddYears(-10).ToString("yyyy-MM-dd");

        // The parent bearer token authorizes creation and household linking of the managed Young Flyer.
        using HttpResponseMessage createChildResponse = GraphQL.AccountCreateYoungFlyerAccount(env, parentToken, childFirstName,
            childLastName, childDateOfBirth, childGender, EnrollmentChannel).EnsureGQLResponseSuccess();

        JObject createChildResult = JObject.Parse(await createChildResponse.Content.ReadAsStringAsync());
        string childCustomerId = createChildResult.RequireToken("data.accountCreateYoungFlyerAccount.customerId").ToString();
        string childMemberNumber = createChildResult.RequireToken("data.accountCreateYoungFlyerAccount.memberDetails.memberNumber").ToString();
        string childStatus = createChildResult.RequireToken("data.accountCreateYoungFlyerAccount.memberDetails.status").ToString();

        Assert.False(childCustomerId.IsNullOrWhiteSpace(), $"Child member '{childMemberNumber}': expected a non-empty customer identifier, but no identifier was returned.");
        Assert.False(childMemberNumber.IsNullOrWhiteSpace(), "Expected a non-empty member number for the created Young Flyer, found '<empty>'.");
        Assert.True(childStatus.Is("Active"), $"Child member '{childMemberNumber}': expected membership status 'Active', found '{childStatus}'.");

        QueryDefinition childCustomerQuery = new QueryDefinition("SELECT TOP 1 * FROM CustomerCollection c WHERE c.PartitionKey = @customerId AND c.DocType = @docType")
            .WithParameter("@customerId", childCustomerId).WithParameter("@docType", Application.DocTypeCustomer);
        QueryDefinition parentCustomerQuery = new QueryDefinition("SELECT TOP 1 * FROM CustomerCollection c WHERE c.PartitionKey = @customerId AND c.DocType = @docType")
            .WithParameter("@customerId", parentCustomerId).WithParameter("@docType", Application.DocTypeCustomer);

        Customer? childCustomer = null;
        Customer? parentCustomer = null;
        string childEmailId = string.Empty;

        // The customer documents are eventually consistent, so wait for inherited contacts and guardianship authorization.
        RetryHelper.RunWithRetry(() =>
        {
            childCustomer = cosmosFixture.CosmosQueryItem<Customer>(childCustomerQuery);
            parentCustomer = cosmosFixture.CosmosQueryItem<Customer>(parentCustomerQuery);
            Assert.True(childCustomer is not null, $"Child member '{childMemberNumber}': expected a customer document with guardianship authorization, but no document was found.");
            Assert.True(parentCustomer is not null, $"Parent member '{parentMemberNumber}': expected a customer document with the registered phone contact, but no document was found.");
            Customer persistedChildCustomer = Assert.IsType<Customer>(childCustomer);
            Authorization? guardianshipAuthorizationCandidate = persistedChildCustomer.Authorizations?
                .SingleOrDefault(authorization => authorization.RelationType.Is("FlyingClubGuardianship"));
            Assert.True(guardianshipAuthorizationCandidate is not null, $"Child member '{childMemberNumber}': expected one 'FlyingClubGuardianship' authorization for parent member '{parentMemberNumber}', but none was found.");
            Authorization guardianshipAuthorization = Assert.IsType<Authorization>(guardianshipAuthorizationCandidate);
            Consent? guardianshipConsentCandidate = guardianshipAuthorization.Consents?
                .SingleOrDefault(consent => consent.ConsentFor.Is("FlyingClubGuardianship"));
            Assert.True(guardianshipConsentCandidate is not null, $"Child member '{childMemberNumber}': expected one 'FlyingClubGuardianship' consent, but none was found.");
            Consent guardianshipConsent = Assert.IsType<Consent>(guardianshipConsentCandidate);

            Assert.True(persistedChildCustomer.CustomerId.Is(childCustomerId), $"Child member '{childMemberNumber}': expected the persisted customer document to match the mutation response, but it referenced a different customer.");
            Assert.True(persistedChildCustomer.FirstName.Is(childFirstName), $"Child member '{childMemberNumber}': expected first name '{childFirstName}', found '{persistedChildCustomer.FirstName}'.");
            Assert.True(persistedChildCustomer.LastName.Is(childLastName), $"Child member '{childMemberNumber}': expected last name '{childLastName}', found '{persistedChildCustomer.LastName}'.");
            Assert.True(persistedChildCustomer.DateOfBirth.HasValue, $"Child member '{childMemberNumber}': expected date of birth '{childDateOfBirth}', but no date of birth was found.");
            Assert.True(persistedChildCustomer.DateOfBirth.Value.ToString("yyyy-MM-dd").Is(childDateOfBirth), $"Child member '{childMemberNumber}': expected date of birth '{childDateOfBirth}', found '{persistedChildCustomer.DateOfBirth.Value:yyyy-MM-dd}'.");
            Assert.True(persistedChildCustomer.Gender.Is(childGender), $"Child member '{childMemberNumber}': expected gender '{childGender}', found '{persistedChildCustomer.Gender}'.");
            Assert.True(guardianshipAuthorization.AuthorizerFCMemberNumber.Is(parentMemberNumber), $"Child member '{childMemberNumber}': expected guardianship parent member '{parentMemberNumber}', found '{guardianshipAuthorization.AuthorizerFCMemberNumber}'.");
            Assert.True(guardianshipAuthorization.Name.Is($"{parentFirstName} {parentLastName}"), $"Child member '{childMemberNumber}': expected the guardianship parent name '{parentFirstName} {parentLastName}', found '{guardianshipAuthorization.Name}'.");
            Assert.True(guardianshipAuthorization.Email.Is(parentEmailId), $"Child member '{childMemberNumber}': expected the guardianship email to match parent member '{parentMemberNumber}', but a different email was found.");
            Assert.True(guardianshipAuthorization.Phone.Is(parentPhoneNumber), $"Child member '{childMemberNumber}': expected the guardianship phone to match parent member '{parentMemberNumber}', but a different phone number was found.");
            Assert.True(guardianshipAuthorization.CountryCode.Is(parentCountryCode), $"Child member '{childMemberNumber}': expected guardianship country code '{parentCountryCode}', found '{guardianshipAuthorization.CountryCode}'.");
            Assert.True(guardianshipAuthorization.RelationType.Is("FlyingClubGuardianship"), $"Child member '{childMemberNumber}': expected relation type 'FlyingClubGuardianship', found '{guardianshipAuthorization.RelationType}'.");
            Assert.True(DateTime.TryParse(guardianshipAuthorization.AuthorizerDateOfBirth, out DateTime authorizerDateOfBirth), $"Child member '{childMemberNumber}': expected a valid parent date of birth for member '{parentMemberNumber}', found '{guardianshipAuthorization.AuthorizerDateOfBirth}'.");
            Assert.True(authorizerDateOfBirth.Date == parentDateOfBirth.Date, $"Child member '{childMemberNumber}': expected parent date of birth '{parentDateOfBirth:yyyy-MM-dd}', found '{authorizerDateOfBirth:yyyy-MM-dd}'.");
            Assert.True(guardianshipConsent.TermsAndConditionsAccepted.Is("Y"), $"Child member '{childMemberNumber}': expected guardianship consent 'Y', found '{guardianshipConsent.TermsAndConditionsAccepted}'.");

            // Require inherited contacts inside the retry so delayed contact persistence cannot leave a stale snapshot.
            Customer persistedParentCustomer = Assert.IsType<Customer>(parentCustomer);
            ContactDetails? childEmailContactCandidate = persistedChildCustomer.ContactDetails?
                .SingleOrDefault(contact => contact.ContactType.Is("Email") && contact.Primary.Is("Y"));
            Assert.True(childEmailContactCandidate is not null, $"Child member '{childMemberNumber}': expected one primary email contact for OCDB validation, but none was found.");
            ContactDetails childEmailContact = Assert.IsType<ContactDetails>(childEmailContactCandidate);
            childEmailId = childEmailContact.ContactValue;
            Assert.False(childEmailId.IsNullOrWhiteSpace(), $"Child member '{childMemberNumber}': expected a non-empty primary email for OCDB validation, but found an empty value.");
            Assert.True(childEmailContact.Valid.Is(YoungFlyerDummyEmailValidity), $"Child member '{childMemberNumber}': expected generated dummy email validity '{YoungFlyerDummyEmailValidity}', found '{childEmailContact.Valid}'.");

            ContactDetails? parentPhoneContactCandidate = persistedParentCustomer.ContactDetails?
                .SingleOrDefault(contact => contact.ContactType.Is("Phone") && contact.Primary.Is("Y"));
            ContactDetails? childPhoneContactCandidate = persistedChildCustomer.ContactDetails?
                .SingleOrDefault(contact => contact.ContactType.Is("Phone") && contact.Primary.Is("Y"));
            Assert.True(parentPhoneContactCandidate is not null, $"Parent member '{parentMemberNumber}': expected one primary phone contact, but none was found.");
            Assert.True(childPhoneContactCandidate is not null, $"Child member '{childMemberNumber}': expected one primary phone contact inherited from parent member '{parentMemberNumber}', but none was found.");
            ContactDetails parentPhoneContact = Assert.IsType<ContactDetails>(parentPhoneContactCandidate);
            ContactDetails childPhoneContact = Assert.IsType<ContactDetails>(childPhoneContactCandidate);
            Assert.True(parentPhoneContact.ContactValue.Is(parentPhoneNumber), $"Parent member '{parentMemberNumber}': expected enrolled phone '{parentPhoneNumber}', found '{parentPhoneContact.ContactValue}'.");
            Assert.True(parentPhoneContact.CountryCode.Is(parentCountryCode), $"Parent member '{parentMemberNumber}': expected enrolled phone country code '{parentCountryCode}', found '{parentPhoneContact.CountryCode}'.");
            Assert.True(childPhoneContact.ContactValue.Is(parentPhoneContact.ContactValue), $"Child member '{childMemberNumber}': expected inherited phone '{parentPhoneContact.ContactValue}', found '{childPhoneContact.ContactValue}'.");
            Assert.True(childPhoneContact.CountryCode.Is(parentPhoneContact.CountryCode), $"Child member '{childMemberNumber}': expected inherited phone country code '{parentPhoneContact.CountryCode}', found '{childPhoneContact.CountryCode}'.");
            Assert.True(childPhoneContact.ContactSubType.Is("Mobile"), $"Child member '{childMemberNumber}': expected inherited phone subtype 'Mobile', found '{childPhoneContact.ContactSubType}'.");
            Assert.True(childPhoneContact.Valid.Is("Y"), $"Child member '{childMemberNumber}': expected inherited phone to be valid, found '{childPhoneContact.Valid}'.");
        }, maxRetries: 8, delay: 2000);

        // Under-18 accounts must receive a generated GUID email at the dedicated U18 domain.
        string[] childEmailParts = childEmailId.Split('@');
        Assert.True(childEmailParts.Length == 2, $"Child member '{childMemberNumber}': expected U18 email '<GUID>@{YoungFlyerEmailDomain}', found '{childEmailId}'.");
        Assert.True(Guid.TryParseExact(childEmailParts[0], "N", out _), $"Child member '{childMemberNumber}': expected a 32-character, hyphen-free GUID email local part, found '{childEmailParts[0]}'.");
        Assert.True(childEmailParts[1].Is(YoungFlyerEmailDomain), $"Child member '{childMemberNumber}': expected U18 email domain '{YoungFlyerEmailDomain}', found '{childEmailParts[1]}'.");

        // CardLinks must return the new Young Flyer's enrolled identity and initial zero balance.
        await RetryHelper.RunWithRetryAsync(async () =>
        {
            using HttpResponseMessage vRedCardLinksResponse = Microservice.VRedMSCardLinks(env, tokenFixture.GetToken("VRedMS"), childMemberNumber).EnsureIsSuccessStatusCode();
            JObject vRedCardLinksResult = JObject.Parse(await vRedCardLinksResponse.Content.ReadAsStringAsync());
            string partnerUserId = vRedCardLinksResult.RequireToken("partnerUserId").ToString();
            string accountStatus = vRedCardLinksResult.RequireToken("accountStatus").ToString();
            string cardLinksFirstName = vRedCardLinksResult.RequireToken("firstName").ToString();
            string cardLinksLastName = vRedCardLinksResult.RequireToken("lastName").ToString();
            string cardLinksEmail = vRedCardLinksResult.RequireToken("email").ToString();
            string currentBalance = vRedCardLinksResult.RequireToken("balance.current").ToString();
            string usableBalance = vRedCardLinksResult.RequireToken("balance.usable").ToString();
            JArray cardLinks = Assert.IsType<JArray>(vRedCardLinksResult.RequireToken("cardLinks"));

            Assert.True(partnerUserId.Is(childMemberNumber), $"Child member '{childMemberNumber}': expected CardLinks partnerUserId '{childMemberNumber}', found '{partnerUserId}'.");
            Assert.True(accountStatus.Is("ACTIVE"), $"Child member '{childMemberNumber}': expected CardLinks accountStatus 'ACTIVE', found '{accountStatus}'.");
            Assert.True(cardLinksFirstName.Is(childFirstName), $"Child member '{childMemberNumber}': expected CardLinks firstName '{childFirstName}', found '{cardLinksFirstName}'.");
            Assert.True(cardLinksLastName.Is(childLastName), $"Child member '{childMemberNumber}': expected CardLinks lastName '{childLastName}', found '{cardLinksLastName}'.");
            Assert.True(cardLinksEmail.Is(childEmailId), $"Child member '{childMemberNumber}': expected CardLinks email '{childEmailId}', found '{cardLinksEmail}'.");
            Assert.True(currentBalance.Is("0"), $"Child member '{childMemberNumber}': expected CardLinks current balance '0', found '{currentBalance}'.");
            Assert.True(usableBalance.Is("0"), $"Child member '{childMemberNumber}': expected CardLinks usable balance '0', found '{usableBalance}'.");
            Assert.Empty(cardLinks);
        }, maxRetries: 5, delay: 2000);

        // Managed Young Flyer identities are registered in B2C but remain disabled for child sign-in.
        await RetryHelper.RunWithRetryAsync(async () =>
        {
            using HttpResponseMessage b2CResponse = Microservice.AzureB2C(tokenFixture.GetToken("azureb2c"), childMemberNumber).EnsureIsSuccessStatusCode();
            JObject b2CResult = JObject.Parse(await b2CResponse.Content.ReadAsStringAsync());
            JArray b2CUsers = Assert.IsType<JArray>(b2CResult.RequireToken("value"));
            JObject b2CUser = Assert.IsType<JObject>(b2CUsers.FirstOrDefault());
            string b2CUserPrincipalName = b2CUser.RequireToken("userPrincipalName").ToString();
            bool b2CAccountEnabled = b2CUser.RequireToken("accountEnabled").Value<bool>();

            Assert.True(b2CUserPrincipalName.Contains(childMemberNumber, StringComparison.Ordinal), $"Child member '{childMemberNumber}': expected the B2C user principal name to contain that member number, but it did not.");
            Assert.False(b2CAccountEnabled, $"Child member '{childMemberNumber}': expected B2C accountEnabled 'False', found '{b2CAccountEnabled}'.");
        }, maxRetries: 8, delay: 1000);

        // Delegate OCDB synchronization validation so its two valid response forms are handled consistently.
        EnrolmentHelper.ValidateOcdbEnrolment(certificateFixture, DeltaSoaCertificateKey, childMemberNumber, childDateOfBirth,
            childFirstName, childLastName, childGender, childEmailId);

        // Household retrieval must expose the Entra parent as leader and the new child as a direct linked member.
        await RetryHelper.RunWithRetryAsync(async () =>
        {
            using HttpResponseMessage householdResponse = GraphQL.RetrieveHouseHoldAccount(env, parentToken,
                string.Empty, HouseholdGroupName).EnsureGQLResponseSuccess();

            JObject householdResult = JObject.Parse(await householdResponse.Content.ReadAsStringAsync());

            JArray linkedMembers = Assert.IsType<JArray>(householdResult.RequireToken("data.retrieveHouseHoldAccount.linkedMemberInfo"));
            // Identify each member explicitly so the type assertions remain correct when response ordering changes.
            JObject linkedYoungFlyer = Assert.IsType<JObject>(linkedMembers.Single(member =>
                childMemberNumber.Is(member.RequireToken("fCNumber").ToString())));
            bool childIsYoungFlyer = linkedYoungFlyer.RequireToken("isYoungFlyer").Value<bool>();

            string householdLeaderMemberNumber = householdResult.RequireToken("data.retrieveHouseHoldAccount.leaderInfo.fCNumber").ToString();
            Assert.True(householdLeaderMemberNumber.Is(parentMemberNumber), $"Parent member '{parentMemberNumber}': expected household leader member number '{parentMemberNumber}', found '{householdLeaderMemberNumber}'.");
            Assert.True(childIsYoungFlyer, $"Parent member '{parentMemberNumber}': expected child member '{childMemberNumber}' to return isYoungFlyer=true.");

            if (existingHouseholdMemberNumber is not null)
            {
                // An established adult must remain linked and must not be classified as a Young Flyer.
                JObject linkedAdult = Assert.IsType<JObject>(linkedMembers.Single(member =>
                    existingHouseholdMemberNumber.Is(member.RequireToken("fCNumber").ToString())));
                bool existingMemberIsYoungFlyer = linkedAdult.RequireToken("isYoungFlyer").Value<bool>();

                Assert.False(existingMemberIsYoungFlyer, $"Parent member '{parentMemberNumber}': expected existing adult member '{existingHouseholdMemberNumber}' to return isYoungFlyer=false.");
                Assert.True(linkedMembers.Count == 2, $"Parent member '{parentMemberNumber}': expected exactly the existing member '{existingHouseholdMemberNumber}' and child member '{childMemberNumber}' as linked members, found '{linkedMembers.Count}'.");
            }
        }, maxRetries: 8, delay: 1000);

        // The managed-account query is parent-scoped and must expose the child just created by this test.
        await RetryHelper.RunWithRetryAsync(async () =>
        {
            using HttpResponseMessage managedAccountsResponse = GraphQL.AccountManagedYoungFlyerAccounts(env, parentToken).EnsureGQLResponseSuccess();
            JObject managedAccountsResult = JObject.Parse(await managedAccountsResponse.Content.ReadAsStringAsync());
            JArray managedChildren = Assert.IsType<JArray>(managedAccountsResult.RequireToken("data.accountManagedYoungFlyerAccounts.youngFlyers"));
            JObject managedChild = Assert.IsType<JObject>(managedChildren.SingleOrDefault(child => child.RequireToken("customerId").ToString().Is(childCustomerId)));

            if (existingHouseholdMemberNumber is not null)
            {
                // Adult household members must not be exposed by the Young Flyer-only retrieval contract.
                Assert.Single(managedChildren);
                bool existingMemberReturnedAsYoungFlyer = managedChildren.Any(youngFlyer => existingHouseholdMemberNumber.Is(youngFlyer.RequireToken("fcNumber").ToString()));
                Assert.False(existingMemberReturnedAsYoungFlyer, $"Parent member '{parentMemberNumber}': expected adult household member '{existingHouseholdMemberNumber}' to be excluded from Young Flyer retrieval, but it was returned.");
            }

            string managedParentCustomerId = managedAccountsResult.RequireToken("data.accountManagedYoungFlyerAccounts.parentCustomerId").ToString();
            string managedParentMemberNumber = managedAccountsResult.RequireToken("data.accountManagedYoungFlyerAccounts.parentFcNumber").ToString();
            bool isHouseholdManaged = managedChild.RequireToken("hoHManaged").Value<bool>();
            string managedChildMemberNumber = managedChild.RequireToken("fcNumber").ToString();
            string managedChildFirstName = managedChild.RequireToken("firstName").ToString();
            string managedChildLastName = managedChild.RequireToken("lastName").ToString();
            string managedChildDateOfBirth = managedChild.RequireToken("dateOfBirth").ToString();
            string managedChildStatus = managedChild.RequireToken("status").ToString();
            bool isManagedChildDateOfBirthParsed = DateTime.TryParse(
                managedChildDateOfBirth,
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AllowWhiteSpaces,
                out DateTime parsedManagedChildDateOfBirth);
            string normalizedManagedChildDateOfBirth = parsedManagedChildDateOfBirth.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

            Assert.True(managedParentCustomerId.Is(parentCustomerId), $"Parent member '{parentMemberNumber}': expected the managed-account query to return its customer reference, but a different customer reference was found.");
            Assert.True(managedParentMemberNumber.Is(parentMemberNumber), $"Parent member '{parentMemberNumber}': expected managed-account parent member number '{parentMemberNumber}', found '{managedParentMemberNumber}'.");
            Assert.True(isHouseholdManaged, $"Child member '{childMemberNumber}': expected hoHManaged 'True', found '{isHouseholdManaged}'.");
            Assert.True(managedChildMemberNumber.Is(childMemberNumber), $"Child member '{childMemberNumber}': expected managed-account member number '{childMemberNumber}', found '{managedChildMemberNumber}'.");
            Assert.True(managedChildFirstName.Is(childFirstName), $"Child member '{childMemberNumber}': expected first name '{childFirstName}', found '{managedChildFirstName}'.");
            Assert.True(managedChildLastName.Is(childLastName), $"Child member '{childMemberNumber}': expected last name '{childLastName}', found '{managedChildLastName}'.");
            Assert.True(isManagedChildDateOfBirthParsed, $"Child member '{childMemberNumber}': expected a parseable managed-account date of birth for '{childDateOfBirth}', found '{managedChildDateOfBirth}'.");
            Assert.True(normalizedManagedChildDateOfBirth.Is(childDateOfBirth), $"Child member '{childMemberNumber}': expected date of birth '{childDateOfBirth}', found '{normalizedManagedChildDateOfBirth}' (raw value '{managedChildDateOfBirth}').");
            Assert.True(managedChildStatus.Is("Active"), $"Child member '{childMemberNumber}': expected managed-account status 'Active', found '{managedChildStatus}'.");
        }, maxRetries: 8, delay: 1000);
    }

    [Theory]
    [InlineData("not-a-date")]
    [InlineData("01/01/2015")]
    [Trait("Category", Traits.MyAccountGQLRegressionTests)]
    [Trait("Category", Traits.IdentityMSRelatedGraphqltests)]
    public async Task TC05_MalformedDateOfBirthReturnsValidationErrorWithoutCreatingChild(string invalidDateOfBirth)
    {
        (string parentEmailId, string parentMemberNumber, _, _, _) = await EnrolmentHelper.GraphQLEnrolment(EnrollmentChannel, MediaCode, forHousehold: true);

        string parentToken = await EntraTokenHelper.GetIdToken(env, parentEmailId, parentMemberNumber, ParentPassword);
        string rejectedFirstName = $"Invalid{Random.Shared.Next(100000, 999999)}";
        string rejectedLastName = $"YoungFlyer{Random.Shared.Next(100000, 999999)}";

        // Invalid DOB values must be rejected before enrolment or household linking begins.
        using HttpResponseMessage createChildResponse = GraphQL.AccountCreateYoungFlyerAccount(env, parentToken, rejectedFirstName,
            rejectedLastName, invalidDateOfBirth, "M", EnrollmentChannel);
        JObject createChildResult = JObject.Parse(await createChildResponse.Content.ReadAsStringAsync());
        bool childWasCreated = createChildResult.SelectToken("data.accountCreateYoungFlyerAccount") is { Type: not JTokenType.Null and not JTokenType.Undefined };

        Assert.True(createChildResponse.IsSuccessStatusCode, $"Parent member '{parentMemberNumber}': expected GraphQL HTTP status 200 with a validation error envelope, found HTTP {(int)createChildResponse.StatusCode}.");
        Assert.True(!childWasCreated, $"Parent member '{parentMemberNumber}': expected no Young Flyer account for malformed date of birth '{invalidDateOfBirth}', but found an account payload.");
        Assert.True(createChildResult.RequireToken("errors[0].message").ToString().Is("The child's date of birth is invalid."), $"Parent member '{parentMemberNumber}': expected the malformed-DOB validation message for '{invalidDateOfBirth}', but found {createChildResult}.");
        Assert.True(createChildResult.RequireToken("errors[0].extensions.code").ToString().Is(YoungFlyerValidationErrorCode), $"Parent member '{parentMemberNumber}': expected error code '{YoungFlyerValidationErrorCode}' for malformed date of birth '{invalidDateOfBirth}', but found {createChildResult}.");

        QueryDefinition rejectedCustomerQuery = new QueryDefinition("SELECT TOP 1 * FROM CustomerCollection c WHERE c.FirstName = @firstName AND c.LastName = @lastName AND c.DocType = @docType")
            .WithParameter("@firstName", rejectedFirstName)
            .WithParameter("@lastName", rejectedLastName)
            .WithParameter("@docType", Application.DocTypeCustomer);
        Customer? rejectedCustomer = cosmosFixture.CosmosQueryItem<Customer>(rejectedCustomerQuery);

        Assert.True(rejectedCustomer is null, $"Parent member '{parentMemberNumber}': expected no customer document for malformed date of birth '{invalidDateOfBirth}', but one was found.");
    }

    [Fact]
    [Trait("Category", Traits.MyAccountGQLRegressionTests)]
    [Trait("Category", Traits.IdentityMSRelatedGraphqltests)]
    [Trait("Category", Traits.MemberManagementMSRelatedGraphqltests)]
    [Trait("Category", Traits.CustomerMSRelatedGraphqltests)]
    public async Task TC06_AppParentCreatesYoungFlyerWithAppChannelAndMediaCode()
    {
        (_, string parentMemberNumber, _, _, _, string parentToken, _, _, _) = await EnrolmentHelper.EntraEnrolment(AppEnrollmentChannel, AppMediaCode, forHousehold: true);

        string childFirstName = $"App{DataGenerator.RandomFirstName()}";
        string childLastName = $"YoungFlyer{Random.Shared.Next(100000, 999999)}";
        string childDateOfBirth = DateTime.UtcNow.Date.AddYears(-10).ToString("yyyy-MM-dd");

        // The APP-MYA request must persist the App media code on the created Young Flyer.
        using HttpResponseMessage createChildResponse = GraphQL.AccountCreateYoungFlyerAccount(env, parentToken, childFirstName,
            childLastName, childDateOfBirth, "F", AppEnrollmentChannel).EnsureGQLResponseSuccess();
        JObject createChildResult = JObject.Parse(await createChildResponse.Content.ReadAsStringAsync());
        string childCustomerId = createChildResult.RequireToken("data.accountCreateYoungFlyerAccount.customerId").ToString();
        string childMemberNumber = createChildResult.RequireToken("data.accountCreateYoungFlyerAccount.memberDetails.memberNumber").ToString();
        string childStatus = createChildResult.RequireToken("data.accountCreateYoungFlyerAccount.memberDetails.status").ToString();

        Assert.True(!childCustomerId.IsNullOrWhiteSpace(), $"Parent member '{parentMemberNumber}': expected a child customer identifier for APP-MYA enrolment, but none was returned.");
        Assert.True(!childMemberNumber.IsNullOrWhiteSpace(), $"Parent member '{parentMemberNumber}': expected a child member number for APP-MYA enrolment, but none was returned.");
        Assert.True(childStatus.Is("Active"), $"Child member '{childMemberNumber}': expected active status for APP-MYA enrolment, found '{childStatus}'.");

        QueryDefinition childCustomerQuery = new QueryDefinition("SELECT TOP 1 * FROM CustomerCollection c WHERE c.PartitionKey = @customerId AND c.DocType = @docType")
            .WithParameter("@customerId", childCustomerId)
            .WithParameter("@docType", Application.DocTypeCustomer);
        QueryDefinition childFlyingClubMemberQuery = new QueryDefinition("SELECT TOP 1 * FROM CustomerDetailsCollection c WHERE c.PartitionKey = @customerId AND c.DocType = @docType")
            .WithParameter("@customerId", childCustomerId)
            .WithParameter("@docType", Application.DocTypeFlyingClubMember);
        Customer? childCustomer = null;
        FlyingClubMember? childFlyingClubMember = null;

        // Wait for the asynchronous enrolment documents before asserting the APP-MYA field mapping.
        RetryHelper.RunWithRetry(() =>
        {
            childCustomer = cosmosFixture.CosmosQueryItem<Customer>(childCustomerQuery);
            childFlyingClubMember = cosmosFixture.CosmosQueryItem<FlyingClubMember>(childFlyingClubMemberQuery);
            Assert.True(childCustomer is not null, $"Child member '{childMemberNumber}': expected a customer document for APP-MYA enrolment, but none was found.");
            Assert.True(childFlyingClubMember is not null, $"Child member '{childMemberNumber}': expected a FlyingClubMember document for APP-MYA enrolment, but none was found.");
        }, maxRetries: 8, delay: 2000);

        if (childCustomer is not null && childFlyingClubMember is not null)
        {
            SoftAssertHelper softAssert = new();
            softAssert.True(childCustomer.EnrollmentChannel.Is(AppEnrollmentChannel), $"Child member '{childMemberNumber}': expected Customer EnrollmentChannel '{AppEnrollmentChannel}', found '{childCustomer.EnrollmentChannel}'.");
            softAssert.True(childFlyingClubMember.MediaCode.Is(AppMediaCode), $"Child member '{childMemberNumber}': expected FlyingClubMember MediaCode '{AppMediaCode}', found '{childFlyingClubMember.MediaCode}'.");
            softAssert.AssertAll();
        }
    }

    private async Task<string> AddExistingHouseholdMemberAsync(string parentMemberNumber, string parentToken)
    {
        // Create an independent adult account so the Young Flyer is added to an established household.
        (
            _, string existingHouseholdMemberNumber, _, _, _, string existingHouseholdMemberToken,
            _, _, _) = await EnrolmentHelper.EntraEnrolment(EnrollmentChannel, MediaCode, forHousehold: true);

        using HttpResponseMessage inviteResponse = GraphQL.AccountInviteMember(env, parentToken, existingHouseholdMemberNumber, HouseholdGroupName).EnsureGQLResponseSuccess();
        JObject inviteResult = JObject.Parse(await inviteResponse.Content.ReadAsStringAsync());
        string inviteMessage = inviteResult.RequireToken("data.accountInviteMember").ToString();
        Assert.True(inviteMessage.Is(HouseholdInviteSuccessMessage), $"Parent member '{parentMemberNumber}': expected household invite confirmation '{HouseholdInviteSuccessMessage}', found '{inviteMessage}'.");

        using HttpResponseMessage acceptInviteResponse = GraphQL.AccountInviteMemberAction(env, existingHouseholdMemberToken, "Accepted", parentMemberNumber, existingHouseholdMemberNumber).EnsureGQLResponseSuccess();
        JObject acceptInviteResult = JObject.Parse(await acceptInviteResponse.Content.ReadAsStringAsync());
        string acceptInviteMessage = acceptInviteResult.RequireToken("data.accountInviteMemberAction").ToString();
        Assert.True(acceptInviteMessage.Is(HouseholdInviteAcceptedMessage), $"Existing household member '{existingHouseholdMemberNumber}': expected invitation acceptance confirmation '{HouseholdInviteAcceptedMessage}', found '{acceptInviteMessage}'.");

        // Wait until the completed invitation is visible before creating the Young Flyer.
        await RetryHelper.RunWithRetryAsync(async () =>
        {
            using HttpResponseMessage householdResponse = GraphQL.RetrieveHouseHoldAccount(env, parentToken, string.Empty, HouseholdGroupName).EnsureGQLResponseSuccess();
            JObject householdResult = JObject.Parse(await householdResponse.Content.ReadAsStringAsync());
            // Confirm the invited adult is visible and is not incorrectly classified as a Young Flyer.
            JArray linkedMembers = Assert.IsType<JArray>(householdResult.RequireToken("data.retrieveHouseHoldAccount.linkedMemberInfo"));
            JObject linkedAdult = Assert.IsType<JObject>(linkedMembers.Single(member =>
                existingHouseholdMemberNumber.Is(member.RequireToken("fCNumber").ToString())));
            bool isYoungFlyer = linkedAdult.RequireToken("isYoungFlyer").Value<bool>();

            Assert.False(isYoungFlyer, $"Parent member '{parentMemberNumber}': expected existing adult member '{existingHouseholdMemberNumber}' to return isYoungFlyer=false.");
        }, maxRetries: 8, delay: 2000);

        return existingHouseholdMemberNumber;
    }

    private void AssertYoungFlyerDocuments(
        string parentCustomerId,
        string parentMemberNumber,
        string childCustomerId,
        string childMemberNumber,
        string childFirstName,
        string childLastName)
    {
        QueryDefinition parentCustomerQuery = new QueryDefinition("SELECT TOP 1 * FROM CustomerCollection c WHERE c.PartitionKey = @customerId AND c.DocType = @docType")
            .WithParameter("@customerId", parentCustomerId)
            .WithParameter("@docType", Application.DocTypeCustomer);
        QueryDefinition childCustomerQuery = new QueryDefinition("SELECT TOP 1 * FROM CustomerCollection c WHERE c.PartitionKey = @customerId AND c.DocType = @docType")
            .WithParameter("@customerId", childCustomerId)
            .WithParameter("@docType", Application.DocTypeCustomer);
        QueryDefinition childFlyingClubMemberQuery = new QueryDefinition("SELECT TOP 1 * FROM CustomerDetailsCollection c WHERE c.PartitionKey = @customerId AND c.DocType = @docType")
            .WithParameter("@customerId", childCustomerId)
            .WithParameter("@docType", Application.DocTypeFlyingClubMember);
        QueryDefinition parentFlyingClubMemberQuery = new QueryDefinition("SELECT TOP 1 * FROM CustomerDetailsCollection c WHERE c.PartitionKey = @customerId AND c.DocType = @docType")
            .WithParameter("@customerId", parentCustomerId)
            .WithParameter("@docType", Application.DocTypeFlyingClubMember);
        Customer? parentCustomer = null;
        Customer? childCustomer = null;
        FlyingClubMember? childFlyingClubMember = null;
        FlyingClubMember? parentFlyingClubMember = null;
        Group? householdGroup = null;

        // Wait for the async enrolment process to persist the child, parent-owned household group, and group history.
        RetryHelper.RunWithRetry(() =>
        {
            parentCustomer = cosmosFixture.CosmosQueryItem<Customer>(parentCustomerQuery);
            childCustomer = cosmosFixture.CosmosQueryItem<Customer>(childCustomerQuery);
            childFlyingClubMember = cosmosFixture.CosmosQueryItem<FlyingClubMember>(childFlyingClubMemberQuery);
            parentFlyingClubMember = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parentFlyingClubMemberQuery);

            Assert.True(parentCustomer is not null, $"Parent member '{parentMemberNumber}': expected a customer document, but no document was found.");
            Assert.True(childCustomer is not null, $"Child member '{childMemberNumber}': expected a customer document, but no document was found.");
            Assert.True(childFlyingClubMember is not null, $"Child member '{childMemberNumber}': expected a FlyingClubMember document, but no document was found.");
            Assert.True(parentFlyingClubMember is not null, $"Parent member '{parentMemberNumber}': expected a FlyingClubMember document, but no document was found.");
            FlyingClubMember persistedChildFlyingClubMember = Assert.IsType<FlyingClubMember>(childFlyingClubMember);
            FlyingClubMember persistedParentFlyingClubMember = Assert.IsType<FlyingClubMember>(parentFlyingClubMember);
            Assert.True(persistedChildFlyingClubMember.GroupId.IsNullOrWhiteSpace(), $"Child member '{childMemberNumber}': expected no GroupId because the parent owns the household group, found '{persistedChildFlyingClubMember.GroupId}'.");
            Assert.False(persistedParentFlyingClubMember.GroupId.IsNullOrWhiteSpace(), $"Parent member '{parentMemberNumber}': expected a household GroupId after Young Flyer creation, but it was empty.");

            QueryDefinition householdGroupQuery = new QueryDefinition("SELECT TOP 1 * FROM CustomerDetailsCollection c WHERE c.GroupId = @groupId AND c.DocType = @docType")
                .WithParameter("@groupId", persistedParentFlyingClubMember.GroupId)
                .WithParameter("@docType", GroupDocumentType);
            householdGroup = cosmosFixture.CosmosQueryItem<Group>(householdGroupQuery);

            Assert.True(householdGroup is not null, $"Parent member '{parentMemberNumber}': expected a household group document for child member '{childMemberNumber}', but no document was found.");
            Group persistedHouseholdGroup = Assert.IsType<Group>(householdGroup);
            Assert.True(persistedHouseholdGroup.HeadOfGroupCustomerId.Is(parentCustomerId),
                $"Parent member '{parentMemberNumber}': expected the household group to be owned by that parent, but it was owned by a different customer.");
            Assert.True(persistedHouseholdGroup.GroupHistory?.Any(history => childCustomerId.Is(history.CustomerId)) == true,
                $"Parent member '{parentMemberNumber}': expected household history to include child member '{childMemberNumber}', but it was not recorded.");
        }, maxRetries: 8, delay: 2000);

        Customer persistedParentCustomer = Assert.IsType<Customer>(parentCustomer);
        Customer persistedChildCustomer = Assert.IsType<Customer>(childCustomer);
        FlyingClubMember persistedChildFlyingClubMember = Assert.IsType<FlyingClubMember>(childFlyingClubMember);

        Assert.True(persistedChildCustomer.CustomerId.Is(childCustomerId), $"Child member '{childMemberNumber}': expected the persisted customer document to match the mutation response, but it referenced a different customer.");
        Assert.True(persistedChildCustomer.FirstName.Is(childFirstName), $"Child member '{childMemberNumber}': expected first name '{childFirstName}', found '{persistedChildCustomer.FirstName}'.");
        Assert.True(persistedChildCustomer.LastName.Is(childLastName), $"Child member '{childMemberNumber}': expected last name '{childLastName}', found '{persistedChildCustomer.LastName}'.");
        Assert.True(persistedParentCustomer.AddressDetails is not null, $"Parent member '{parentMemberNumber}': expected an address for child member '{childMemberNumber}' to inherit, but no address was found.");
        Assert.True(persistedChildCustomer.AddressDetails is not null, $"Child member '{childMemberNumber}': expected an inherited address from parent member '{parentMemberNumber}', but no address was found.");
        List<Address> parentPrimaryAddresses = persistedParentCustomer.AddressDetails
            .Where(address => (address.Primary ?? string.Empty).Is("Y"))
            .ToList();
        List<Address> childPrimaryAddresses = persistedChildCustomer.AddressDetails
            .Where(address => (address.Primary ?? string.Empty).Is("Y"))
            .ToList();
        Assert.True(parentPrimaryAddresses.Count == 1, $"Parent member '{parentMemberNumber}': expected exactly one primary address, found '{parentPrimaryAddresses.Count}'.");
        Assert.True(childPrimaryAddresses.Count == 1, $"Child member '{childMemberNumber}': expected exactly one inherited primary address, found '{childPrimaryAddresses.Count}'.");
        Address parentPrimaryAddress = parentPrimaryAddresses[0];
        Address childPrimaryAddress = childPrimaryAddresses[0];

        SoftAssertHelper softAssert = new();

        // Compare inherited address fields individually because IDs and audit metadata are generated separately for the child.
        softAssert.True(childPrimaryAddress.AdditionalComments.Is(parentPrimaryAddress.AdditionalComments), $"Child member '{childMemberNumber}': expected AdditionalComments '{parentPrimaryAddress.AdditionalComments}', found '{childPrimaryAddress.AdditionalComments}'.");
        softAssert.True(childPrimaryAddress.AddressLine1.Is(parentPrimaryAddress.AddressLine1), $"Child member '{childMemberNumber}': expected AddressLine1 '{parentPrimaryAddress.AddressLine1}', found '{childPrimaryAddress.AddressLine1}'.");
        softAssert.True((childPrimaryAddress.AddressLine2 ?? string.Empty).Is(parentPrimaryAddress.AddressLine2 ?? string.Empty), $"Child member '{childMemberNumber}': expected AddressLine2 '{parentPrimaryAddress.AddressLine2}', found '{childPrimaryAddress.AddressLine2}'.");
        softAssert.True((childPrimaryAddress.AddressLine3 ?? string.Empty).Is(parentPrimaryAddress.AddressLine3 ?? string.Empty), $"Child member '{childMemberNumber}': expected AddressLine3 '{parentPrimaryAddress.AddressLine3}', found '{childPrimaryAddress.AddressLine3}'.");
        softAssert.True((childPrimaryAddress.AddressType ?? string.Empty).Is(parentPrimaryAddress.AddressType ?? string.Empty), $"Child member '{childMemberNumber}': expected AddressType '{parentPrimaryAddress.AddressType}', found '{childPrimaryAddress.AddressType}'.");
        softAssert.True((childPrimaryAddress.AddressVerificationStatus ?? string.Empty).Is(parentPrimaryAddress.AddressVerificationStatus ?? string.Empty), $"Child member '{childMemberNumber}': expected AddressVerificationStatus '{parentPrimaryAddress.AddressVerificationStatus}', found '{childPrimaryAddress.AddressVerificationStatus}'.");
        softAssert.True((childPrimaryAddress.AddressVerified ?? string.Empty).Is(parentPrimaryAddress.AddressVerified ?? string.Empty), $"Child member '{childMemberNumber}': expected AddressVerified '{parentPrimaryAddress.AddressVerified}', found '{childPrimaryAddress.AddressVerified}'.");
        softAssert.True(childPrimaryAddress.City.Is(parentPrimaryAddress.City), $"Child member '{childMemberNumber}': expected City '{parentPrimaryAddress.City}', found '{childPrimaryAddress.City}'.");
        softAssert.True(childPrimaryAddress.Country.Is(parentPrimaryAddress.Country), $"Child member '{childMemberNumber}': expected Country '{parentPrimaryAddress.Country}', found '{childPrimaryAddress.Country}'.");
        softAssert.True((childPrimaryAddress.GeoCodeValidFlag ?? string.Empty).Is(parentPrimaryAddress.GeoCodeValidFlag ?? string.Empty), $"Child member '{childMemberNumber}': expected GeoCodeValidFlag '{parentPrimaryAddress.GeoCodeValidFlag}', found '{childPrimaryAddress.GeoCodeValidFlag}'.");
        softAssert.True((childPrimaryAddress.LandlordOrCompanyName ?? string.Empty).Is(parentPrimaryAddress.LandlordOrCompanyName ?? string.Empty), $"Child member '{childMemberNumber}': expected LandlordOrCompanyName '{parentPrimaryAddress.LandlordOrCompanyName}', found '{childPrimaryAddress.LandlordOrCompanyName}'.");
        softAssert.True(childPrimaryAddress.MetroArea.Is(parentPrimaryAddress.MetroArea), $"Child member '{childMemberNumber}': expected MetroArea '{parentPrimaryAddress.MetroArea}', found '{childPrimaryAddress.MetroArea}'.");
        softAssert.True((childPrimaryAddress.PostalStatus ?? string.Empty).Is(parentPrimaryAddress.PostalStatus ?? string.Empty), $"Child member '{childMemberNumber}': expected PostalStatus '{parentPrimaryAddress.PostalStatus}', found '{childPrimaryAddress.PostalStatus}'.");
        softAssert.True(childPrimaryAddress.PostCode.Is(parentPrimaryAddress.PostCode), $"Child member '{childMemberNumber}': expected PostCode '{parentPrimaryAddress.PostCode}', found '{childPrimaryAddress.PostCode}'.");
        softAssert.True((childPrimaryAddress.PremiseFlag ?? string.Empty).Is(parentPrimaryAddress.PremiseFlag ?? string.Empty), $"Child member '{childMemberNumber}': expected PremiseFlag '{parentPrimaryAddress.PremiseFlag}', found '{childPrimaryAddress.PremiseFlag}'.");
        softAssert.True((childPrimaryAddress.Primary ?? string.Empty).Is(parentPrimaryAddress.Primary ?? string.Empty), $"Child member '{childMemberNumber}': expected Primary '{parentPrimaryAddress.Primary}', found '{childPrimaryAddress.Primary}'.");
        softAssert.True(childPrimaryAddress.Province.Is(parentPrimaryAddress.Province), $"Child member '{childMemberNumber}': expected Province '{parentPrimaryAddress.Province}', found '{childPrimaryAddress.Province}'.");
        softAssert.True((childPrimaryAddress.Valid ?? string.Empty).Is(parentPrimaryAddress.Valid ?? string.Empty), $"Child member '{childMemberNumber}': expected Valid '{parentPrimaryAddress.Valid}', found '{childPrimaryAddress.Valid}'.");
        softAssert.True(persistedChildFlyingClubMember.MemberNumber.Is(childMemberNumber), $"Child member '{childMemberNumber}': expected FlyingClubMember number '{childMemberNumber}', found '{persistedChildFlyingClubMember.MemberNumber}'.");
        softAssert.True(persistedChildFlyingClubMember.MemberType.Is("Individual"), $"Child member '{childMemberNumber}': expected member type 'Individual', found '{persistedChildFlyingClubMember.MemberType}'.");
        softAssert.True(persistedChildFlyingClubMember.HeadOfGroupMemberNumber.Is(parentMemberNumber), $"Child member '{childMemberNumber}': expected household leader member '{parentMemberNumber}', found '{persistedChildFlyingClubMember.HeadOfGroupMemberNumber}'.");
        softAssert.True(persistedChildFlyingClubMember.Status.Is("Active"), $"Child member '{childMemberNumber}': expected membership status 'Active', found '{persistedChildFlyingClubMember.Status}'.");

        softAssert.AssertAll();
    }

}
