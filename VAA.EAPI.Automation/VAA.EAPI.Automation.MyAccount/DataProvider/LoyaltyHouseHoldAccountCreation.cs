using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.AzureStorageTable;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MyAccount.Constants;
using VAA.EAPI.Automation.MyAccount.Helpers;
using VAA.EAPI.Automation.MyAccount.Models.GraphQL;
using VAA.EAPI.Automation.MyAccount.Requests.Endpoint;
using VAA.EAPI.Infrastructure.AzureStorage;
using Application = VAA.EAPI.Automation.MyAccount.Constants.Application;

namespace VAA.EAPI.Automation.MyAccount.DataProvider
{
    [Collection("Sequential")]
    public class LoyaltyHouseHoldAccountCreation : IClassFixture<TokenFixture>
    {
        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        public FlyingClubMemberHelper flyingClubMemberHelper;

        public static string env = Config.Environment;
        public static string password = Application.LoginPassword;
        public static string connectionString = Vault.Default.GetSecretValue("AzureStoragePnrCreationsConnectionString");

        private readonly TokenFixture _tokenFixture;


        public LoyaltyHouseHoldAccountCreation(TokenFixture tokenFixture)
        {
            flyingClubMemberHelper = new();

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            _tokenFixture = tokenFixture;
            _tokenFixture.AddTokenForVRedMicroserviceFromKeyVault("VRedMS");
        }


