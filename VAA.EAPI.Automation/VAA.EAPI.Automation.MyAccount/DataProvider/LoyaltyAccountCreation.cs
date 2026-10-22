using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.AzureStorageTable;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Delta.Helpers;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MyAccount.Constants;
using VAA.EAPI.Automation.MyAccount.Helpers;
using VAA.EAPI.Automation.MyAccount.Models;
using VAA.EAPI.Automation.MyAccount.Models.Enrolment;
using VAA.EAPI.Automation.MyAccount.Requests.Endpoint;
using VAA.EAPI.Automation.MyAccount.Requests.Models.GraphQL.Mutation;
using VAA.EAPI.Infrastructure.AzureStorage;
using Xunit.Abstractions;
using static VAA.EAPI.Automation.MyAccount.Constants.Application;

namespace VAA.EAPI.Automation.MyAccount.DataProvider
{
    public class LoyaltyAccountCreation : IClassFixture<CertificateFixture>, IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>, IClassFixture<TableStorageService<OtpRequestTableEntity>>
    {
        public FlyingClubMemberHelper flyingClubMemberHelper;
        public readonly string env = Config.Environment;
        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;
        private readonly TableStorageService<OtpRequestTableEntity> otpRequestTableEntityTableStorageService;
        private readonly ITestOutputHelper output;

        private const string DeltaSoaCertificateKey = "Delta-SOA-Certificate";

        private const string LoyaltyAccountsTableName = "LoyaltyAccounts";
        public static string connectionString = Vault.Default.GetSecretValue("AzureStoragePnrCreationsConnectionString");

        private readonly string currentWorkingDirectory = Path.GetFullPath(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName);


        private const string wingsAwardProductId = "1634561747";
        private const string wingsAwardProductName = "Top Member";
        private const string operationIdentifier = "CONTACTUPDATE";

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        public static readonly string CustomerCosmosDbSIT = "CustomerDB_Acc";
        public static readonly string CustomerCosmosDbUAT = "CustomerDB_UAT";
        public static readonly string CustomerCosmosCollection = "CustomerCollection";
        public static readonly string CustomerTransactionCosmosDbSIT = "CustomerTransactionsDB_Acc";
        public static readonly string CustomerTransactionCosmosDbUAT = "CustomerTransactionsDB_UAT";
        public static readonly string CustomerTransactionCosmosCollection = "CustomerDetailsCollection";

        private static string ocpSubKey = string.Empty;

        private readonly CertificateFixture _certificateFixture;

        public LoyaltyAccountCreation(CertificateFixture certificateFixture, TokenFixture tokenFixture, CosmosFixture cosmosFixture, TableStorageService<OtpRequestTableEntity> otpRequestTableEntityTableStorageService, ITestOutputHelper output)
        {
            flyingClubMemberHelper = new();
            this.tokenFixture = tokenFixture;
            this.tokenFixture.AddTokenForVRedMicroserviceFromKeyVault("VRedMS");

            this.cosmosFixture = cosmosFixture;

            this.tokenFixture.AddTokenForCustomerMSFromKeyVault("customerMS");
            this.tokenFixture.AddTokenForAzureB2CFromKeyVault("azureGraphB2C");
            this.tokenFixture.AddTokenForDynamicsEnrolmentFromKeyVault("enrolment");

            this.otpRequestTableEntityTableStorageService = otpRequestTableEntityTableStorageService;
            this.output = output;

            string cosmosEndpoint = Constants.Application.CustomerCosmosDbEndpoint;
            string cosmosEndpointForCustomerTrx = Constants.Application.CustomerTransactionCosmosDbEndpoint;

            customerCosmosDb = env.Is("uat") ? CustomerCosmosDbUAT : CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? CustomerTransactionCosmosDbUAT : CustomerTransactionCosmosDbSIT;

            this.cosmosFixture.RegisterClient(cosmosEndpoint, Vault.Default.GetSecretValue("TESTING-Cmdbcustomer-Automation-Key"));
            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<Award>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, CustomerTransactionCosmosCollection);
            this.cosmosFixture.RegisterType<Customer>(cosmosEndpoint, customerCosmosDb, CustomerCosmosCollection);

            ocpSubKey = Vault.Default.GetSecretValue("TestingCustomerMSSubscriptionKey");

            _certificateFixture = certificateFixture;
            _certificateFixture.ImportCertificate(DeltaSoaCertificateKey, Vault.Default, "Delta-SOA-Certificate");

        }


        [Theory]
        [Trait("Category", Traits.LoyaltyAccountDataCreation)]
        [InlineData("UK", 1)]
        [InlineData("US", 1)]
        [InlineData("NonUKUS", 1)]
        public async Task FCIndividualUnderEighteenFacadeEnrolment(string countryGroup, int numberOfAccounts)
        {
            Assert.True(numberOfAccounts > 0, $"Expected number of U18 accounts to create to be greater than zero, but found '{numberOfAccounts}'.");

            // Create the requested number of individual U18 accounts for the selected regional address group.
            for (int accountIndex = 0; accountIndex < numberOfAccounts; accountIndex++)
            {
                await CreateIndividualUnderEighteenFacadeAccountAsync(countryGroup);
            }
        }

        private async Task CreateIndividualUnderEighteenFacadeAccountAsync(string countryGroup)
        {
            EnrolmentHelper.AddressWithPhoneCode addressDetails = countryGroup switch
            {
                "UK" => EnrolmentHelper.PickRandomAddress(EnrolmentHelper.Country.GB),
                "US" => EnrolmentHelper.PickRandomAddress(EnrolmentHelper.Country.US),
                "NonUKUS" => EnrolmentHelper.PickRandomNonUkUsAddress(),
                _ => throw new ArgumentOutOfRangeException(nameof(countryGroup), countryGroup, "A supported U18 address group was not supplied.")
            };
            string firstName = DataGenerator.RandomFirstName();
            string lastName = DataGenerator.RandomLastName();
            string email = firstName.ToLower() + lastName.ToLower() + Random.Shared.NextInt64(100000000000, 999999999999) + "@example.com";
            string dateOfBirth = DateTime.UtcNow.AddYears(-14).ToString("yyyy-MM-dd");
            string enrollmentChannel = "WEB-MYA";

            // Enrolment Facade remains the supported route for creating an individual U18 test account.
            using HttpResponseMessage enrolmentResponse = Microservice.Enrolment(
                env,
                tokenFixture.GetToken("enrolment"),
                enrollmentChannel,
                firstName,
                lastName,
                email,
                addressDetails.PostalCode,
                addressDetails.CountryCode,
                output,
                dateOfBirth: dateOfBirth,
                city: addressDetails.City,
                addressLine1: addressDetails.AddressLine1,
                memberType: "Individual",
                guardianTermsAndConditionsAccepted: true,
                phoneCountryCode: addressDetails.PhoneCountryCode,
                region: string.Empty);

            string enrolmentBody = await enrolmentResponse.Content.ReadAsStringAsync();
            Assert.True(
                enrolmentResponse.StatusCode == System.Net.HttpStatusCode.Accepted,
                $"Expected enrolment facade to return '{System.Net.HttpStatusCode.Accepted}', but found '{enrolmentResponse.StatusCode}'.");

            JObject enrolmentResult = JObject.Parse(enrolmentBody);
            string customerId = enrolmentResult.RequireToken("customerId").ToString();
            string memberNumber = enrolmentResult.RequireToken("memberdetails.memberNumber").ToString();

            Assert.False(customerId.IsNullOrEmpty(), $"Expected an under-18 enrolment customer ID, but found '{customerId}'.");
            Assert.False(memberNumber.IsNullOrEmpty(), $"Expected an under-18 enrolment member number, but found '{memberNumber}'.");

            // Facade provisioning is asynchronous, so retry the Identity Facade password update before persisting the account.
            RetryHelper.RunWithRetry(() =>
            {
                using HttpResponseMessage passwordResetResponse = IdentityFacadeApi.ResetPassword(memberNumber, Application.LoginPassword);
                Assert.True(passwordResetResponse.IsSuccessStatusCode, $"Expected Identity Facade password update to return a successful HTTP status, but found '{passwordResetResponse.StatusCode}'.");
            }, maxRetries: 5, delay: 1000);

            // Persist the individual U18 account for suites that require an account outside a household.
            await AddLoyaltyAccountToStorageTable(
                LoyaltyAccountsTableName,
                memberNumber,
                env,
                "Red",
                0,
                firstName,
                lastName,
                email,
                dateOfBirth,
                addressDetails.CountryCode == "GB" ? "UK" : addressDetails.CountryCode,
                true);
        }

