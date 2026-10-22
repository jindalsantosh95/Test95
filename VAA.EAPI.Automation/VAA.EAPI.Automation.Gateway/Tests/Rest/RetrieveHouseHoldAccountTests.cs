using System.Net;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.MyAccount.Helpers;
using RestEndpoint = VAA.EAPI.Automation.Gateway.Requests.Endpoint.Rest;

namespace VAA.EAPI.Automation.Gateway.Tests.Rest
{
    public class RetrieveHouseHoldAccountTests
    {
        public string env = Config.Environment;

        // Sign-in can transiently fail (network) or land on an MFA-gated member; the codebase
        // wraps it in RetryHelper. See AccountMemberDetailsTests for the fuller rationale.
        private const int MaxSignInAttempts = 5;

        // Household: authenticate as the household leader (a row in LoyaltyHouseHoldAccounts,
        // RowKey = leader FC) and retrieve scoped by that leader FC. The response carries a
        // leaderInfo block plus a linkedMemberInfo entry for each enrolled child.
        [Fact]
        public async Task TC01_RetrieveHouseHoldAccount_Household_ReturnsLeaderAndLinkedMembers()
        {
            string password = Vault.Default.GetSecretValue("MYA-Password");

            Dictionary<string, string> household =
                RetrieveFromStorageTable.RetrieveLoyaltyHouseHoldAccountFromStorageTable();
            if (household.TryGetValue("error", out string? error))
            {
                Assert.Fail($"Test case failed: {error}");
            }

            string leaderFcNumber = household["rowKey"];
            string leaderEmail = EnrolmentEmail(household["firstName"], household["lastName"]);
            Assert.False(string.IsNullOrEmpty(leaderFcNumber), "Household leader FC number (rowKey) should not be empty");

            string token = await SignIn(leaderEmail, leaderFcNumber, password);
            Assert.False(string.IsNullOrEmpty(token), $"Failed to obtain Entra id-token for leader FC {leaderFcNumber} after {MaxSignInAttempts} attempts");

            JObject response = await RetrieveHouseHold(token, memberType: "Household", houseHoldFcNumber: leaderFcNumber);

            // The leader on the returned record must be the member we authenticated as.
            string actualLeaderFc = response.RequireToken("leaderInfo.fcNumber").ToString();
            Assert.True(leaderFcNumber.Is(actualLeaderFc),
                $"leaderInfo.fcNumber mismatch. Expected: {leaderFcNumber} Actual: {actualLeaderFc}");

            // Every linked member returned must be one of the household's enrolled children.
            string[] expectedChildFcNumbers = household["childFcNumbers"]
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            JToken linkedMembers = response.RequireToken("linkedMemberInfo");
            Assert.NotEmpty(linkedMembers);
            foreach (JToken linkedMember in linkedMembers)
            {
                string linkedFc = linkedMember.RequireToken("fcNumber").ToString();
                Assert.Contains(linkedFc, expectedChildFcNumbers);
            }
        }

