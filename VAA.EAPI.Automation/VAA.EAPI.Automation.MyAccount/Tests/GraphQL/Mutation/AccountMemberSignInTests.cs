using System.Globalization;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Requests;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MyAccount.Constants;
using VAA.EAPI.Automation.MyAccount.Helpers;
using VAA.EAPI.Automation.MyAccount.Requests.Endpoint;
using static VAA.EAPI.Automation.MyAccount.Constants.Application;

namespace VAA.EAPI.Automation.MyAccount.Tests.GraphQlTests.Mutation
{
[Trait("Category", Traits.MyAccountGQLRegressionTests)]
[Trait("Category", Traits.IdentityMSRelatedGraphqltests)]
    public class AccountMemberSignInTests : IClassFixture<MemoryStoreFixture>
    {
        private static readonly string env = Config.Environment;
        private readonly MemoryStoreFixture memoryStoreFixture;

        public AccountMemberSignInTests(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;
            memoryStoreFixture.SetOnce("MYA-Password", () => Vault.Default.GetSecretValue("MYA-Password"));

        }

        [Theory]
        [InlineData("WEB-MYA", "Web")]
        [Trait("Category", Traits.GqlEntraAuthentication)]
        [Trait("Category", Traits.MemberManagementMSRelatedGraphqltests)]
        [Trait("Category", Traits.VRedMSRelatedGraphqltests)]
        [Trait("Category", Traits.CustomerMSRelatedGraphqltests)]
        [Trait("Category", Traits.CustomerTransactionMSRelatedGraphqltests)]
        public async Task TC01_ValidateAccountMemberSignIn(string enrollmentChannel, string mediaCode)
        {
            // Use helper to get email and loyalty number
            (string emailId, string loyaltyAccountNumber, _, _, _) = await EnrolmentHelper.GraphQLEnrolment(enrollmentChannel, mediaCode);

            string password = memoryStoreFixture.Get<string>("MYA-Password");
            // Retry the asserted sign-in while Entra finishes propagating the newly enrolled account.
            await RetryHelper.RunWithRetryAsync(async () =>
            {
                System.Net.Http.HttpResponseMessage gqlResponse = GraphQL.AccountMemberSignIn(env, emailId, loyaltyAccountNumber, password).EnsureGQLResponseSuccess();
                JObject dynamicGqlResponse = JObject.Parse(await gqlResponse.Content.ReadAsStringAsync());

                bool requiresMFA = dynamicGqlResponse.SelectToken("data.accountMemberSignIn.requiresMFA")?.Value<bool>() == true;
                if (requiresMFA)
                {
                    string? expiresIn = dynamicGqlResponse.SelectToken("data.accountMemberSignIn.expiresIn")?.ToString();
                    string? accessToken = dynamicGqlResponse.SelectToken("data.accountMemberSignIn.accessToken")?.ToString();
                    string? refreshToken = dynamicGqlResponse.SelectToken("data.accountMemberSignIn.refreshToken")?.ToString();
                    string? idToken = dynamicGqlResponse.SelectToken("data.accountMemberSignIn.idToken")?.ToString();
                    string? continuationToken = dynamicGqlResponse.SelectToken("data.accountMemberSignIn.continuationToken")?.ToString();

                    Assert.True(expiresIn == null);
                    Assert.True(idToken == null);
                    Assert.True(accessToken == null);
                    Assert.True(refreshToken == null);
                    Assert.False(continuationToken.IsNullOrEmpty(), "continuation token was empty");
                    return;
                }

                string tokenType = dynamicGqlResponse.RequireToken("data.accountMemberSignIn.tokenType").ToString();
                string scope = dynamicGqlResponse.RequireToken("data.accountMemberSignIn.scope").ToString();
                string expiresInNonMfa = dynamicGqlResponse.RequireToken("data.accountMemberSignIn.expiresIn").ToString();
                string accessTokenNonMfa = dynamicGqlResponse.RequireToken("data.accountMemberSignIn.accessToken").ToString();
                string refreshTokenNonMfa = dynamicGqlResponse.RequireToken("data.accountMemberSignIn.refreshToken").ToString();
                string idTokenNonMfa = dynamicGqlResponse.RequireToken("data.accountMemberSignIn.idToken").ToString();

                Assert.True(tokenType == "Bearer", $"Token type returned was not Bearer as expected. It was returned as {tokenType}");
                Assert.True(scope.Contains("openid offline_access email profile"), $"Scope returned was not openid profile as expected. It was returned as {scope}");
                Assert.False(expiresInNonMfa.IsNullOrEmpty(), "ExpiresIn was empty");
                Assert.False(accessTokenNonMfa.IsNullOrEmpty(), "AccessToken was empty");
                Assert.False(refreshTokenNonMfa.IsNullOrEmpty(), "RefreshToken was empty");
                Assert.True(accessTokenNonMfa != refreshTokenNonMfa, "Access and Refresh tokens were the same and should not be");
                Assert.False(idTokenNonMfa.IsNullOrEmpty(), "IdToken was empty");

                GraphQL.AccountMemberDetails(env, idTokenNonMfa).EnsureGQLResponseSuccess();
            }, maxRetries: 5, delay: 1000);



        }

