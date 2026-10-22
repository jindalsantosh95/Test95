using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.MyAccount.Helpers;
using RestEndpoint = VAA.EAPI.Automation.Gateway.Requests.Endpoint.Rest;

namespace VAA.EAPI.Automation.Gateway.Tests.Rest
{
    public class AccountMemberDetailsTests
    {
        public string env = Config.Environment;

        // accountMemberSignIn has a documented MFA branch (requiresMFA: true → null
        // tokens, continuationToken instead) which EntraTokenHelper surfaces as
        // EntraMfaRequiredException. Random Red-tier picks can land on an MFA
        // member, so the storage-pick + sign-in is wrapped in the codebase's
        // RetryHelper.RunWithRetryAsync. The cap is intentionally small — a real
        // failure rate this high means the test pool needs an MFA filter, not
        // bigger retries. Note: RunWithRetryAsync catches *every* exception, so
        // real endpoint/config failures also incur up-to-cap retries before
        // surfacing; that's the codebase-idiomatic tradeoff.
        private const int MaxSignInAttempts = 5;

        [Fact]
        public async Task TC01_AccountMemberDetails_ReturnsMemberRecord()
        {
            string password = Vault.Default.GetSecretValue("MYA-Password");
            string fcNumber = string.Empty;
            string email = string.Empty;
            string token = string.Empty;

            await RetryHelper.RunWithRetryAsync(async () =>
            {
                // Storage-table-driven test data (see docs/TEST_DATA_STRATEGY.md): pull an
                // active Red-tier loyalty account whose row key IS the Flying Club number.
                // The helper filters by env on PartitionKey (case-insensitive via .Is()).
                Dictionary<string, string> loyaltyDetails =
                    RetrieveFromStorageTable.RetrieveLoyaltyAccountNumberDetailsFromStorageTable(tier: "Red", points: 0);

                if (loyaltyDetails.TryGetValue("error", out string? error))
                {
                    Assert.Fail($"Test case failed: {error}");
                }

                fcNumber = loyaltyDetails["rowKey"];
                email = loyaltyDetails["email"];
                Assert.False(string.IsNullOrEmpty(fcNumber), "Loyalty FC number (rowKey) should not be empty");
                Assert.False(string.IsNullOrEmpty(email), "Loyalty email should not be empty");

                // Throws EntraMfaRequiredException on MFA picks → caught by RetryHelper
                // → next iteration re-samples storage and tries again.
                token = await EntraTokenHelper.GetIdToken(env, email, fcNumber, password);
            }, maxRetries: MaxSignInAttempts);

            Assert.False(string.IsNullOrEmpty(token), $"Failed to obtain Entra id-token for FC number {fcNumber} after {MaxSignInAttempts} attempts");

            // Call the Gateway accountMemberDetails endpoint with the requested counts
            // (mirrors the payload the user specified: zero of each).
            HttpResponseMessage response = RestEndpoint.AccountMemberDetails(
                env,
                token,
                vouchersCount: 0,
                flightsCount: 0,
                activitiesCount: 0
            ).EnsureIsSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(responseBody), "AccountMemberDetails response body was empty");
            JObject memberDetails = JObject.Parse(responseBody);

            // The REST shim hoists the GraphQL fields to the root. Echo-back: the
            // membership number on the returned record must match the one we
            // authenticated as. (See docs/TEST_DATA_STRATEGY.md § Body assertions.)
            string actualMembershipNumber = memberDetails
                .RequireToken("flyingClubMemberDetails.fCMembershipNumber")
                .ToString();
            // .Is() — codebase idiom for case-insensitive string compare; null-safe
            // if actualMembershipNumber ever resolves to null/empty. (AGENTS.md
            // § Design conventions.)
            Assert.True(fcNumber.Is(actualMembershipNumber),
                $"FC membership number mismatch. Expected: {fcNumber} Actual: {actualMembershipNumber}");

            // Tier code and total points must be present — the whole point of the call.
            string tierCode = memberDetails
                .RequireToken("flyingClubMemberDetails.tierCode")
                .ToString();
            Assert.False(string.IsNullOrEmpty(tierCode), "tierCode should not be empty");

            JToken totalPoints = memberDetails.RequireToken("flyingClubMemberDetails.totalVirginPoints");
            Assert.NotNull(totalPoints);
        }
    }
}