        // Linked: authenticate as a linked child member and retrieve scoped by the LEADER's FC
        // (the household identifier — passing the child's own FC returns a 500). The child's
        // sign-in email is derived from the name the leader's response exposes for them.
        [Fact]
        public async Task TC02_RetrieveHouseHoldAccount_Linked_ReturnsCallingMemberAsLinked()
        {
            string password = Vault.Default.GetSecretValue("MYA-Password");

            Dictionary<string, string> household =
                RetrieveFromStorageTable.RetrieveLoyaltyHouseHoldAccountFromStorageTable();
            if (household.TryGetValue("error", out string? error))
            {
                Assert.Fail($"Test case failed: {error}");
            }

            string leaderFcNumber = household["rowKey"];
            string leaderEmail = EnrolmentEmail(household["firstName"], household["lastName"]);

            // Sign in as the leader first, only to read a child's name + FC from linkedMemberInfo:
            // the table stores child FCs but not their names, and the child sign-in email needs the name.
            string leaderToken = await SignIn(leaderEmail, leaderFcNumber, password);
            Assert.False(string.IsNullOrEmpty(leaderToken), $"Failed to obtain Entra id-token for leader FC {leaderFcNumber}");

            JObject leaderView = await RetrieveHouseHold(leaderToken, "Household", leaderFcNumber);
            JToken linkedMembers = leaderView.RequireToken("linkedMemberInfo");
            Assert.NotEmpty(linkedMembers);
            JToken child = linkedMembers.First();
            string childFcNumber = child.RequireToken("fcNumber").ToString();
            string childEmail = EnrolmentEmail(
                child.RequireToken("firstName").ToString(),
                child.RequireToken("lastName").ToString());

            // Now authenticate AS the linked child and retrieve, still scoped by the leader FC.
            string childToken = await SignIn(childEmail, childFcNumber, password);
            Assert.False(string.IsNullOrEmpty(childToken), $"Failed to obtain Entra id-token for linked member FC {childFcNumber}");

            JObject response = await RetrieveHouseHold(childToken, "Linked", leaderFcNumber);

            // Same household leader, and the calling child appears under linkedMemberInfo.
            string actualLeaderFc = response.RequireToken("leaderInfo.fcNumber").ToString();
            Assert.True(leaderFcNumber.Is(actualLeaderFc),
                $"leaderInfo.fcNumber mismatch. Expected: {leaderFcNumber} Actual: {actualLeaderFc}");

            bool callerIsLinked = false;
            foreach (JToken linkedMember in response.RequireToken("linkedMemberInfo"))
            {
                if (linkedMember.RequireToken("fcNumber").ToString().Is(childFcNumber))
                {
                    callerIsLinked = true;
                    break;
                }
            }
            Assert.True(callerIsLinked, $"Calling linked member {childFcNumber} not found under linkedMemberInfo.");
        }

        // Individual: a member who is NOT in a household. The endpoint answers 200 with an empty
        // object — no leaderInfo block — which is the defining trait of a non-household account.
        [Fact]
        public async Task TC03_RetrieveHouseHoldAccount_Individual_ReturnsEmptyHousehold()
        {
            string password = Vault.Default.GetSecretValue("MYA-Password");

            // requireNonHousehold guarantees the sample really is individual; the verified
            // Individual response is handed back so we don't re-call the endpoint here.
            (string _, string _, JObject? individual) = await SignInAsPlainMember(password, requireNonHousehold: true);

            Assert.NotNull(individual);
            Assert.Null(individual!["leaderInfo"]);
        }

        // Negative: a member who is not a household leader cannot be resolved as a household.
        // The endpoint answers 500 with a fetch error — contrast TC03, where the SAME class of
        // member under the Individual memberType gets an empty 200 payload.
        [Fact]
        public async Task TC04_RetrieveHouseHoldAccount_Household_NonHouseholdMember_Returns500()
        {
            string password = Vault.Default.GetSecretValue("MYA-Password");
            (string fcNumber, string token, JObject? _) = await SignInAsPlainMember(password, requireNonHousehold: true);

            HttpResponseMessage response = RestEndpoint.RetrieveHouseHoldAccount(
                env, token, memberType: "Household", houseHoldFcNumber: fcNumber);

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            string body = await response.Content.ReadAsStringAsync();
            Assert.Contains("Error while fetching retrieve of House hold Details", body);
        }

        // Negative: the gateway requires the request key 'retrieveHHAccountRequest' (double H).
        // Sending the wrong casing is rejected with 400 and a named-parameter error. This guards
        // the casing fix in Factory.Rest.RetrieveHouseHoldAccount against silent regression.
        [Fact]
        public async Task TC05_RetrieveHouseHoldAccount_MalformedRequestKey_Returns400()
        {
            string password = Vault.Default.GetSecretValue("MYA-Password");
            // The 400 is about the request key, not the member, so any valid sign-in suffices.
            (string fcNumber, string token, JObject? _) = await SignInAsPlainMember(password);

            // Deliberately the wrong casing (single 'Hh') that the server does not recognise.
            string malformedBody = new JObject
            {
                ["retrieveHhAccountRequest"] = new JObject
                {
                    ["memberType"] = "Household",
                    ["houseHoldFcNumber"] = fcNumber
                }
            }.ToString();

            HttpResponseMessage response = RestEndpoint.RetrieveHouseHoldAccountRaw(env, token, malformedBody);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            string body = await response.Content.ReadAsStringAsync();
            Assert.Contains("Missing required parameter 'retrieveHHAccountRequest'", body);
        }