        [Obsolete("This test is obsolete and will be removed in a upcoming PR as the enrolment is now on entra.")]
        [Theory]
        [MemberData(nameof(LoyaltyAccountCreationTestData.EnrolmentCases), MemberType = typeof(LoyaltyAccountCreationTestData))]
        public async Task FCEnrolment(FlyingClubMemberTiers tier, int points, string fName = "", string lName = "", bool underSixteen = false, bool isWings = false, bool forHouseHold = false)
        {

            string firstName = string.IsNullOrEmpty(fName) ? DataGenerator.RandomFirstName() : fName;
            string lastName = string.IsNullOrEmpty(lName) ? DataGenerator.RandomLastName() : lName;
            // Under-sixteen cases use a minor date of birth so the enrolment payload and stored account classification agree.
            string dateOfBirth = underSixteen ? DateTime.UtcNow.AddYears(-15).AddMonths(Random.Shared.Next(1, 12)).ToString("yyyy-MM-dd") : DateTime.UtcNow.AddYears(-30).AddMonths(Random.Shared.Next(1, 12)).ToString("yyyy-MM-dd");
            string gender = Application.GenderTypes[Random.Shared.Next(0, Application.GenderTypes.Length)];
            string email = firstName.ToLower() + lastName.ToLower() + Random.Shared.Next(0, 10000) +
            "@example.com";
            string enrollmentChannel = "WEB-MYA";
            string mediaCode = "App";
            int maxAttempts = 10;
            int attempt = 0;
            string password = Application.LoginPassword;
            string tierSet = tier.ToString();

            int countryCode = Random.Shared.Next(0, 2);

            // If it is for household then it will use a fixed address else it will pick a random address from the list of addresses available in the EnrolmentHelper class.
            EnrolmentHelper.AddressWithPhoneCode addressDetails = forHouseHold ? new EnrolmentHelper.AddressWithPhoneCode(
                "Household",
                "Test Address Line 1",
                "Test Address Line 2",
                "London",
                "RH117DU",
                "GB",
                "44"
            ) : EnrolmentHelper.PickRandomAddress((EnrolmentHelper.Country)countryCode);

            EnrolmentHelper.PreferenceSet preferences = EnrolmentHelper.PickRandomPreferences();

            string phoneNumber = Random.Shared.NextInt64(7000000000, 9999999999).ToString("D10").TrimStart('0');

            //Get enrolment token for the email from Azure Table Storage where OTP details are stored which will be used for enrolment request
            string enrolmentToken = await EnrolmentHelper.ReturnEnrolmentToken(otpRequestTableEntityTableStorageService, email);

            EnrolmentRequest req = new()
            {
                EnrolmentToken = enrolmentToken,
                EnrollmentChannel = enrollmentChannel,
                MediaCode = mediaCode,
                ChannelId = enrollmentChannel,
                TermsAndConditionsAccepted = true,
                CustomerType = CustomerType.Loyalty,
                Customer = new() { FirstName = firstName, LastName = lastName, DateOfBirth = dateOfBirth, Gender = gender, Password = password },
                Address = new() { AddressLine1 = addressDetails.AddressLine1, City = addressDetails.City, PostalCode = addressDetails.PostalCode, CountryCode = addressDetails.CountryCode },
                Contact = new() { EmailId = email, PhoneCountryCode = addressDetails.PhoneCountryCode, PhoneNumber = phoneNumber },
                Preferences = new() { VaaMarketingPermission = preferences.VaaMarketingPermissions, VaPartnersMarketingPermission = preferences.VaPartnersMarketingPermissions, VaGroupMarketingPermissions = preferences.VaGroupMarketingPermissions, PreferredPartnerFFP = preferences.PreferredPartnerFFP }
            };

            //Invoke enrolment GRAPHQL endpoint
            HttpResponseMessage enrolmentResponse = null;
            JObject enrolmentResult = null;

            enrolmentResponse = GraphQL.Enrolment(env, req);
            string enrolmentBody = await enrolmentResponse.Content.ReadAsStringAsync();
            enrolmentResult = JObject.Parse(enrolmentBody);
            Assert.False(enrolmentResult["errors"]?.Values().Any() == true, $"Errors were returned in the response. Error was: {System.Environment.NewLine} {enrolmentResult}");

            string customerId = enrolmentResult.RequireToken("data.accountEnrolMember.customerId").ToString();

            string memberNumber = enrolmentResult.RequireToken("data.accountEnrolMember.memberDetails.memberNumber").ToString();

            //Validate if member is enrolled into VRed
            await RetryHelper.RunWithRetryAsync(async () =>
             {

                 HttpResponseMessage vRedResponseAPI = Microservice.VredPointsMsEndPoint(env, tokenFixture.GetToken("VRedMS"), memberNumber).EnsureIsSuccessStatusCode();

                 dynamic vRedResultAPI = JsonConvert.DeserializeObject<dynamic>(await vRedResponseAPI.Content.ReadAsStringAsync());

                 Assert.True(!string.IsNullOrEmpty(vRedResultAPI.Current.ToString()), $"Member '{memberNumber}': expected a populated VRed current balance, but found '{vRedResultAPI.Current}'.");

             }, 5, 4000);

            // Delta OCDB is an external downstream dependency and is not required to create
            // reusable loyalty test data. Do not let its availability block account creation.

            #region AIR4 XML request for DL Call and Validating if the member is synced in DL OCDB system

            string requestLoyaltyProfileSyncAdd;
            string doB = DateTime.Parse(dateOfBirth).ToString("yyyyMMdd");
            XmlDocument air4LoyaltyProfileSyncAddResponseDocument = null;
            await RetryHelper.RunWithRetryAsync(async () =>
            {

                string localRequest;
                air4LoyaltyProfileSyncAddResponseDocument = AIR4Helper.LoyaltyProfileSyncAdd(
                    _certificateFixture.GetCertificate(DeltaSoaCertificateKey),
                    memberNumber,
                    doB,
                    firstName,
                    lastName,
                    "M",
                    email,
                    out localRequest);
                requestLoyaltyProfileSyncAdd = localRequest;

            }, 3, 2000);

            XmlNamespaceManager air4LoyaltyProfileSyncAddResponseManager = air4LoyaltyProfileSyncAddResponseDocument.CreateXmlNamespaceManager();

            string errorCode = string.Empty;
            string errorText = string.Empty;
            string reasonCode = string.Empty;
            string reasonText = string.Empty;
            string freeformText = string.Empty;
            string messageNumber = string.Empty;
            string sscRequest = string.Empty;

            try
            {
                // First Sceanrio - Expected to get error code with Error attributes - Members should already be synced at DL end
                errorCode = air4LoyaltyProfileSyncAddResponseDocument.SelectSingleNode("//error:Error/@ErrorCode", air4LoyaltyProfileSyncAddResponseManager)?.InnerText;
                errorText = air4LoyaltyProfileSyncAddResponseDocument.SelectSingleNode("//error:Error/@ErrorText", air4LoyaltyProfileSyncAddResponseManager)?.InnerText;
                reasonCode = air4LoyaltyProfileSyncAddResponseDocument.SelectSingleNode("//error:Error/@ReasonCode", air4LoyaltyProfileSyncAddResponseManager)?.InnerText;
                reasonText = air4LoyaltyProfileSyncAddResponseDocument.SelectSingleNode("//error:Error/@ReasonText", air4LoyaltyProfileSyncAddResponseManager)?.InnerText;
            }
            catch (Exception)
            {
                //ignore
            }

            try
            {
                // Second scenario endpoint will sync at DL end and validate for AdvisoryMessage attributes (success case)
                freeformText = air4LoyaltyProfileSyncAddResponseDocument.SelectSingleNode("//advisorymessage:AdvisoryMessage/@FreeformText", air4LoyaltyProfileSyncAddResponseManager)?.InnerText;
                messageNumber = air4LoyaltyProfileSyncAddResponseDocument.SelectSingleNode("//advisorymessage:AdvisoryMessage/@MessageNumber", air4LoyaltyProfileSyncAddResponseManager)?.InnerText;
                sscRequest = air4LoyaltyProfileSyncAddResponseDocument.SelectSingleNode("//advisorymessage:AdvisoryMessage/@SscRequest", air4LoyaltyProfileSyncAddResponseManager)?.InnerText;
            }
            catch (Exception)
            {
                //ignore
            }

            // Check type of response we received and validate accordingly
            if (!freeformText.IsNullOrEmpty() && !messageNumber.IsNullOrEmpty() && !sscRequest.IsNullOrEmpty())
            {
                // Success case assertions
                Assert.True(freeformText.Equals("OK"), $"Expected FreeformText 'OK' for member number {memberNumber}. Actual value: {freeformText}");
                Assert.True(messageNumber.Equals("65535"), $"Expected MessageNumber '65535' for member number {memberNumber}. Actual value: {messageNumber}");
                Assert.True(sscRequest.Equals("FCFFSC"), $"Expected SscRequest 'FCFFSC' for member number {memberNumber}. Actual value: {sscRequest}");
            }
            else if (!errorCode.IsNullOrEmpty() && !errorText.IsNullOrEmpty() && !reasonCode.IsNullOrEmpty() && !reasonText.IsNullOrEmpty())
            {
                // Error case assertions
                Assert.True(errorCode.Equals("1501"), $"Expected error code 1501 received for ACCT PRESENT in OCDB for member number {memberNumber}. Actual value: {errorCode}");
                Assert.True(errorText.Equals("tpf error"), $"Expected ErrorText 'tpf error' for member number {memberNumber}. Actual value: {errorText}");
                Assert.True(reasonCode.Equals("65535"), $"Expected ReasonCode '65535' for member number {memberNumber}. Actual value: {reasonCode}");
                Assert.True(reasonText.Equals("FFSC FAILURE - ACCT ALREADY EXISTS"), $"Expected ReasonText 'FFSC FAILURE - ACCT ALREADY EXISTS' for member number {memberNumber}. Actual value: {reasonText}");
            }
            else
            {
                // Fail test for unexpected response
                string actualResponse = air4LoyaltyProfileSyncAddResponseDocument.OuterXml;
                Assert.Fail($"OCDB sync service is not working at DL end for member number {memberNumber}. Unexpected response received: {actualResponse}");
            }

            #endregion AIR4 XML request for DL Call and Validating if the member is synced in DL OCDB system


            //Upgrade tier using Loops API's if tier is Silver/Gold as enrolment will happen in Red tier by default, else if tier is Red then it will validate the same tier.
            await flyingClubMemberHelper.UpgradeTheFlyingClubMemberTier(memberNumber, tier);

            //Add points to the member account using Loops API's and validate the same in VRed with retry mechanism as it can take time to reflect in VRed.
            bool isTransactionCreated = await flyingClubMemberHelper.AddLoyaltyPointsToTheFlyingClubAccount(memberNumber, points);

            Assert.True(isTransactionCreated, $"Member '{memberNumber}': expected loyalty-points credit transaction creation to succeed, but found success '{isTransactionCreated}'.");

            long memberPointBalance = await flyingClubMemberHelper.GetPointBalanceOfFlyingClubAccount(memberNumber);

            while (attempt < maxAttempts)
            {
                if (memberPointBalance.Equals(points))
                    break;
                else
                {
                    memberPointBalance = await flyingClubMemberHelper.GetPointBalanceOfFlyingClubAccount(memberNumber);
                    attempt++;
                    await Task.Delay(1000);
                }
            }

            //Force a recall as this can sometimes take time to reflect.
            Assert.True(memberPointBalance.Equals(points), $"For the MemberNumber {memberNumber} Points balance did not match Vred after 5 retries. Expected {points} and got {memberPointBalance}");

            //Validate if the member details are updated in Azure B2C using Graph API with retry mechanism as it can take time to reflect in Graph.
            await RetryHelper.RunWithRetryAsync(async () =>
            {
                HttpResponseMessage getUserResponse = MicrosoftGraph.GetUserDetailsByEmailOrFCNumber(tokenFixture.GetToken("azureGraphB2C"), email).EnsureIsSuccessStatusCode();

                AzureB2CResponse b2cResponse = JsonConvert.DeserializeObject<AzureB2CResponse>(await getUserResponse.Content.ReadAsStringAsync());

                Assert.True(b2cResponse?.azureB2CValues?.Count() == 1, $"Expected to find exactly one user in Azure B2C with email {email}, but found {b2cResponse.azureB2CValues.Count()}.");

                Assert.True(b2cResponse.azureB2CValues[0]?.givenName == firstName, $"Member '{memberNumber}': expected Azure B2C given name '{firstName}', but found '{b2cResponse.azureB2CValues[0]?.givenName ?? "<missing>"}'.");
                Assert.True(b2cResponse.azureB2CValues[0]?.surname == lastName, $"Member '{memberNumber}': expected Azure B2C surname '{lastName}', but found '{b2cResponse.azureB2CValues[0]?.surname ?? "<missing>"}'.");
                Assert.True(b2cResponse.azureB2CValues[0]?.accountEnabled == true, $"Member '{memberNumber}': expected Azure B2C accountEnabled 'True', but found '{b2cResponse.azureB2CValues[0]?.accountEnabled.ToString() ?? "<missing>"}'.");
                Assert.True(b2cResponse.azureB2CValues[0]?.identities.FirstOrDefault(x => x.signInType == "emailAddress")?.issuerAssignedId == email, $"Member '{memberNumber}': expected Azure B2C email identity '{email}', but found '{b2cResponse.azureB2CValues[0]?.identities.FirstOrDefault(x => x.signInType == "emailAddress")?.issuerAssignedId ?? "<missing>"}'.");
                Assert.True(b2cResponse.azureB2CValues[0]?.identities.FirstOrDefault(x => x.signInType == "username")?.issuerAssignedId == memberNumber, $"Member '{memberNumber}': expected Azure B2C username identity '{memberNumber}', but found '{b2cResponse.azureB2CValues[0]?.identities.FirstOrDefault(x => x.signInType == "username")?.issuerAssignedId ?? "<missing>"}'.");


            }, 5, 1000);

            //Validate that the member can sign in using MemberSignIn GraphQL endpoint and validate the response with retry mechanism as it can take time to reflect in Graph.
            await RetryHelper.RunWithRetryAsync(async () =>
            {
                //invoke enrolment MemberSignIn GraphQL endpoint       
                HttpResponseMessage gqlResponse = GraphQL.AccountMemberSignIn(env, email, memberNumber, password).EnsureGQLResponseSuccess();

                JObject dynamicGqlResponse = JObject.Parse(await gqlResponse.Content.ReadAsStringAsync());

                bool requiresMFA = dynamicGqlResponse.SelectToken("data.accountMemberSignIn.requiresMFA")?.Value<bool>() == true;
                if (requiresMFA)
                {
                    string? expiresIn = dynamicGqlResponse.SelectToken("data.accountMemberSignIn.expiresIn")?.ToString();
                    string? accessToken = dynamicGqlResponse.SelectToken("data.accountMemberSignIn.accessToken")?.ToString();
                    string? refreshToken = dynamicGqlResponse.SelectToken("data.accountMemberSignIn.refreshToken")?.ToString();
                    string? idToken = dynamicGqlResponse.SelectToken("data.accountMemberSignIn.idToken")?.ToString();
                    string? continuationToken = dynamicGqlResponse.SelectToken("data.accountMemberSignIn.continuationToken")?.ToString();

                    Assert.True(expiresIn == null, $"Member '{memberNumber}': expected no MFA expiresIn value, but found '{expiresIn ?? "<null>"}'.");
                    Assert.True(idToken == null, $"Member '{memberNumber}': expected no MFA ID token, but found '{(idToken is null ? "<null>" : "<populated>")}'.");
                    Assert.True(accessToken == null, $"Member '{memberNumber}': expected no MFA access token, but found '{(accessToken is null ? "<null>" : "<populated>")}'.");
                    Assert.True(refreshToken == null, $"Member '{memberNumber}': expected no MFA refresh token, but found '{(refreshToken is null ? "<null>" : "<populated>")}'.");
                    Assert.False(continuationToken.IsNullOrEmpty(), "continuation token was empty");
                }
                else
                {
                    string tokenType = dynamicGqlResponse.RequireToken("data.accountMemberSignIn.tokenType").ToString();
                    string scope = dynamicGqlResponse.RequireToken("data.accountMemberSignIn.scope").ToString();
                    string expiresIn = dynamicGqlResponse.RequireToken("data.accountMemberSignIn.expiresIn").ToString();
                    string accessToken = dynamicGqlResponse.RequireToken("data.accountMemberSignIn.accessToken").ToString();
                    string refreshToken = dynamicGqlResponse.RequireToken("data.accountMemberSignIn.refreshToken").ToString();
                    string idToken = dynamicGqlResponse.RequireToken("data.accountMemberSignIn.idToken").ToString();

                    Assert.True(tokenType == "Bearer", $"Token type returned was not Bearer as expected. It was returned as {tokenType}");
                    Assert.True(!scope.IsNullOrEmpty() && scope.Contains("openid offline_access email profile"), $"Scope returned was not openid profile as expected. It was returned as {scope}");
                    Assert.False(expiresIn.IsNullOrEmpty(), "ExpiresIn was empty");
                    Assert.False(accessToken.IsNullOrEmpty(), "AccessToken was empty");
                    Assert.False(refreshToken.IsNullOrEmpty(), "RefreshToken was empty");
                    Assert.True(accessToken != refreshToken, $"Member '{memberNumber}': expected distinct access and refresh tokens, but found matching token state '{(accessToken == refreshToken ? "<matching>" : "<distinct>")}'.");
                    Assert.False(idToken.IsNullOrEmpty(), "IdToken was empty");
                }
            }, 5, 500);

            //Wings Award Creation - Only for Gold members who are Adults(As Status is Active for Age>18. This is to ensure that we have test data created with wings awards for future testing and to validate the same in Cosmos DB.
            if (tierSet.Is("Gold") && isWings)
            {
                await RetryHelper.RunWithRetryAsync(async () =>
                {
                    DateTime currentDate = DateTime.UtcNow;

                    string startDate = currentDate.ToString("MM/dd/yyyy");

                    HttpResponseMessage response = Automation.Core.Helpers.Requests.Endpoints.AddOrUpdateAwardToTheMember(operationIdentifier, customerId, memberNumber,
                        firstName, "Active", firstName, DateTime.Parse(dateOfBirth).ToString("MM/dd/yyyy"), firstName, lastName,
                        "Mr", $"Dear {lastName}", wingsAwardProductName, startDate).EnsureIsSuccessStatusCode();

                    JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());

                    string? errorCodeFromResponse = result.SelectToken("data.errorCode")?.ToString();
                    Assert.True(string.IsNullOrEmpty(errorCodeFromResponse), $"The Award Creation was not successful. The response returned was {result}");

                    string queryForAwardDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey and c.ProductId = @productId order by c._ts desc";

                    QueryDefinition awardDocQuery = new QueryDefinition(query: queryForAwardDoc).WithParameter("@docType", "Award").WithParameter("@partitionKey", customerId).WithParameter("@productId", wingsAwardProductId);

                    Award awardDoc = null;

                    RetryHelper.RunWithRetry(() =>
                     {
                         awardDoc = cosmosFixture.CosmosQueryItem<Award>(awardDocQuery);

                         Assert.True(awardDoc is not null, $"Award Document not found for the member {customerId}");

                     }, 5, 2000);

                    Assert.True(awardDoc.ProductId.Equals(wingsAwardProductId),
                        $"Expected the Voucher ProductId to be {wingsAwardProductId} but was {awardDoc.ProductId} .");

                    Assert.True(awardDoc.StartDate.ToString("MM/dd/yyyy").Equals(startDate),
                        $"Expected the Voucher StartDate to be {startDate} but was {awardDoc.StartDate.ToString("yyyy-MM-dd")} .");

                    string expiryDate = awardDoc.ExpiryDate == DateTime.MinValue ? "" : awardDoc.ExpiryDate.ToString("yyyy-MM-dd");

                    Assert.True(string.IsNullOrEmpty(expiryDate),
                        $"Expected the Voucher EndDate to be Null or Empty but was {awardDoc.ExpiryDate} .");

                }, 2, 1000);
            }