        [Theory]
        [InlineData("Gold")]
        [InlineData("Silver")]
        [Trait("Category", Traits.GqlEntraAuthentication)]
        [Trait("Category", Traits.CustomerMSRelatedGraphqltests)]
        public async Task TC02_ValidateAccountMemberSignInTierMember(string tier)
        {
            (string emailId, string loyaltyAccountNumber) = GetStoredLoyaltyAccountCredentials(tier);
            string password = memoryStoreFixture.Get<string>("MYA-Password");

            //invoke enrolment MemberSignIn GraphQL endpoint       
            System.Net.Http.HttpResponseMessage gqlResponse = GraphQL.AccountMemberSignIn(env, emailId, loyaltyAccountNumber, password).EnsureGQLResponseSuccess();

            JObject dynamicGqlResponse = JObject.Parse(await gqlResponse.Content.ReadAsStringAsync());

            string tokenType = dynamicGqlResponse.RequireToken("data.accountMemberSignIn.tokenType").ToString();
            string scope = dynamicGqlResponse.RequireToken("data.accountMemberSignIn.scope").ToString();
            string expiresIn = dynamicGqlResponse.RequireToken("data.accountMemberSignIn.expiresIn").ToString();
            string accessToken = dynamicGqlResponse.RequireToken("data.accountMemberSignIn.accessToken").ToString();
            string refreshToken = dynamicGqlResponse.RequireToken("data.accountMemberSignIn.refreshToken").ToString();
            string idToken = dynamicGqlResponse.RequireToken("data.accountMemberSignIn.idToken").ToString();

            Assert.True(tokenType == "Bearer", $"Token type returned was not Bearer as expected. It was returned as {tokenType}");
            Assert.True(scope.Contains("openid offline_access email profile"), $"Scope returned was not openid profile as expected. It was returned as {scope}");
            Assert.False(expiresIn.IsNullOrEmpty(), "ExpiresIn was empty");
            Assert.False(accessToken.IsNullOrEmpty(), "AccessToken was empty");
            Assert.False(refreshToken.IsNullOrEmpty(), "RefreshToken was empty");
            Assert.True(accessToken != refreshToken, "Access and Refresh tokens were the same and should not be");
            Assert.False(idToken.IsNullOrEmpty(), "IdToken was empty");
        }

        [Fact]
        [Trait("Category", Traits.GqlEntraAuthentication)]
        [Trait("Category", Traits.CustomerMSRelatedGraphqltests)]
        public async Task TC03_ValidateAccountMemberSignInInvalidEmail()
        {
            // Use no member number so the invalid email cannot be resolved through a valid account identifier.
            string emailId = "abc.com";
            string password = memoryStoreFixture.Get<string>("MYA-Password");

            System.Net.Http.HttpResponseMessage gqlResponse = GraphQL.AccountMemberSignIn(env, emailId, string.Empty, password);

            JObject dynamicGqlResponse = JObject.Parse(await gqlResponse.Content.ReadAsStringAsync());

            JToken error = dynamicGqlResponse.RequireToken("errors[0]");

            Assert.Equal("Error during account member Sign In, invalid username or password", error.RequireToken("message").ToString());
            Assert.Equal("Unauthorized", error.RequireToken("extensions.code").ToString());
            Assert.Equal(401, error.RequireToken("extensions.httpStatusCode").Value<int>());
            Assert.Equal("Invalid username or password", error.RequireToken("extensions.Exception").ToString());
        }