        // The household data provider enrols members with
        // email = firstName + lastName + "@example.com" (MyAccount/DataProvider/
        // LoyaltyHouseHoldAccountCreation.cs). The table persists the enrolled names but NOT the
        // email, so we reconstruct it the same way. Both that seeder and this method lowercase
        // with ToLowerInvariant, so the derived address matches the seeded one independent of the
        // runner's culture (e.g. the Turkish-I case). Caveat: rows seeded before the seeder was
        // aligned to ToLowerInvariant rely on that run having used an invariant/English culture.
        //
        // NOTE: this couples the test to that data provider's email convention. If the seeding
        // ever changes how emails are formed, these sign-ins break. The robust fix is to persist
        // an email column on the LoyaltyHouseHoldAccounts table and read it directly — until then
        // this derivation is the contract between the seeder and these tests.
        private static string EnrolmentEmail(string firstName, string lastName) =>
            $"{(firstName + lastName).ToLowerInvariant()}@example.com";

        // Picks a Red-tier member from the shared LoyaltyAccounts pool and signs in, validating
        // the sampled rowKey/email first. With requireNonHousehold it also confirms the member
        // has no household (retrieveHouseHoldAccount under Individual returns no leaderInfo) and
        // re-samples past any household-enrolled member that might sit in the pool — so the
        // Individual/negative scenarios don't flake on a stray household account. The verified
        // Individual response is returned (null when requireNonHousehold is false). RetryHelper
        // also re-samples on MFA / transient sign-in failures.
        private async Task<(string fcNumber, string token, JObject? individualResponse)> SignInAsPlainMember(
            string password, bool requireNonHousehold = false)
        {
            string fcNumber = string.Empty;
            string email = string.Empty;
            string token = string.Empty;
            JObject? individualResponse = null;

            await RetryHelper.RunWithRetryAsync(async () =>
            {
                Dictionary<string, string> loyaltyDetails =
                    RetrieveFromStorageTable.RetrieveLoyaltyAccountNumberDetailsFromStorageTable(tier: "Red", points: 0);
                if (loyaltyDetails.TryGetValue("error", out string? error))
                {
                    Assert.Fail($"Test case failed: {error}");
                }

                fcNumber = loyaltyDetails["rowKey"];
                email = loyaltyDetails["email"];
                Assert.False(string.IsNullOrEmpty(fcNumber), "Sampled loyalty FC number (rowKey) should not be empty");
                Assert.False(string.IsNullOrEmpty(email), "Sampled loyalty email should not be empty");

                token = await EntraTokenHelper.GetIdToken(env, email, fcNumber, password);

                if (requireNonHousehold)
                {
                    // If the random pick happens to be household-enrolled, leaderInfo is present —
                    // throw so RetryHelper re-samples a genuinely individual member.
                    individualResponse = await RetrieveHouseHold(token, "Individual", fcNumber);
                    if (individualResponse["leaderInfo"] != null)
                    {
                        throw new InvalidOperationException($"Sampled member {fcNumber} is household-enrolled; re-sampling for an individual.");
                    }
                }
            }, maxRetries: MaxSignInAttempts);

            Assert.False(string.IsNullOrEmpty(token), $"Failed to obtain Entra id-token for FC number {fcNumber} after {MaxSignInAttempts} attempts");
            return (fcNumber, token, individualResponse);
        }

        // Throws EntraMfaRequiredException on MFA picks / transient failures → caught by RetryHelper.
        private async Task<string> SignIn(string email, string fcNumber, string password)
        {
            string token = string.Empty;
            await RetryHelper.RunWithRetryAsync(async () =>
            {
                token = await EntraTokenHelper.GetIdToken(env, email, fcNumber, password);
            }, maxRetries: MaxSignInAttempts);
            return token;
        }

        private async Task<JObject> RetrieveHouseHold(string token, string memberType, string houseHoldFcNumber)
        {
            HttpResponseMessage response = RestEndpoint.RetrieveHouseHoldAccount(
                env, token, memberType: memberType, houseHoldFcNumber: houseHoldFcNumber)
                .EnsureIsSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrWhiteSpace(responseBody), "RetrieveHouseHoldAccount response body was empty");

            JObject parsed = JObject.Parse(responseBody);

            // The GraphQL-backed shim can return 200 with an errors-shaped body; assert on the
            // actual top-level property rather than a substring match (which a value containing
            // "errors" could trip).
            Assert.Null(parsed["errors"]);

            return parsed;
        }
    }
}