            //Cobrand test case will be identified with first name and last name as "Joe" and "Approval" respectively and this info will be stored in storage table for reference. This is to easily identify the test data created for Cobrand testing without having to check other parameters.
            bool isCobrandTest = !fName.IsNullOrEmpty();

            //Add to storage table post updates
            await AddLoyaltyAccountToStorageTable(LoyaltyAccountsTableName, memberNumber, env, tierSet, points, firstName, lastName, email, dateOfBirth, addressDetails.CountryCode == "GB" ? "UK" : addressDetails.CountryCode, underSixteen, isCobrandTest, isWings, forHouseHold);
        }

        [Theory]
        [Trait("Category", Traits.LoyaltyAccountDataCreation)]
        [MemberData(nameof(LoyaltyAccountCreationTestData.EnrolmentCases), MemberType = typeof(LoyaltyAccountCreationTestData))]
        public async Task FCEntraEnrolment(FlyingClubMemberTiers tier, int points, string fName = "", string lName = "", bool underSixteen = false, bool isWings = false, bool forHouseHold = false)
        {

            string firstName = string.IsNullOrEmpty(fName) ? DataGenerator.RandomFirstName() : fName;
            string lastName = string.IsNullOrEmpty(lName) ? DataGenerator.RandomLastName() : lName;
            // Under-sixteen cases use a minor date of birth so the enrolment payload and stored account classification agree.
            string dateOfBirth = underSixteen ? DateTime.UtcNow.AddYears(-15).AddMonths(Random.Shared.Next(1, 12)).ToString("yyyy-MM-dd") : DateTime.UtcNow.AddYears(-30).AddMonths(Random.Shared.Next(1, 12)).ToString("yyyy-MM-dd");
            string gender = Application.GenderTypes[Random.Shared.Next(0, Application.GenderTypes.Length)];
            string email = firstName.ToLower() + lastName.ToLower() + Random.Shared.Next(0, 10000) +
            "@example.com";
            string enrollmentChannel = "WEB-MYA";
            string mediaCode = "App";
            int maxAttempts = 10;
            int attempt = 0;
            string password = Application.LoginPassword;
            string tierSet = tier.ToString();

            int countryCode = Random.Shared.Next(0, 2);

            // If it is for household then it will use a fixed address else it will pick a random address from the list of addresses available in the EnrolmentHelper class.
            EnrolmentHelper.AddressWithPhoneCode addressDetails = forHouseHold ? new EnrolmentHelper.AddressWithPhoneCode(
                "Household",
                "Test Address Line 1",
                "Test Address Line 2",
                "London",
                "RH117DU",
                "GB",
                "44"
            ) : EnrolmentHelper.PickRandomAddress((EnrolmentHelper.Country)countryCode);

            EnrolmentHelper.PreferenceSet preferences = EnrolmentHelper.PickRandomPreferences();

            string phoneNumber = Random.Shared.NextInt64(7000000000, 9999999999).ToString("D10").TrimStart('0');

            // Get the enrolment token from the SIT OTP storage table for this test only.
            // SIT and UAT share the same tenant.
            TableStorageService<OtpRequestTableEntity> sitOtpRequestTableStorageService = new()
            {
                ConnectionString = Vault.Default.GetSecretValue("IdentityStorageAccountConnectionString-sit"),
                TableName = "OTPRequest"
            };

            string enrolmentToken = await EnrolmentHelper.ReturnEntraEnrolmentToken(sitOtpRequestTableStorageService, email);

            AccountEntraEnrolMemberRequest request = new()
            {
                EnrolmentToken = enrolmentToken,
                ChannelId = enrollmentChannel,
                CustomerType = CustomerType.Loyalty,
                Customer = new() { FirstName = firstName, LastName = lastName, DateOfBirth = dateOfBirth, Gender = gender, Password = password },
                Address = new() { AddressLine1 = addressDetails.AddressLine1, City = addressDetails.City, PostalCode = addressDetails.PostalCode, CountryCode = addressDetails.CountryCode },
                Contact = new() { EmailId = email, PhoneCountryCode = addressDetails.PhoneCountryCode, PhoneNumber = phoneNumber },
                LoyaltyMember = new() { EnrollmentChannel = enrollmentChannel, MediaCode = mediaCode },
                Preferences = new() { VaaMarketingPermission = preferences.VaaMarketingPermissions, VaPartnersMarketingPermission = preferences.VaPartnersMarketingPermissions, VaGroupMarketingPermissions = preferences.VaGroupMarketingPermissions, PreferredPartnerFFP = preferences.PreferredPartnerFFP }
            };

            //Invoke enrolment GRAPHQL endpoint
            HttpResponseMessage enrolmentResponse = null;
            JObject enrolmentResult = null;

            enrolmentResponse = GraphQL.AccountEntraEnrolMember(env, request);
            string enrolmentBody = await enrolmentResponse.Content.ReadAsStringAsync();
            enrolmentResult = JObject.Parse(enrolmentBody);
            Assert.False(enrolmentResult["errors"]?.Values().Any() == true, $"Member Email {email} , Errors were returned in the response. Error was: {System.Environment.NewLine} {enrolmentResult}");

            string idToken = enrolmentResult.RequireToken("data.accountEntraEnrolMember.idToken").ToString();

            string actualFirstName = enrolmentResult.RequireToken("data.accountEntraEnrolMember.firstName").ToString();

            string actualLastName = enrolmentResult.RequireToken("data.accountEntraEnrolMember.lastName").ToString();

            string memberNumber = enrolmentResult.RequireToken("data.accountEntraEnrolMember.memberNumber").ToString();

            Assert.True(actualFirstName.Equals(firstName), $"Expected First Name {firstName} but got {actualFirstName}");

            Assert.True(actualLastName.Equals(lastName), $"Expected Last Name {lastName} but got {actualLastName}");

            Assert.True(!memberNumber.IsNullOrEmpty(), $"Member Number was not returned in the response for the member with email {email}");

            // Get memberNumber + customerId via accountMemberDetails
            HttpResponseMessage memberDetailsResponse = GraphQL.AccountMemberDetails(env, idToken);
            string memberDetailsBody = await memberDetailsResponse.Content.ReadAsStringAsync();
            JObject memberDetails = JObject.Parse(memberDetailsBody);
            Assert.False(memberDetails["errors"]?.Values().Any() == true, $"Errors were returned in the response. Error was: {System.Environment.NewLine} {memberDetails}");

            //string memberNumber = memberDetails.RequireToken("data.accountMemberDetails.flyingClubMemberDetails.fCMembershipNumber").ToString();
            string customerId = memberDetails.RequireToken("data.accountMemberDetails.flyingClubMemberDetails.customerID").ToString();

            Assert.True(!customerId.IsNullOrEmpty(), $"Customer ID was not returned in the response for the member with email {email}");

            //Validate if member is enrolled into VRed
            await RetryHelper.RunWithRetryAsync(async () =>
            {

                HttpResponseMessage vRedResponseAPI = Microservice.VredPointsMsEndPoint(env, tokenFixture.GetToken("VRedMS"), memberNumber).EnsureIsSuccessStatusCode();

                dynamic vRedResultAPI = JsonConvert.DeserializeObject<dynamic>(await vRedResponseAPI.Content.ReadAsStringAsync());

                Assert.True(!string.IsNullOrEmpty(vRedResultAPI.Current.ToString()), $"Member '{memberNumber}': expected a populated VRed current balance, but found '{vRedResultAPI.Current}'.");

            }, 5, 4000);

            // Delta OCDB is an external downstream dependency and is not required to create
            // reusable loyalty test data. Do not let its availability block account creation.

            #region AIR4 XML request for DL Call and Validating if the member is synced in DL OCDB system

            string requestLoyaltyProfileSyncAdd;
            string doB = DateTime.Parse(dateOfBirth).ToString("yyyyMMdd");
            XmlDocument air4LoyaltyProfileSyncAddResponseDocument = null;
            await RetryHelper.RunWithRetryAsync(async () =>
            {

                string localRequest;
                air4LoyaltyProfileSyncAddResponseDocument = AIR4Helper.LoyaltyProfileSyncAdd(
                    _certificateFixture.GetCertificate(DeltaSoaCertificateKey),
                    memberNumber,
                    doB,
                    firstName,
                    lastName,
                    "M",
                    email,
                    out localRequest);
                requestLoyaltyProfileSyncAdd = localRequest;

            }, 3, 2000);

            XmlNamespaceManager air4LoyaltyProfileSyncAddResponseManager = air4LoyaltyProfileSyncAddResponseDocument.CreateXmlNamespaceManager();

            string errorCode = string.Empty;
            string errorText = string.Empty;
            string reasonCode = string.Empty;
            string reasonText = string.Empty;
            string freeformText = string.Empty;
            string messageNumber = string.Empty;
            string sscRequest = string.Empty;

            try
            {
                // First Sceanrio - Expected to get error code with Error attributes - Members should already be synced at DL end
                errorCode = air4LoyaltyProfileSyncAddResponseDocument.SelectSingleNode("//error:Error/@ErrorCode", air4LoyaltyProfileSyncAddResponseManager)?.InnerText;
                errorText = air4LoyaltyProfileSyncAddResponseDocument.SelectSingleNode("//error:Error/@ErrorText", air4LoyaltyProfileSyncAddResponseManager)?.InnerText;
                reasonCode = air4LoyaltyProfileSyncAddResponseDocument.SelectSingleNode("//error:Error/@ReasonCode", air4LoyaltyProfileSyncAddResponseManager)?.InnerText;
                reasonText = air4LoyaltyProfileSyncAddResponseDocument.SelectSingleNode("//error:Error/@ReasonText", air4LoyaltyProfileSyncAddResponseManager)?.InnerText;
            }
            catch (Exception)
            {
                //ignore
            }

            try
            {
                // Second scenario endpoint will sync at DL end and validate for AdvisoryMessage attributes (success case)
                freeformText = air4LoyaltyProfileSyncAddResponseDocument.SelectSingleNode("//advisorymessage:AdvisoryMessage/@FreeformText", air4LoyaltyProfileSyncAddResponseManager)?.InnerText;
                messageNumber = air4LoyaltyProfileSyncAddResponseDocument.SelectSingleNode("//advisorymessage:AdvisoryMessage/@MessageNumber", air4LoyaltyProfileSyncAddResponseManager)?.InnerText;
                sscRequest = air4LoyaltyProfileSyncAddResponseDocument.SelectSingleNode("//advisorymessage:AdvisoryMessage/@SscRequest", air4LoyaltyProfileSyncAddResponseManager)?.InnerText;
            }
            catch (Exception)
            {
                //ignore
            }

            // Check type of response we received and validate accordingly
            if (!freeformText.IsNullOrEmpty() && !messageNumber.IsNullOrEmpty() && !sscRequest.IsNullOrEmpty())
            {
                // Success case assertions
                Assert.True(freeformText.Equals("OK"), $"Expected FreeformText 'OK' for member number {memberNumber}. Actual value: {freeformText}");
                Assert.True(messageNumber.Equals("65535"), $"Expected MessageNumber '65535' for member number {memberNumber}. Actual value: {messageNumber}");
                Assert.True(sscRequest.Equals("FCFFSC"), $"Expected SscRequest 'FCFFSC' for member number {memberNumber}. Actual value: {sscRequest}");
            }
            else if (!errorCode.IsNullOrEmpty() && !errorText.IsNullOrEmpty() && !reasonCode.IsNullOrEmpty() && !reasonText.IsNullOrEmpty())
            {
                // Error case assertions
                Assert.True(errorCode.Equals("1501"), $"Expected error code 1501 received for ACCT PRESENT in OCDB for member number {memberNumber}. Actual value: {errorCode}");
                Assert.True(errorText.Equals("tpf error"), $"Expected ErrorText 'tpf error' for member number {memberNumber}. Actual value: {errorText}");
                Assert.True(reasonCode.Equals("65535"), $"Expected ReasonCode '65535' for member number {memberNumber}. Actual value: {reasonCode}");
                Assert.True(reasonText.Equals("FFSC FAILURE - ACCT ALREADY EXISTS"), $"Expected ReasonText 'FFSC FAILURE - ACCT ALREADY EXISTS' for member number {memberNumber}. Actual value: {reasonText}");
            }
            else
            {
                // Fail test for unexpected response
                string actualResponse = air4LoyaltyProfileSyncAddResponseDocument.OuterXml;
                Assert.Fail($"OCDB sync service is not working at DL end for member number {memberNumber}. Unexpected response received: {actualResponse}");
            }

            #endregion AIR4 XML request for DL Call and Validating if the member is synced in DL OCDB system


            //Upgrade tier using Loops API's if tier is Silver/Gold as enrolment will happen in Red tier by default, else if tier is Red then it will validate the same tier.
            await flyingClubMemberHelper.UpgradeTheFlyingClubMemberTier(memberNumber, tier);

            //Add points to the member account using Loops API's and validate the same in VRed with retry mechanism as it can take time to reflect in VRed.
            bool isTransactionCreated = await flyingClubMemberHelper.AddLoyaltyPointsToTheFlyingClubAccount(memberNumber, points);

            Assert.True(isTransactionCreated, $"Member '{memberNumber}': expected loyalty-points credit transaction creation to succeed, but found success '{isTransactionCreated}'.");

            long memberPointBalance = await flyingClubMemberHelper.GetPointBalanceOfFlyingClubAccount(memberNumber);

            while (attempt < maxAttempts)
            {
                if (memberPointBalance.Equals(points))
                    break;
                else
                {
                    memberPointBalance = await flyingClubMemberHelper.GetPointBalanceOfFlyingClubAccount(memberNumber);
                    attempt++;
                    await Task.Delay(1000);
                }
            }

            //Force a recall as this can sometimes take time to reflect.
            Assert.True(memberPointBalance.Equals(points), $"For the MemberNumber {memberNumber} Points balance did not match Vred after {maxAttempts} retries. Expected {points} and got {memberPointBalance}");

            //Validate if the member details are updated in Azure B2C using Graph API with retry mechanism as it can take time to reflect in Graph.
            await RetryHelper.RunWithRetryAsync(async () =>
            {
                HttpResponseMessage getUserResponse = MicrosoftGraph.GetUserDetailsByEmailOrFCNumber(tokenFixture.GetToken("azureGraphB2C"), email).EnsureIsSuccessStatusCode();

                AzureB2CResponse b2cResponse = JsonConvert.DeserializeObject<AzureB2CResponse>(await getUserResponse.Content.ReadAsStringAsync());

                Assert.True((b2cResponse?.azureB2CValues?.Count() ?? 0) == 1, $"Expected to find exactly one user in Azure B2C with email {email}, but found {b2cResponse?.azureB2CValues?.Count() ?? 0}.");

                Assert.True(b2cResponse.azureB2CValues[0]?.givenName == firstName, $"Member '{memberNumber}': expected Azure B2C given name '{firstName}', but found '{b2cResponse.azureB2CValues[0]?.givenName ?? "<missing>"}'.");
                Assert.True(b2cResponse.azureB2CValues[0]?.surname == lastName, $"Member '{memberNumber}': expected Azure B2C surname '{lastName}', but found '{b2cResponse.azureB2CValues[0]?.surname ?? "<missing>"}'.");
                Assert.True(b2cResponse.azureB2CValues[0]?.accountEnabled == true, $"Member '{memberNumber}': expected Azure B2C accountEnabled 'True', but found '{b2cResponse.azureB2CValues[0]?.accountEnabled.ToString() ?? "<missing>"}'.");
                Assert.True(b2cResponse.azureB2CValues[0]?.identities.FirstOrDefault(x => x.signInType == "emailAddress")?.issuerAssignedId == email, $"Member '{memberNumber}': expected Azure B2C email identity '{email}', but found '{b2cResponse.azureB2CValues[0]?.identities.FirstOrDefault(x => x.signInType == "emailAddress")?.issuerAssignedId ?? "<missing>"}'.");
                Assert.True(b2cResponse.azureB2CValues[0]?.identities.FirstOrDefault(x => x.signInType == "username")?.issuerAssignedId == memberNumber, $"Member '{memberNumber}': expected Azure B2C username identity '{memberNumber}', but found '{b2cResponse.azureB2CValues[0]?.identities.FirstOrDefault(x => x.signInType == "username")?.issuerAssignedId ?? "<missing>"}'.");


            }, 5, 2000);

            //Validate that the member can sign in using MemberSignIn GraphQL endpoint and validate the response with retry mechanism as it can take time to reflect in Graph.
            await RetryHelper.RunWithRetryAsync(async () =>
            {
                //invoke enrolment MemberSignIn GraphQL endpoint       
                HttpResponseMessage gqlResponse = GraphQL.AccountMemberSignIn(env, email, memberNumber, password).EnsureGQLResponseSuccess();

                JObject dynamicGqlResponse = JObject.Parse(await gqlResponse.Content.ReadAsStringAsync());

                bool requiresMFA = dynamicGqlResponse.SelectToken("data.accountMemberSignIn.requiresMFA")?.Value<bool>() == true;
                if (requiresMFA)
                {
                    string? expiresIn = dynamicGqlResponse.SelectToken("data.accountMemberSignIn.expiresIn")?.ToString();
                    string? accessToken = dynamicGqlResponse.SelectToken("data.accountMemberSignIn.accessToken")?.ToString();
                    string? refreshToken = dynamicGqlResponse.SelectToken("data.accountMemberSignIn.refreshToken")?.ToString();
                    string? idToken = dynamicGqlResponse.SelectToken("data.accountMemberSignIn.idToken")?.ToString();
                    string? continuationToken = dynamicGqlResponse.SelectToken("data.accountMemberSignIn.continuationToken")?.ToString();

                    Assert.True(expiresIn == null, $"Member '{memberNumber}': expected no MFA expiresIn value, but found '{expiresIn ?? "<null>"}'.");
                    Assert.True(idToken == null, $"Member '{memberNumber}': expected no MFA ID token, but found '{(idToken is null ? "<null>" : "<populated>")}'.");
                    Assert.True(accessToken == null, $"Member '{memberNumber}': expected no MFA access token, but found '{(accessToken is null ? "<null>" : "<populated>")}'.");
                    Assert.True(refreshToken == null, $"Member '{memberNumber}': expected no MFA refresh token, but found '{(refreshToken is null ? "<null>" : "<populated>")}'.");
                    Assert.False(continuationToken.IsNullOrEmpty(), "continuation token was empty");
                }
                else
                {
                    string tokenType = dynamicGqlResponse.RequireToken("data.accountMemberSignIn.tokenType").ToString();
                    string scope = dynamicGqlResponse.RequireToken("data.accountMemberSignIn.scope").ToString();
                    string expiresIn = dynamicGqlResponse.RequireToken("data.accountMemberSignIn.expiresIn").ToString();
                    string accessToken = dynamicGqlResponse.RequireToken("data.accountMemberSignIn.accessToken").ToString();
                    string refreshToken = dynamicGqlResponse.RequireToken("data.accountMemberSignIn.refreshToken").ToString();
                    string idToken = dynamicGqlResponse.RequireToken("data.accountMemberSignIn.idToken").ToString();

                    Assert.True(tokenType == "Bearer", $"Token type returned was not Bearer as expected. It was returned as {tokenType}");
                    Assert.True(!scope.IsNullOrEmpty() && scope.Contains("openid offline_access email profile"), $"Scope returned was not openid profile as expected. It was returned as {scope}");
                    Assert.False(expiresIn.IsNullOrEmpty(), "ExpiresIn was empty");
                    Assert.False(accessToken.IsNullOrEmpty(), "AccessToken was empty");
                    Assert.False(refreshToken.IsNullOrEmpty(), "RefreshToken was empty");
                    Assert.True(accessToken != refreshToken, $"Member '{memberNumber}': expected distinct access and refresh tokens, but found matching token state '{(accessToken == refreshToken ? "<matching>" : "<distinct>")}'.");
                    Assert.False(idToken.IsNullOrEmpty(), "IdToken was empty");
                }
            }, 5, 500);

            //Wings Award Creation - Only for Gold members who are Adults(As Status is Active for Age>18. This is to ensure that we have test data created with wings awards for future testing and to validate the same in Cosmos DB.
            if (tierSet.Is("Gold") && isWings)
            {
                await RetryHelper.RunWithRetryAsync(async () =>
                {
                    DateTime currentDate = DateTime.UtcNow;

                    string startDate = currentDate.ToString("MM/dd/yyyy");

                    HttpResponseMessage response = Automation.Core.Helpers.Requests.Endpoints.AddOrUpdateAwardToTheMember(operationIdentifier, customerId, memberNumber,
                        firstName, "Active", firstName, DateTime.Parse(dateOfBirth).ToString("MM/dd/yyyy"), firstName, lastName,
                        "Mr", $"Dear {lastName}", wingsAwardProductName, startDate).EnsureIsSuccessStatusCode();

                    JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());

                    string? errorCodeFromResponse = result.SelectToken("data.errorCode")?.ToString();
                    Assert.True(string.IsNullOrEmpty(errorCodeFromResponse), $"The Award Creation was not successful. The response returned was {result}");

                    string queryForAwardDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.PartitionKey = @partitionKey and c.ProductId = @productId order by c._ts desc";

                    QueryDefinition awardDocQuery = new QueryDefinition(query: queryForAwardDoc).WithParameter("@docType", "Award").WithParameter("@partitionKey", customerId).WithParameter("@productId", wingsAwardProductId);

                    Award awardDoc = null;

                    RetryHelper.RunWithRetry(() =>
                    {
                        awardDoc = cosmosFixture.CosmosQueryItem<Award>(awardDocQuery);

                        Assert.True(awardDoc is not null, $"Award Document not found for the member {customerId}");

                    }, 5, 2000);

                    Assert.True(awardDoc.ProductId.Equals(wingsAwardProductId),
                        $"Expected the Voucher ProductId to be {wingsAwardProductId} but was {awardDoc.ProductId} .");

                    Assert.True(awardDoc.StartDate.ToString("MM/dd/yyyy").Equals(startDate),
                        $"Expected the Voucher StartDate to be {startDate} but was {awardDoc.StartDate.ToString("yyyy-MM-dd")} .");

                    string expiryDate = awardDoc.ExpiryDate == DateTime.MinValue ? "" : awardDoc.ExpiryDate.ToString("yyyy-MM-dd");

                    Assert.True(string.IsNullOrEmpty(expiryDate),
                        $"Expected the Voucher EndDate to be Null or Empty but was {awardDoc.ExpiryDate} .");

                }, 2, 1000);
            }

            //Cobrand test case will be identified with first name and last name as "Joe" and "Approval" respectively and this info will be stored in storage table for reference. This is to easily identify the test data created for Cobrand testing without having to check other parameters.
            bool isCobrandTest = !fName.IsNullOrEmpty();

            //Add to storage table post updates
            await AddLoyaltyAccountToStorageTable(LoyaltyAccountsTableName, memberNumber, env, tierSet, points, firstName, lastName, email, dateOfBirth, addressDetails.CountryCode == "GB" ? "UK" : addressDetails.CountryCode, underSixteen, isCobrandTest, isWings, forHouseHold);
        }


        //This test is created only for the local run to help VRed team for fc data creation as per their requirement.
        //This test will read the data from the excel file and create the loyalty accounts and
        //write the flying club number back to the excel file for reference.
        [Fact]
        [Trait("Category", Traits.LocalRun)]
        public async Task FCEnrolment_ExcelTestData_CreatesLoyaltyAccountAndWritesFlyingClubNumber()
        {
            const string ExcelWorksheetName = "LoyaltyAccounts1";
            const int FirstNameColumn = 1;
            const int LastNameColumn = 2;
            const int DateOfBirthColumn = 3;
            const int EmailColumn = 4;
            const int FlyingClubNumberColumn = 5;
            const int RemarksColumn = 8;
            const string fileName = "LoyaltyAccountCreation.xlsx";

            string workbookPath = Path.Combine(currentWorkingDirectory, fileName); // File will be added at the time of execution
            using ExcelOperations excelOperations = new(workbookPath);

            int rowCount = excelOperations.GetWorkSheetRowCount(ExcelWorksheetName);

            for (int rowNumber = 2; rowNumber <= rowCount; rowNumber++)
            {
                string existingFlyingClubNumber = excelOperations.ReadData(ExcelWorksheetName, rowNumber, FlyingClubNumberColumn);
                if (!existingFlyingClubNumber.IsNullOrWhiteSpace())
                    continue;

                string firstName = excelOperations.ReadData(ExcelWorksheetName, rowNumber, FirstNameColumn);
                string lastName = excelOperations.ReadData(ExcelWorksheetName, rowNumber, LastNameColumn);
                string email = excelOperations.ReadData(ExcelWorksheetName, rowNumber, EmailColumn).Trim().ToLower();
                string dateOfBirth = excelOperations.ReadData(ExcelWorksheetName, rowNumber, DateOfBirthColumn);

                Assert.False(firstName.IsNullOrWhiteSpace(), $"Excel row {rowNumber} has no first name.");
                Assert.False(lastName.IsNullOrWhiteSpace(), $"Excel row {rowNumber} has no last name.");
                Assert.False(email.IsNullOrWhiteSpace(), $"Excel row {rowNumber} has no email address.");
                Assert.True(DateTime.TryParse(dateOfBirth, out DateTime parsedDateOfBirth), $"Excel row {rowNumber} has an invalid date of birth '{dateOfBirth}'.");

                string enrollmentChannel = "WEB-MYA";
                string gender = Application.GenderTypes[Random.Shared.Next(0, Application.GenderTypes.Length)];
                string phoneNumber = Random.Shared.NextInt64(7000000000, 9999999999).ToString("D10").TrimStart('0');
                EnrolmentHelper.Country country = Random.Shared.Next(0, 2) == 0 ? EnrolmentHelper.Country.GB : EnrolmentHelper.Country.US;
                EnrolmentHelper.AddressWithPhoneCode addressDetails = EnrolmentHelper.PickRandomAddress(country);
                EnrolmentHelper.PreferenceSet preferences = EnrolmentHelper.PickRandomPreferences();
                string enrolmentToken = await EnrolmentHelper.ReturnEnrolmentToken(otpRequestTableEntityTableStorageService, email);

                EnrolmentRequest request = new()
                {
                    EnrolmentToken = enrolmentToken,
                    EnrollmentChannel = enrollmentChannel,
                    MediaCode = "App",
                    ChannelId = enrollmentChannel,
                    TermsAndConditionsAccepted = false,
                    CustomerType = CustomerType.Loyalty,
                    Customer = new() { FirstName = firstName, LastName = lastName, DateOfBirth = parsedDateOfBirth.ToString("yyyy-MM-dd"), Gender = gender, Password = Application.LoginPassword },
                    Address = new() { AddressLine1 = addressDetails.AddressLine1, City = addressDetails.City, PostalCode = addressDetails.PostalCode, CountryCode = addressDetails.CountryCode },
                    Contact = new() { EmailId = email, PhoneCountryCode = addressDetails.PhoneCountryCode, PhoneNumber = phoneNumber },
                    Preferences = new() { VaaMarketingPermission = preferences.VaaMarketingPermissions, VaPartnersMarketingPermission = preferences.VaPartnersMarketingPermissions, VaGroupMarketingPermissions = preferences.VaGroupMarketingPermissions, PreferredPartnerFFP = preferences.PreferredPartnerFFP }
                };

                HttpResponseMessage enrolmentResponse = GraphQL.Enrolment(env, request);
                string enrolmentBody = await enrolmentResponse.Content.ReadAsStringAsync();
                JObject enrolmentResult = JObject.Parse(enrolmentBody);
                string? errorCode = enrolmentResult.SelectToken("errors[0].extensions.code")?.ToString();
                string? duplicateMemberMessage = enrolmentResult.SelectToken("errors[0].extensions.message")?.ToString();

                if (errorCode is not null && errorCode.Is("1012") && duplicateMemberMessage is not null && !duplicateMemberMessage.IsNullOrWhiteSpace())
                {
                    Match flyingClubNumberMatch = Regex.Match(duplicateMemberMessage, @"FCNumber\s*:\s*(\d+)");
                    Assert.True(flyingClubNumberMatch.Success, $"Excel row '{rowNumber}': expected a Flying Club number in the duplicate-member error, but found match success '{flyingClubNumberMatch.Success}'.");

                    string duplicateFlyingClubNumber = flyingClubNumberMatch.Groups[1].Value;
                    excelOperations.WriteData(ExcelWorksheetName, rowNumber, FlyingClubNumberColumn, duplicateFlyingClubNumber);
                    excelOperations.WriteData(ExcelWorksheetName, rowNumber, RemarksColumn, "Account was already registered in the past");
                    continue;
                }

                enrolmentResponse.EnsureGQLResponseSuccess();
                Assert.False(enrolmentResult["errors"]?.Values().Any() == true, $"Excel row {rowNumber} returned enrolment errors: {enrolmentResult}");

                string customerId = enrolmentResult.RequireToken("data.accountEnrolMember.customerId").ToString();
                string flyingClubNumber = enrolmentResult.RequireToken("data.accountEnrolMember.memberDetails.memberNumber").ToString();
                Assert.False(customerId.IsNullOrWhiteSpace(), $"Excel row {rowNumber} returned an empty customer ID.");
                Assert.False(flyingClubNumber.IsNullOrWhiteSpace(), $"Excel row {rowNumber} returned an empty Flying Club number.");

                await RetryHelper.RunWithRetryAsync(async () =>
                {
                    HttpResponseMessage vredMSCardLinks = Microservice.VRedMSCardLinks(env, tokenFixture.GetToken("VRedMS"), flyingClubNumber).EnsureIsSuccessStatusCode();

                    JObject response = JObject.Parse(await vredMSCardLinks.Content.ReadAsStringAsync());

                    string redUserId = response.RequireToken("redUserId").ToString();

                    Assert.True(redUserId.IsNullOrEmpty(), $"Expected an empty redUserId for a newly created Flying Club member, but found '{(redUserId.IsNullOrEmpty() ? "<empty>" : redUserId)}'.");

                }, 5, 2000);

                excelOperations.WriteData(ExcelWorksheetName, rowNumber, FlyingClubNumberColumn, flyingClubNumber);
            }
        }

        private static async Task AddLoyaltyAccountToStorageTable(string tableName, string loyaltyAccountNumber, string environment, string tier, int points, string firstName, string lastName, string email, string dateOfBirth, string CountryCode, bool isUnderEighteen, bool CoBrand = false, bool IsWings = false, bool ForHouseHold = false)
        {
            TableStorageService<LoyaltyAccountsTableEntity> tableStorageService = new()
            {
                TableName = tableName,
                ConnectionString = connectionString
            };

            await tableStorageService.InsertOrMergeAsync(new LoyaltyAccountsTableEntity()
            {
                PartitionKey = environment.ToUpper(),
                RowKey = loyaltyAccountNumber,
                Tier = tier,
                Points = points,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                DateOfBirth = dateOfBirth,
                UnderSixteen = isUnderEighteen,
                CountryAddress = CountryCode,
                CoBrand = CoBrand,
                IsWingsMember = IsWings,
                ForHouseHold = ForHouseHold,
                CreatedDate = DateTime.Now
            });
        }
    }
}