        [Theory]
        [InlineData("WEB-MYA", "Web")]
        [Trait("Category", Traits.GqlEntraAuthentication)]
        [Trait("Category", Traits.CustomerMSRelatedGraphqltests)]
        public async Task TC04_ValidateAccountMemberSignInInvalidPwd(string enrollmentChannel, string mediaCode)
        {
            // Use helper to get email and loyalty number
            //invalid password
            (string emailId, string loyaltyAccountNumber, _, _, _) = await EnrolmentHelper.GraphQLEnrolment(enrollmentChannel, mediaCode);
            string password = "Password123";

            // Retry the asserted invalid-password response while Entra finishes propagating the newly enrolled account.
            await RetryHelper.RunWithRetryAsync(async () =>
            {
                System.Net.Http.HttpResponseMessage gqlResponse = GraphQL.AccountMemberSignIn(env, emailId, loyaltyAccountNumber, password);
                JObject dynamicGqlResponse = JObject.Parse(await gqlResponse.Content.ReadAsStringAsync());
                JToken error = dynamicGqlResponse.RequireToken("errors[0]");

                Assert.Equal("Account member SignIn failed", error.RequireToken("message").ToString());
                Assert.Equal("50126", error.RequireToken("extensions.code").ToString());
                Assert.False(error.RequireToken("extensions.message").ToString().IsNullOrEmpty(), "Message was empty");
                Assert.Equal("Entra Api", error.RequireToken("extensions.source").ToString());
            }, maxRetries: 5, delay: 1000);
        }

        [Theory]
        [InlineData(LoyaltyMemberStatus.Expired, "WEB-MYA", "Web")]
        [InlineData(LoyaltyMemberStatus.Frozen, "WEB-MYA", "Web")]
        [InlineData(LoyaltyMemberStatus.Deceased, "WEB-MYA", "Web")]
        [InlineData(LoyaltyMemberStatus.Suspended, "WEB-MYA", "Web")]
        [InlineData(LoyaltyMemberStatus.Cancelled, "WEB-MYA", "Web")]
        [InlineData(LoyaltyMemberStatus.Fraud, "WEB-MYA", "Web")]
        [Trait("Category", Traits.GqlEntraAuthentication)]
        [Trait("Category", Traits.CustomerMSRelatedGraphqltests)]
        public async Task TC05_ValidateAccountMemberSignInValidation(LoyaltyMemberStatus status, string enrollmentChannel, string mediaCode)
        {
            // Keep the enrolled DOB because the status update must identify the same member record.
            (string emailId, string loyaltyAccountNumber, string customerId, string firstName, string lastName, _, DateTime dateOfBirth, _, _) = await EnrolmentHelper.EntraEnrolment(enrollmentChannel, mediaCode);

            string password = memoryStoreFixture.Get<string>("MYA-Password");
            string formattedDateOfBirth = dateOfBirth.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);

            // Enrolment projects asynchronously, so retry the status update only until the new member is queryable.
            await RetryHelper.RunWithRetryAsync(async () =>
            {
                using System.Net.Http.HttpResponseMessage updateStatusResponse = Endpoints.UpdateFCMemberStatus(loyaltyAccountNumber, customerId, firstName, lastName, formattedDateOfBirth, status.ToString(), "Mr");
                Assert.True(updateStatusResponse.IsSuccessStatusCode, $"Expected member status update to return a successful HTTP status, but found '{updateStatusResponse.StatusCode}'.");
                JObject updateStatusResult = JObject.Parse(await updateStatusResponse.Content.ReadAsStringAsync());
                Assert.Equal(loyaltyAccountNumber, updateStatusResult.RequireToken("data.cardNumber").ToString());
            }, maxRetries: 5, delay: 2000);

            await RetryHelper.RunWithRetryAsync(async () =>
            {
                //invoke enrolment MemberSignIn GraphQL endpoint
                System.Net.Http.HttpResponseMessage gqlResponse = GraphQL.AccountMemberSignIn(env, emailId, loyaltyAccountNumber, password);

                JObject dynamicGqlResponse = JObject.Parse(await gqlResponse.Content.ReadAsStringAsync());

                JToken error = dynamicGqlResponse.RequireToken("errors[0]");

                Assert.Equal("Account member SignIn failed", error.RequireToken("message").ToString());
                Assert.Equal("601", error.RequireToken("extensions.code").ToString());
                Assert.Equal("Account did not meet the required status to log in", error.RequireToken("extensions.message").ToString());
            }, maxRetries: 5, delay: 2000);
        }

