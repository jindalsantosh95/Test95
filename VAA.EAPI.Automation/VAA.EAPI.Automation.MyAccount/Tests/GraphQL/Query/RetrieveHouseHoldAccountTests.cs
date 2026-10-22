using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MyAccount.Constants;
using VAA.EAPI.Automation.MyAccount.Helpers;
using VAA.EAPI.Automation.MyAccount.Requests.Endpoint;

namespace VAA.EAPI.Automation.MyAccount.Tests.GraphQlTests.Query
{
    public sealed class RetrieveHouseHoldAccountTests : IClassFixture<MemoryStoreFixture>
    {
        private readonly string env = Config.Environment;
        private readonly MemoryStoreFixture memoryStoreFixture;

        public RetrieveHouseHoldAccountTests(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;
            memoryStoreFixture.SetOnce("MYA-Password", () => Vault.Default.GetSecretValue("MYA-Password"));
            // Cache the rotating legacy-account password once per test class.
            memoryStoreFixture.SetOnce("MYA-LegacyPassword", () => Vault.Default.GetSecretValue("MYA-LegacyPassword"));
        }

        [Theory]
    [InlineData("APP-MYA", "App")]
    [Trait("Category", Traits.MyAccountGQLRegressionTests)]
    [Trait("Category", Traits.IdentityMSRelatedGraphqltests)]
    [Trait("Category", Traits.MemberManagementMSRelatedGraphqltests)]
    [Trait("Category", Traits.VRedMSRelatedGraphqltests)]
    [Trait("Category", Traits.CustomerMSRelatedGraphqltests)]
    [Trait("Category", Traits.CustomerTransactionMSRelatedGraphqltests)]
    public async Task TC01_RetrieveHouseHoldAccount_ReturnsHouseholdDetails(string enrollmentChannel, string mediaCode)
        {
            (string parentEmailId, string parentFlyingClubNumber, _, _, _) = await EnrolmentHelper.GraphQLEnrolment(enrollmentChannel, mediaCode, forHousehold: true);
            (string childEmailId, string childFlyingClubNumber, _, _, _) = await EnrolmentHelper.GraphQLEnrolment(enrollmentChannel, mediaCode, forHousehold: true);

            string password = memoryStoreFixture.Get<string>("MYA-Password");

            string parentIdToken = await EntraTokenHelper.GetIdToken(env, parentEmailId, parentFlyingClubNumber, password);
            string childIdToken = await EntraTokenHelper.GetIdToken(env, childEmailId, childFlyingClubNumber, password);

            HttpResponseMessage inviteResponse = GraphQL.AccountInviteMember(env, parentIdToken, childFlyingClubNumber, "Household").EnsureGQLResponseSuccess();
            string inviteBody = await inviteResponse.Content.ReadAsStringAsync();
            JObject inviteResult = JObject.Parse(inviteBody);
            JToken inviteMessage = inviteResult.RequireToken("data.accountInviteMember");
            Assert.NotEqual(JTokenType.Null, inviteMessage.Type);

            HttpResponseMessage acceptResponse = GraphQL.AccountInviteMemberAction(env, childIdToken, "Accepted", parentFlyingClubNumber, childFlyingClubNumber).EnsureGQLResponseSuccess();
            string acceptBody = await acceptResponse.Content.ReadAsStringAsync();
            JObject acceptResult = JObject.Parse(acceptBody);
            JToken acceptMessage = acceptResult.RequireToken("data.accountInviteMemberAction");
            Assert.NotEqual(JTokenType.Null, acceptMessage.Type);

            await RetryHelper.RunWithRetryAsync(async () =>
            {
                HttpResponseMessage retrieveResponse = GraphQL.RetrieveHouseHoldAccount(env, parentIdToken, "", "Household").EnsureGQLResponseSuccess();
                string retrieveBody = await retrieveResponse.Content.ReadAsStringAsync();
                JObject retrieveResult = JObject.Parse(retrieveBody);

                string leaderNumber = retrieveResult.RequireToken("data.retrieveHouseHoldAccount.leaderInfo.fCNumber").ToString();
                Assert.Equal(parentFlyingClubNumber, leaderNumber);

                // Match the invited adult by FC number so the age-status assertion is independent of response ordering.
                JArray linkedMembers = Assert.IsType<JArray>(retrieveResult.RequireToken("data.retrieveHouseHoldAccount.linkedMemberInfo"));
                JObject linkedMember = Assert.IsType<JObject>(linkedMembers.Single(member =>
                    childFlyingClubNumber.Is(member.RequireToken("fCNumber").ToString())));
                bool isYoungFlyer = linkedMember.RequireToken("isYoungFlyer").Value<bool>();

                Assert.False(isYoungFlyer,
                    $"Normal linked member '{childFlyingClubNumber}' should return isYoungFlyer=false.");
            }, 8, 2000);
        }

    [Fact]
    [Trait("Category", Traits.MyAccountGQLRegressionTests)]
    [Trait("Category", Traits.IdentityMSRelatedGraphqltests)]
    [Trait("Category", Traits.VRedMSRelatedGraphqltests)]
    [Trait("Category", Traits.CustomerMSRelatedGraphqltests)]
    [Trait("Category", Traits.CustomerTransactionMSRelatedGraphqltests)]
    public async Task TC02_RetrieveHouseHoldAccount_ReturnsStoredLeaderAndLinkedMembers()
        {
            Dictionary<string, string> household = RetrieveFromStorageTable.RetrieveLoyaltyHouseHoldAccountFromStorageTable();
            Assert.False(household.ContainsKey("error"), household.GetValueOrDefault("error"));

            string leaderMemberNumber = household["rowKey"];
            // Child FC numbers are persisted as a delimiter-separated value in the household storage row.
            string[] expectedChildMemberNumbers = household["childFcNumbers"]
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            Assert.NotEmpty(expectedChildMemberNumbers);

            string legacyPassword = memoryStoreFixture.Get<string>("MYA-LegacyPassword");
            string leaderToken = await EntraTokenHelper.GetIdTokenWithPasswordFallback(
                env,
                string.Empty,
                leaderMemberNumber,
                Application.LoginPassword,
                legacyPassword);
            HttpResponseMessage response = GraphQL.RetrieveHouseHoldAccount(env, leaderToken, string.Empty, "Household").EnsureGQLResponseSuccess();
            string body = await response.Content.ReadAsStringAsync();
            JObject result = JObject.Parse(body);

            Assert.True(
                result.RequireToken("data.retrieveHouseHoldAccount.leaderInfo.fCNumber").ToString().Is(leaderMemberNumber),
                $"Expected household leader '{leaderMemberNumber}' to be returned.");

            JArray linkedMembers = Assert.IsType<JArray>(result.RequireToken("data.retrieveHouseHoldAccount.linkedMemberInfo"));
            IEnumerable<string> actualChildMemberNumbers = linkedMembers
                .Select(member => member.RequireToken("fCNumber").ToString());
            foreach (string expectedChildMemberNumber in expectedChildMemberNumbers)
            {
                Assert.Contains(expectedChildMemberNumber, actualChildMemberNumbers);
            }
        }

    }
}