        #region Create household Accounts 
        [Theory]
        [InlineData(FlyingClubMemberTiers.Red, 1)]
        [InlineData(FlyingClubMemberTiers.Silver, 2)]
        [InlineData(FlyingClubMemberTiers.Gold, 3)]
        [InlineData(FlyingClubMemberTiers.Red, 2)]
        [InlineData(FlyingClubMemberTiers.Silver, 1)]
        [InlineData(FlyingClubMemberTiers.Gold, 2)]
        [InlineData(FlyingClubMemberTiers.Red, 3)]
        [InlineData(FlyingClubMemberTiers.Silver, 3)]
        [InlineData(FlyingClubMemberTiers.Gold, 1)]
        [InlineData(FlyingClubMemberTiers.Red, 4)]
        [InlineData(FlyingClubMemberTiers.Silver, 4)]
        [InlineData(FlyingClubMemberTiers.Gold, 4)]
        [Trait("Category", Traits.LoyaltyHouseHoldAccountDataCreation)]
        public async Task ValidateAcceptHouseholdAccountInviteAndRetrieve(FlyingClubMemberTiers tierCode, int noOfChilds)
        {
            string enrollmentChannel = "WEB-MYA";
            string mediaCode = "WEB";
            int expectedChildCount = noOfChilds;
            List<string> expectedChildMemberNumbers = new();

            (_, string parentMemberNumber, _, _, _) = await EnrolmentHelper.GraphQLEnrolment(enrollmentChannel, mediaCode, forHousehold: true);

            Assert.True(!string.IsNullOrEmpty(parentMemberNumber), $"Expected a populated parent member number after enrolment, but found '{(string.IsNullOrEmpty(parentMemberNumber) ? "<empty>" : parentMemberNumber)}'.");

            if (tierCode != FlyingClubMemberTiers.Red)
            {
                await flyingClubMemberHelper.UpgradeTheFlyingClubMemberTier(parentMemberNumber, tierCode);
            }

            // Parent's token (inviter)
            string tokenForParentAccountDetails = await EntraTokenHelper.GetIdToken(env, "", parentMemberNumber, password);

            Assert.True(!string.IsNullOrEmpty(tokenForParentAccountDetails), $"Parent member '{parentMemberNumber}': expected a populated login token, but found '{(string.IsNullOrEmpty(tokenForParentAccountDetails) ? "<empty>" : "<populated>")}'.");

            JObject? retrieveHouseHoldLeaderResult = null;

            while (noOfChilds-- > 0)
            {
                // 1) create child Account
                (_, string childMemberNumber, _, _, _) = await EnrolmentHelper.GraphQLEnrolment(enrollmentChannel, mediaCode, forHousehold: true);

                Assert.True(!string.IsNullOrEmpty(childMemberNumber), $"Expected a populated child member number after enrolment, but found '{(string.IsNullOrEmpty(childMemberNumber) ? "<empty>" : childMemberNumber)}'.");

                expectedChildMemberNumbers.Add(childMemberNumber);

                string tokenForChildAccountDetails = await EntraTokenHelper.GetIdToken(env, "", childMemberNumber, password);

                Assert.True(!string.IsNullOrEmpty(tokenForChildAccountDetails), $"Child member '{childMemberNumber}': expected a populated login token, but found '{(string.IsNullOrEmpty(tokenForChildAccountDetails) ? "<empty>" : "<populated>")}'.");

                // Poll a read-only endpoint until the child account is available before inviting it.
                await RetryHelper.RunWithRetryAsync(async () =>
                {
                    HttpResponseMessage memberDetails = GraphQL.AccountMemberDetails(env, tokenForChildAccountDetails).EnsureGQLResponseSuccess();

                    JObject memberDetailsResult = JObject.Parse(await memberDetails.Content.ReadAsStringAsync());

                    memberDetailsResult.RequireToken("data.accountMemberDetails.flyingClubMemberDetails");
                }, 8, 2000);

                // 2) Invite only once: retrying a non-idempotent mutation can create an invalid duplicate request.
                System.Net.Http.HttpResponseMessage inviteResp = GraphQL.AccountInviteMember(env, tokenForParentAccountDetails, childMemberNumber, "Household").EnsureGQLResponseSuccess();

                string inviteContent = await inviteResp.Content.ReadAsStringAsync();
                JObject inviteResult = JObject.Parse(inviteContent);

                string successMessage = "Successfully sent household invitation";
                Assert.Equal(successMessage, inviteResult.RequireToken("data.accountInviteMember").ToString());

                // 3) Accept only once for the same reason as the invitation mutation.
                System.Net.Http.HttpResponseMessage acceptResp = GraphQL.AccountInviteMemberAction(env, tokenForChildAccountDetails, "Accepted", parentMemberNumber, childMemberNumber).EnsureGQLResponseSuccess();

                string acceptContent = await acceptResp.Content.ReadAsStringAsync();

                JObject acceptResult = JObject.Parse(acceptContent);

                string acceptSuccessMessage = "Successfully Accepted HouseHold Invite";

                Assert.Equal(acceptSuccessMessage, acceptResult.RequireToken("data.accountInviteMemberAction").ToString());

                // 4) Poll / retry retrieval until the child shows up in child's account details with correct member type and head of group number
                await RetryHelper.RunWithRetryAsync(async () =>
                {
                    HttpResponseMessage memberDetails = GraphQL.AccountMemberDetails(env, tokenForChildAccountDetails).EnsureGQLResponseSuccess();

                    AccountMemberDetailsResponse accountMemberDetailsResponse = JsonConvert.DeserializeObject<AccountMemberDetailsResponse>(await memberDetails.Content.ReadAsStringAsync());

                    Assert.True(accountMemberDetailsResponse.Data.AccountMemberDetails != null, $"Child member '{childMemberNumber}': expected AccountMemberDetailsResponse data, but found '{(accountMemberDetailsResponse.Data.AccountMemberDetails is null ? "<null>" : "<populated>")}'.");

                    string memberType = accountMemberDetailsResponse.Data.AccountMemberDetails.FlyingClubMemberDetails.MemberType;

                    string headOfGroupMemberNumber = accountMemberDetailsResponse.Data.AccountMemberDetails.FlyingClubMemberDetails.HeadOfGroupMemberNumber ?? string.Empty;

                    Assert.True(memberType == "Linked", $"Child member '{childMemberNumber}': expected member type 'Linked' after accepting the household invite, but found '{memberType}'.");

                    Assert.True(headOfGroupMemberNumber == parentMemberNumber, $"Child member '{childMemberNumber}': expected head-of-group member number '{parentMemberNumber}', but found '{headOfGroupMemberNumber}'.");

                }, 8, 2000);

                // 5) Poll / retry retrieval until the parent shows up in parent's account details with correct member type
                await RetryHelper.RunWithRetryAsync(async () =>
                {
                    HttpResponseMessage memberDetails = GraphQL.AccountMemberDetails(env, tokenForParentAccountDetails).EnsureGQLResponseSuccess();

                    AccountMemberDetailsResponse accountMemberDetailsResponse = JsonConvert.DeserializeObject<AccountMemberDetailsResponse>(await memberDetails.Content.ReadAsStringAsync());

                    Assert.True(accountMemberDetailsResponse.Data.AccountMemberDetails != null, $"Parent member '{parentMemberNumber}': expected AccountMemberDetailsResponse data, but found '{(accountMemberDetailsResponse.Data.AccountMemberDetails is null ? "<null>" : "<populated>")}'.");

                    string memberType = accountMemberDetailsResponse.Data.AccountMemberDetails.FlyingClubMemberDetails.MemberType;

                    string headOfGroupMemberNumber = accountMemberDetailsResponse.Data.AccountMemberDetails.FlyingClubMemberDetails.HeadOfGroupMemberNumber ?? string.Empty;

                    Assert.True(memberType == "Household", $"Parent member '{parentMemberNumber}': expected member type 'Household' after accepting the household invite, but found '{memberType}'.");

                    Assert.True(headOfGroupMemberNumber == string.Empty, $"Parent member '{parentMemberNumber}': expected an empty head-of-group member number, but found '{headOfGroupMemberNumber}'.");

                }, 8, 2000);

                // 6) Poll / retry retrieval until the child shows up in parent's linkedMemberInfo
                await RetryHelper.RunWithRetryAsync(async () =>
                {
                    System.Net.Http.HttpResponseMessage retrieveResp = GraphQL.RetrieveHouseHoldAccount(env, tokenForParentAccountDetails, "", "Household").EnsureGQLResponseSuccess();

                    string retrieveContent = await retrieveResp.Content.ReadAsStringAsync();

                    retrieveHouseHoldLeaderResult = JObject.Parse(retrieveContent);

                    bool found = retrieveHouseHoldLeaderResult
                        .SelectTokens("$.data.retrieveHouseHoldAccount.linkedMemberInfo[*].fCNumber")
                        .Any(x => x != null && x.ToString() == childMemberNumber);

                    Assert.True(found, $"Expected child member '{childMemberNumber}' in the household-leader response, but found match '{found}'.");

                }, 8, 2000);

            }

            // Poll the final household state until it contains exactly the children created by this test.
            await RetryHelper.RunWithRetryAsync(async () =>
            {
                System.Net.Http.HttpResponseMessage retrieveResp = GraphQL.RetrieveHouseHoldAccount(env, tokenForParentAccountDetails, "", "Household").EnsureGQLResponseSuccess();

                string retrieveContent = await retrieveResp.Content.ReadAsStringAsync();

                retrieveHouseHoldLeaderResult = JObject.Parse(retrieveContent);

                string actualParentMemberNumber = retrieveHouseHoldLeaderResult.RequireToken("data.retrieveHouseHoldAccount.leaderInfo.fCNumber").ToString();

                JArray linkedMembers = Assert.IsType<JArray>(retrieveHouseHoldLeaderResult.RequireToken("data.retrieveHouseHoldAccount.linkedMemberInfo"));
                List<string> actualChildMemberNumbers = linkedMembers
                    .Select(member => member.RequireToken("fCNumber").ToString())
                    .ToList();

                Assert.Equal(parentMemberNumber, actualParentMemberNumber);
                Assert.Equal(expectedChildCount, actualChildMemberNumbers.Count);

                foreach (string expectedChildMemberNumber in expectedChildMemberNumbers)
                {
                    Assert.Contains(expectedChildMemberNumber, actualChildMemberNumbers);

                    // The data provider creates standard adult accounts, which must not be returned as Young Flyers.
                    JObject linkedAdult = Assert.IsType<JObject>(linkedMembers.Single(member =>
                        expectedChildMemberNumber.Is(member.RequireToken("fCNumber").ToString())));
                    bool isYoungFlyer = linkedAdult.RequireToken("isYoungFlyer").Value<bool>();
                    Assert.False(isYoungFlyer, $"Seeded adult member '{expectedChildMemberNumber}' should return isYoungFlyer=false, but found '{isYoungFlyer}'.");
                }
            }, 8, 2000);

            Assert.True(retrieveHouseHoldLeaderResult is not null, "Expected household retrieval to return a final result after retry, but found no result.");
            JObject finalHouseHoldLeaderResult = retrieveHouseHoldLeaderResult ?? new JObject();

            // Build the child FC numbers from the fully validated final response.
            JArray? linked = finalHouseHoldLeaderResult.SelectToken("$.data.retrieveHouseHoldAccount.linkedMemberInfo") as JArray;

            string childFcNumbers = string.Empty;

            if (linked != null && linked.Count > 0)
            {
                System.Collections.Generic.IEnumerable<string?> fcNumbers = linked
                    .Select(item => item["fCNumber"]?.ToString())
                    .Where(s => !string.IsNullOrWhiteSpace(s));


                if (fcNumbers.Any())
                {
                    childFcNumbers = string.Join(" , ", fcNumbers); // normalized separator
                }
            }

            await AddHouseHoldAccountsToStorageTable(
                env,
                parentMemberNumber,
                finalHouseHoldLeaderResult.RequireToken("data.retrieveHouseHoldAccount.leaderInfo.firstName").ToString(),
                finalHouseHoldLeaderResult.RequireToken("data.retrieveHouseHoldAccount.leaderInfo.lastName").ToString(),
                finalHouseHoldLeaderResult.RequireToken("data.retrieveHouseHoldAccount.leaderInfo.tierCode").ToString(),
                childFcNumbers
            );
        }


        private static async Task AddHouseHoldAccountsToStorageTable(string env, string parentFcNumber, string parentFirstName, string parentLastName, string parentTierCode, string childFcNumbers)
        {
            TableStorageService<LoyaltyHouseHoldandHighFiveAccountsTableEntity> tableStorageService = new();
            tableStorageService.TableName = "LoyaltyHouseHoldAccounts";
            tableStorageService.ConnectionString = connectionString;

            await tableStorageService.InsertOrMergeAsync(new LoyaltyHouseHoldandHighFiveAccountsTableEntity()
            {
                PartitionKey = env.ToUpper(),
                RowKey = parentFcNumber,
                ParentFirstName = parentFirstName,
                ParentLastName = parentLastName,
                ParentTierCode = parentTierCode,
                ChildFCNumbers = childFcNumbers,
            });
        }

        #endregion
    }
}