        [Theory]
        [InlineData("WEB-MYA", "Web")]
        [Trait("Category", Traits.GqlEntraAuthentication)]
        [Trait("Category", Traits.CustomerMSRelatedGraphqltests)]
        public async Task TC06_ValidateAccountMemberSignInInactiveMember(string enrollmentChannel, string mediaCode)
        {
            // Inactive is an allowed accountMemberSignIn status, despite being non-active for other member journeys.
            (string emailId, string loyaltyAccountNumber, string customerId, string firstName, string lastName, _, DateTime dateOfBirth, _, _) = await EnrolmentHelper.EntraEnrolment(enrollmentChannel, mediaCode);
            string password = memoryStoreFixture.Get<string>("MYA-Password");
            string formattedDateOfBirth = dateOfBirth.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

            using System.Net.Http.HttpResponseMessage updateStatusResponse = Endpoints.UpdateFCMemberStatus(loyaltyAccountNumber, customerId, firstName, lastName, formattedDateOfBirth, LoyaltyMemberStatus.Inactive.ToString(), "Mr");
            Assert.True(updateStatusResponse.IsSuccessStatusCode, $"Expected member status update to return a successful HTTP status, but found '{updateStatusResponse.StatusCode}'.");
            JObject updateStatusResult = JObject.Parse(await updateStatusResponse.Content.ReadAsStringAsync());
            Assert.Equal(loyaltyAccountNumber, updateStatusResult.RequireToken("data.cardNumber").ToString());

            await RetryHelper.RunWithRetryAsync(async () =>
            {
                using System.Net.Http.HttpResponseMessage gqlResponse = GraphQL.AccountMemberSignIn(env, emailId, loyaltyAccountNumber, password);
                Assert.True(gqlResponse.IsSuccessStatusCode, $"Expected accountMemberSignIn to return a successful HTTP status, but found '{gqlResponse.StatusCode}'.");
                JObject signInResult = JObject.Parse(await gqlResponse.Content.ReadAsStringAsync());
                Assert.False(signInResult["errors"] is JArray { Count: > 0 }, "Expected accountMemberSignIn to complete without GraphQL errors, but found an error response.");
                bool requiresMFA = signInResult.RequireToken("data.accountMemberSignIn.requiresMFA").Value<bool>();

                if (requiresMFA)
                {
                    string continuationToken = signInResult.RequireToken("data.accountMemberSignIn.continuationToken").ToString();
                    Assert.False(continuationToken.IsNullOrEmpty(), "Continuation token was empty for an inactive member requiring MFA.");
                    return;
                }

                string idToken = signInResult.RequireToken("data.accountMemberSignIn.idToken").ToString();
                string accessToken = signInResult.RequireToken("data.accountMemberSignIn.accessToken").ToString();
                string refreshToken = signInResult.RequireToken("data.accountMemberSignIn.refreshToken").ToString();

                Assert.False(idToken.IsNullOrEmpty(), "IdToken was empty for the inactive member.");
                Assert.False(accessToken.IsNullOrEmpty(), "AccessToken was empty for the inactive member.");
                Assert.False(refreshToken.IsNullOrEmpty(), "RefreshToken was empty for the inactive member.");
            }, maxRetries: 5, delay: 2000);
        }

        [Theory]
        [InlineData("WEB-MYA", "Web")]
        [Trait("Category", Traits.GqlEntraAuthentication)]
        [Trait("Category", Traits.CustomerMSRelatedGraphqltests)]
        public async Task TC07_ValidateRefreshTokenSuccess(string enrollmentChannel, string mediaCode)
        {
            // Use helper to get email and loyalty number
            (string emailId, string loyaltyAccountNumber, _, _, _) = await EnrolmentHelper.GraphQLEnrolment(enrollmentChannel, mediaCode);
            string password = memoryStoreFixture.Get<string>("MYA-Password");
            string accessToken = string.Empty;
            string refreshToken = string.Empty;

            // Retry only the immediate post-enrolment sign-in that supplies the refresh token.
            await RetryHelper.RunWithRetryAsync(async () =>
            {
                System.Net.Http.HttpResponseMessage gqlResponse = GraphQL.AccountMemberSignIn(env, emailId, loyaltyAccountNumber, password).EnsureGQLResponseSuccess();
                JObject dynamicGqlResponse = JObject.Parse(await gqlResponse.Content.ReadAsStringAsync());

                // Refresh tokens are issued only by the non-MFA sign-in branch for these fresh test accounts.
                bool requiresMFA = dynamicGqlResponse.RequireToken("data.accountMemberSignIn.requiresMFA").Value<bool>();
                Assert.False(requiresMFA, "Expected the fresh non-production account not to require MFA before refresh-token validation.");

                accessToken = dynamicGqlResponse.RequireToken("data.accountMemberSignIn.accessToken").ToString();
                refreshToken = dynamicGqlResponse.RequireToken("data.accountMemberSignIn.refreshToken").ToString();
            }, maxRetries: 5, delay: 1000);

            System.Net.Http.HttpResponseMessage refreshTokenResponse = GraphQL.AccountRefreshToken(env, refreshToken).EnsureGQLResponseSuccess();
            JObject dynamicRefreshTokenResponse = JObject.Parse(await refreshTokenResponse.Content.ReadAsStringAsync());

            string refreshedTokenType = dynamicRefreshTokenResponse.RequireToken("data.accountRefreshToken.tokenType").ToString();
            string refreshedScope = dynamicRefreshTokenResponse.RequireToken("data.accountRefreshToken.scope").ToString();
            string refreshedExpiresIn = dynamicRefreshTokenResponse.RequireToken("data.accountRefreshToken.expiresIn").ToString();
            string refreshedAccessToken = dynamicRefreshTokenResponse.RequireToken("data.accountRefreshToken.accessToken").ToString();
            string refreshedRefreshToken = dynamicRefreshTokenResponse.RequireToken("data.accountRefreshToken.refreshToken").ToString();
            string refreshedIdToken = dynamicRefreshTokenResponse.RequireToken("data.accountRefreshToken.idToken").ToString();

            Assert.Equal("Bearer", refreshedTokenType);
            Assert.Contains("openid offline_access email profile", refreshedScope);
            Assert.False(refreshedExpiresIn.IsNullOrEmpty(), "ExpiresIn was empty");
            Assert.False(refreshedAccessToken.IsNullOrEmpty(), "AccessToken was empty");
            Assert.False(refreshedRefreshToken.IsNullOrEmpty(), "RefreshToken was empty");
            Assert.False(refreshedIdToken.IsNullOrEmpty(), "IdToken was empty");
            Assert.NotEqual(accessToken, refreshedAccessToken);
            Assert.NotEqual(refreshToken, refreshedRefreshToken);
        }

        [Fact]
        [Trait("Category", Traits.GqlEntraAuthentication)]
        public async Task TC08_ValidateInvalidRefreshToken()
        {
            string refreshToken = "invalid token";

            System.Net.Http.HttpResponseMessage refreshTokenResponse = GraphQL.AccountRefreshToken(env, refreshToken);
            refreshTokenResponse.AssertIsSuccessStatusCode();
            JObject dynamicRefreshTokenResponse = JObject.Parse(await refreshTokenResponse.Content.ReadAsStringAsync());

            JToken error = dynamicRefreshTokenResponse.RequireToken("errors[0]");
            Assert.Equal("Generating Refresh Token failed", error.RequireToken("message").ToString());
            Assert.Equal("9002313", error.RequireToken("extensions.code").ToString());
            Assert.False(error.RequireToken("extensions.message").ToString().IsNullOrEmpty(), "Message was empty");
            Assert.Equal("Entra Api", error.RequireToken("extensions.source").ToString());
        }

        private static (string emailId, string loyaltyAccountNumber) GetStoredLoyaltyAccountCredentials(string tier)
        {
            // Resolve tier-specific test credentials from the current environment instead of fixed member records.
            Dictionary<string, string> loyaltyAccount = RetrieveFromStorageTable.FetchLoyaltyAccountInfoExcludingPoints(tier);

            Assert.False(loyaltyAccount.ContainsKey("error"), loyaltyAccount.GetValueOrDefault("error"));
            Assert.True(loyaltyAccount.TryGetValue("email", out string? emailId) && !emailId.IsNullOrWhiteSpace(), $"Expected a populated email address for the '{tier}' loyalty account.");
            Assert.True(loyaltyAccount.TryGetValue("rowKey", out string? loyaltyAccountNumber) && !loyaltyAccountNumber.IsNullOrWhiteSpace(), $"Expected a populated member number for the '{tier}' loyalty account.");

            return (emailId ?? string.Empty, loyaltyAccountNumber ?? string.Empty);
        }

    }
}
