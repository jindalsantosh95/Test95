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

namespace VAA.EAPI.Automation.MyAccount.Tests.GraphQlTests.Mutation
{
    [Collection("Sequential")]
    [Trait("Category", Traits.IdentityMSRelatedGraphqltests)]
    [Trait("Category", Traits.MemberManagementMSRelatedGraphqltests)]
    [Trait("Category", Traits.CustomerMSRelatedGraphqltests)]
    [Trait("Category", Traits.CustomerTransactionMSRelatedGraphqltests)]
    public sealed class AccountInviteMemberTests : IClassFixture<MemoryStoreFixture>
    {
        private const string HouseholdGroupName = "Household";
        private const string DefaultHouseholdGroupName = "Your HouseHold Account";
        private const string InviteSuccessMessage = "Successfully sent household invitation";
        private const string CancelledInviteSuccessMessage = "Successfully Cancelled HouseHold Invite";
        private const string DeclinedInviteSuccessMessage = "Successfully Declined HouseHold Invite";
        private const string AcceptedInviteSuccessMessage = "Successfully Accepted HouseHold Invite";
        private const string InviteLinkedMemberErrorCode = "10165";
        private const string InviteLinkedMemberErrorMessage = "Inviter is a linked member, can not invite members to HHA";
        private const string InviteLinkedMemberErrorSource = "Cosuming application";
        private static readonly string Password = Application.LoginPassword;
        private static readonly string EnvironmentName = Config.Environment;
        private readonly MemoryStoreFixture memoryStoreFixture;

        private static void AssertGraphQlText(JObject result, string path, string expected, string operation)
        {
            // Read optional paths safely so a changed error envelope reports the missing value alongside the intended contract.
            string actual = result.SelectToken(path)?.ToString() ?? "<missing>";
            Assert.True(actual.Is(expected), $"{operation}: expected '{expected}', but found '{actual}'.");
        }

        private static void AssertGraphQlErrorEnvelope(SoftAssertHelper softAssert, System.Net.Http.HttpResponseMessage response, JObject result, string expectedCode, string expectedMessage, string operation, bool exactMessage = true, string messagePath = "errors[0].extensions.message")
        {
            // Preserve the response envelope's safe status, code, and message so contract changes are visible without logging tokens or request data.
            string actualCode = result.SelectToken("errors[0].extensions.code")?.ToString() ?? "<missing>";
            string actualMessage = result.SelectToken(messagePath)?.ToString() ?? "<missing>";
            bool isExpectedMessage = exactMessage
                ? actualMessage.Is(expectedMessage)
                : actualMessage.Contains(expectedMessage, StringComparison.OrdinalIgnoreCase);

            softAssert.True(response.IsSuccessStatusCode, $"{operation}: expected HTTP 200 with a GraphQL error envelope, but found HTTP {(int)response.StatusCode}.");
            softAssert.True(actualCode.Is(expectedCode), $"{operation}: expected error code '{expectedCode}', but found '{actualCode}'.");
            softAssert.True(isExpectedMessage, $"{operation}: expected error message '{expectedMessage}', but found '{actualMessage}'.");
        }

        public AccountInviteMemberTests(MemoryStoreFixture memoryStoreFixture)
        {
            this.memoryStoreFixture = memoryStoreFixture;
            memoryStoreFixture.SetOnce("MYA-LegacyPassword", () => Vault.Default.GetSecretValue("MYA-LegacyPassword"));
        }

        [Theory]
        [InlineData("APP-MYA", "App")]
        [InlineData("WEB-MYA", "Web")]
        [Trait("Category", Traits.MyAccountGQLRegressionTests)]
        [Trait("Category", "HouseHoldAccountGraphQl")]
        public async Task TC01_ValidateInviteHouseHoldAccountResponseForParent(string enrollmentChannel, string mediaCode)
        {
            (string parentEmailId, string parentMemberNumber, _, _, _) = await EnrolmentHelper.GraphQLEnrolment(enrollmentChannel, mediaCode, forHousehold: true);
            (_, string childMemberNumber, _, _, _) = await EnrolmentHelper.GraphQLEnrolment(enrollmentChannel, mediaCode, forHousehold: true);
            string parentToken = await EntraTokenHelper.GetIdToken(EnvironmentName, parentEmailId, parentMemberNumber, Password);

            using System.Net.Http.HttpResponseMessage inviteResponse = GraphQL.AccountInviteMember(EnvironmentName, parentToken, childMemberNumber, HouseholdGroupName).EnsureGQLResponseSuccess();
            JObject inviteResult = JObject.Parse(await inviteResponse.Content.ReadAsStringAsync());
            AssertGraphQlText(inviteResult, "data.accountInviteMember", InviteSuccessMessage, $"Inviter '{parentMemberNumber}' invite for member '{childMemberNumber}'");

            // Remove the pending invitation created for this response-contract test.
            using System.Net.Http.HttpResponseMessage cancelInviteResponse = GraphQL.AccountInviteMemberAction(EnvironmentName, parentToken, "Cancelled", parentMemberNumber, childMemberNumber).EnsureGQLResponseSuccess();
            JObject cancelInviteResult = JObject.Parse(await cancelInviteResponse.Content.ReadAsStringAsync());
            AssertGraphQlText(cancelInviteResult, "data.accountInviteMemberAction", CancelledInviteSuccessMessage, $"Inviter '{parentMemberNumber}' cancellation for member '{childMemberNumber}'");
        }

        [Theory]
        [InlineData("APP-MYA", "App")]
        [InlineData("WEB-MYA", "Web")]
        [Trait("Category", Traits.MyAccountGQLRegressionTests)]
        [Trait("Category", "HouseHoldAccountGraphQl")]
        public async Task TC02_ValidateErrorMessageWhenInviteAlreadySentToInviteeFcNumber(string enrollmentChannel, string mediaCode)
        {
            (string parentEmailId, string parentMemberNumber, _, _, _) = await EnrolmentHelper.GraphQLEnrolment(enrollmentChannel, mediaCode, forHousehold: true);
            (_, string childMemberNumber, _, _, _) = await EnrolmentHelper.GraphQLEnrolment(enrollmentChannel, mediaCode, forHousehold: true);
            string parentToken = await EntraTokenHelper.GetIdToken(EnvironmentName, parentEmailId, parentMemberNumber, Password);

            using System.Net.Http.HttpResponseMessage inviteResponse = GraphQL.AccountInviteMember(EnvironmentName, parentToken, childMemberNumber, HouseholdGroupName).EnsureGQLResponseSuccess();
            JObject inviteResult = JObject.Parse(await inviteResponse.Content.ReadAsStringAsync());
            AssertGraphQlText(inviteResult, "data.accountInviteMember", InviteSuccessMessage, $"Inviter '{parentMemberNumber}' invite for member '{childMemberNumber}'");

            using System.Net.Http.HttpResponseMessage duplicateInviteResponse = GraphQL.AccountInviteMember(EnvironmentName, parentToken, childMemberNumber, HouseholdGroupName);
            JObject duplicateInviteResult = JObject.Parse(await duplicateInviteResponse.Content.ReadAsStringAsync());
            SoftAssertHelper softAssert = new();
            AssertGraphQlErrorEnvelope(softAssert, duplicateInviteResponse, duplicateInviteResult, "10170", "Invite has already been sent to this FC Number", $"Inviter '{parentMemberNumber}' duplicate invite for member '{childMemberNumber}'", messagePath: "errors[0].message");
            softAssert.AssertAll();

            // Remove the original pending invitation after validating the duplicate-invite contract.
            using System.Net.Http.HttpResponseMessage cancelInviteResponse = GraphQL.AccountInviteMemberAction(EnvironmentName, parentToken, "Cancelled", parentMemberNumber, childMemberNumber).EnsureGQLResponseSuccess();
            JObject cancelInviteResult = JObject.Parse(await cancelInviteResponse.Content.ReadAsStringAsync());
            AssertGraphQlText(cancelInviteResult, "data.accountInviteMemberAction", CancelledInviteSuccessMessage, $"Inviter '{parentMemberNumber}' cancellation for member '{childMemberNumber}'");
        }

        [Theory]
        [InlineData("APP-MYA", "App")]
        [InlineData("WEB-MYA", "Web")]
        [Trait("Category", Traits.MyAccountGQLRegressionTests)]
        [Trait("Category", "HouseHoldAccountGraphQl")]
        public async Task TC03_ValidateErrorMessageWhenInviteeFcNumberIsWrong(string enrollmentChannel, string mediaCode)
        {
            (string parentEmailId, string parentMemberNumber, _, _, _) = await EnrolmentHelper.GraphQLEnrolment(enrollmentChannel, mediaCode, forHousehold: true);
            string parentToken = await EntraTokenHelper.GetIdToken(EnvironmentName, parentEmailId, parentMemberNumber, Password);

            using System.Net.Http.HttpResponseMessage inviteResponse = GraphQL.AccountInviteMember(EnvironmentName, parentToken, "123456789", HouseholdGroupName);
            JObject inviteResult = JObject.Parse(await inviteResponse.Content.ReadAsStringAsync());
            SoftAssertHelper softAssert = new();

            softAssert.True(inviteResult.SelectToken("data.accountInviteMember") is null or { Type: JTokenType.Null }, $"Inviter '{parentMemberNumber}': expected no invite confirmation for an unknown invitee member number.");
            AssertGraphQlErrorEnvelope(softAssert, inviteResponse, inviteResult, "10164", "Invited FC member number is not recognised", $"Inviter '{parentMemberNumber}' unknown invitee validation");
            softAssert.AssertAll();
        }

        [Theory]
        [InlineData("APP-MYA", "App")]
        [InlineData("WEB-MYA", "Web")]
        [Trait("Category", Traits.MyAccountGQLRegressionTests)]
        [Trait("Category", "HouseHoldAccountGraphQl")]
        public async Task TC04_ValidateErrorMessageWhenInviteeFcNumbersAddressIsDifferent(string enrollmentChannel, string mediaCode)
        {
            (string parentEmailId, string parentMemberNumber, _, _, _) = await EnrolmentHelper.GraphQLEnrolment(enrollmentChannel, mediaCode, forHousehold: true);
            (_, string inviteeMemberNumber, _, _, _) = await EnrolmentHelper.GraphQLEnrolment(enrollmentChannel, mediaCode);
            string parentToken = await EntraTokenHelper.GetIdToken(EnvironmentName, parentEmailId, parentMemberNumber, Password);

            // The non-household enrolment uses a different address from the household parent.
            using System.Net.Http.HttpResponseMessage inviteResponse = GraphQL.AccountInviteMember(EnvironmentName, parentToken, inviteeMemberNumber, HouseholdGroupName);
            JObject inviteResult = JObject.Parse(await inviteResponse.Content.ReadAsStringAsync());
            SoftAssertHelper softAssert = new();

            AssertGraphQlErrorEnvelope(softAssert, inviteResponse, inviteResult, "10166", "Address mismatch", $"Inviter '{parentMemberNumber}' address-mismatch validation", exactMessage: false);
            softAssert.AssertAll();
        }

        [Theory]
        [InlineData("APP-MYA", "App")]
        [InlineData("WEB-MYA", "Web")]
        [Trait("Category", Traits.MyAccountGQLRegressionTests)]
        [Trait("Category", "HouseHoldAccountGraphQl")]
        public async Task TC05_ValidateErrorMessageWhenInviteeFcNumberIsAlreadyPartOfHousehold(string enrollmentChannel, string mediaCode)
        {
            (string parentEmailId, string parentMemberNumber, _, _, _) = await EnrolmentHelper.GraphQLEnrolment(enrollmentChannel, mediaCode, forHousehold: true);
            string parentToken = await EntraTokenHelper.GetIdToken(EnvironmentName, parentEmailId, parentMemberNumber, Password);
            Dictionary<string, string> household = RetrieveFromStorageTable.RetrieveLoyaltyHouseHoldAccountFromStorageTable();
            Assert.True(!household.ContainsKey("error"), household.GetValueOrDefault("error"));
            string childMemberNumbers = household.GetValueOrDefault("childFcNumbers") ?? string.Empty;
            string childMemberNumber = childMemberNumbers.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault() ?? string.Empty;
            Assert.True(!childMemberNumber.IsNullOrWhiteSpace(), "Expected LoyaltyHouseHoldAccounts to provide a linked child member number.");

            // A child stored in the household table is already linked, so a new leader cannot invite that member.
            using System.Net.Http.HttpResponseMessage inviteResponse = GraphQL.AccountInviteMember(EnvironmentName, parentToken, childMemberNumber, HouseholdGroupName);
            JObject inviteResult = JObject.Parse(await inviteResponse.Content.ReadAsStringAsync());
            SoftAssertHelper softAssert = new();

            AssertGraphQlErrorEnvelope(softAssert, inviteResponse, inviteResult, "10167", "Invited FC member is part of any other Household Account", $"Inviter '{parentMemberNumber}' linked-invitee validation", exactMessage: false);
            softAssert.AssertAll();
        }

        [Theory]
        [InlineData("APP-MYA", "App")]
        [InlineData("WEB-MYA", "Web")]
        [Trait("Category", Traits.MyAccountGQLRegressionTests)]
        [Trait("Category", "HouseHoldAccountGraphQl")]
        public async Task TC06_ValidateCancelHouseholdAccountInvite(string enrollmentChannel, string mediaCode)
        {
            (string parentEmailId, string parentMemberNumber, _, _, _) = await EnrolmentHelper.GraphQLEnrolment(enrollmentChannel, mediaCode, forHousehold: true);
            (_, string childMemberNumber, _, _, _) = await EnrolmentHelper.GraphQLEnrolment(enrollmentChannel, mediaCode, forHousehold: true);
            string parentToken = await EntraTokenHelper.GetIdToken(EnvironmentName, parentEmailId, parentMemberNumber, Password);

            using System.Net.Http.HttpResponseMessage inviteResponse = GraphQL.AccountInviteMember(EnvironmentName, parentToken, childMemberNumber, HouseholdGroupName).EnsureGQLResponseSuccess();
            JObject inviteResult = JObject.Parse(await inviteResponse.Content.ReadAsStringAsync());
            AssertGraphQlText(inviteResult, "data.accountInviteMember", InviteSuccessMessage, $"Inviter '{parentMemberNumber}' invite for member '{childMemberNumber}'");

            using System.Net.Http.HttpResponseMessage cancelInviteResponse = GraphQL.AccountInviteMemberAction(EnvironmentName, parentToken, "Cancelled", parentMemberNumber, childMemberNumber).EnsureGQLResponseSuccess();
            JObject cancelInviteResult = JObject.Parse(await cancelInviteResponse.Content.ReadAsStringAsync());
            AssertGraphQlText(cancelInviteResult, "data.accountInviteMemberAction", CancelledInviteSuccessMessage, $"Inviter '{parentMemberNumber}' cancellation for member '{childMemberNumber}'");

            // Cancellation must remove the inviter's pending-invite record, not only return a success message.
            await RetryHelper.RunWithRetryAsync(async () =>
            {
                using System.Net.Http.HttpResponseMessage retrieveResponse = GraphQL.RetrieveHouseHoldAccount(EnvironmentName, parentToken, string.Empty, "Individual").EnsureGQLResponseSuccess();
                JObject retrieveResult = JObject.Parse(await retrieveResponse.Content.ReadAsStringAsync());
                Assert.True(retrieveResult.SelectToken("data.retrieveHouseHoldAccount.inviteeInfo") is null or { Type: JTokenType.Null }, $"Inviter '{parentMemberNumber}': expected no pending invitee after cancelling the invite for member '{childMemberNumber}'.");
            }, maxRetries: 8, delay: 2000);
        }

        [Theory]
        [InlineData("APP-MYA", "App")]
        [InlineData("WEB-MYA", "Web")]
        [Trait("Category", Traits.MyAccountGQLRegressionTests)]
        [Trait("Category", "HouseHoldAccountGraphQl")]
        public async Task TC07_ValidateDeclineHouseholdAccountInvite(string enrollmentChannel, string mediaCode)
        {
            (string parentEmailId, string parentMemberNumber, _, _, _) = await EnrolmentHelper.GraphQLEnrolment(enrollmentChannel, mediaCode, forHousehold: true);
            (string childEmailId, string childMemberNumber, _, _, _) = await EnrolmentHelper.GraphQLEnrolment(enrollmentChannel, mediaCode, forHousehold: true);
            string parentToken = await EntraTokenHelper.GetIdToken(EnvironmentName, parentEmailId, parentMemberNumber, Password);
            string childToken = await EntraTokenHelper.GetIdToken(EnvironmentName, childEmailId, childMemberNumber, Password);

            using System.Net.Http.HttpResponseMessage inviteResponse = GraphQL.AccountInviteMember(EnvironmentName, parentToken, childMemberNumber, HouseholdGroupName).EnsureGQLResponseSuccess();
            JObject inviteResult = JObject.Parse(await inviteResponse.Content.ReadAsStringAsync());
            AssertGraphQlText(inviteResult, "data.accountInviteMember", InviteSuccessMessage, $"Inviter '{parentMemberNumber}' invite for member '{childMemberNumber}'");

            using System.Net.Http.HttpResponseMessage declineInviteResponse = GraphQL.AccountInviteMemberAction(EnvironmentName, childToken, "Declined", parentMemberNumber, childMemberNumber).EnsureGQLResponseSuccess();
            JObject declineInviteResult = JObject.Parse(await declineInviteResponse.Content.ReadAsStringAsync());
            AssertGraphQlText(declineInviteResult, "data.accountInviteMemberAction", DeclinedInviteSuccessMessage, $"Invitee '{childMemberNumber}' decline for inviter '{parentMemberNumber}'");

            // Declining must remove the invitee's pending-inviter record, not only return a success message.
            await RetryHelper.RunWithRetryAsync(async () =>
            {
                using System.Net.Http.HttpResponseMessage retrieveResponse = GraphQL.RetrieveHouseHoldAccount(EnvironmentName, childToken, string.Empty, "Individual").EnsureGQLResponseSuccess();
                JObject retrieveResult = JObject.Parse(await retrieveResponse.Content.ReadAsStringAsync());
                Assert.True(retrieveResult.SelectToken("data.retrieveHouseHoldAccount.inviterInfo") is null or { Type: JTokenType.Null }, $"Invitee '{childMemberNumber}': expected no pending inviter after declining the invite from member '{parentMemberNumber}'.");
            }, maxRetries: 8, delay: 2000);
        }

        [Theory]
        [InlineData("APP-MYA", "App")]
        [InlineData("WEB-MYA", "Web")]
        [Trait("Category", Traits.MyAccountGQLRegressionTests)]
        [Trait("Category", "HouseHoldAccountGraphQl")]
        public async Task TC08_ValidateAcceptHouseholdAccountInviteAndRetrieve(string enrollmentChannel, string mediaCode)
        {
            (string parentEmailId, string parentMemberNumber, _, _, _) = await EnrolmentHelper.GraphQLEnrolment(enrollmentChannel, mediaCode, forHousehold: true);
            (string childEmailId, string childMemberNumber, _, _, _) = await EnrolmentHelper.GraphQLEnrolment(enrollmentChannel, mediaCode, forHousehold: true);
            string parentToken = await EntraTokenHelper.GetIdToken(EnvironmentName, parentEmailId, parentMemberNumber, Password);
            string childToken = await EntraTokenHelper.GetIdToken(EnvironmentName, childEmailId, childMemberNumber, Password);

            using System.Net.Http.HttpResponseMessage inviteResponse = GraphQL.AccountInviteMember(EnvironmentName, parentToken, childMemberNumber, HouseholdGroupName).EnsureGQLResponseSuccess();
            JObject inviteResult = JObject.Parse(await inviteResponse.Content.ReadAsStringAsync());
            AssertGraphQlText(inviteResult, "data.accountInviteMember", InviteSuccessMessage, $"Inviter '{parentMemberNumber}' invite for member '{childMemberNumber}'");

            using System.Net.Http.HttpResponseMessage acceptInviteResponse = GraphQL.AccountInviteMemberAction(EnvironmentName, childToken, "Accepted", parentMemberNumber, childMemberNumber).EnsureGQLResponseSuccess();
            JObject acceptInviteResult = JObject.Parse(await acceptInviteResponse.Content.ReadAsStringAsync());
            AssertGraphQlText(acceptInviteResult, "data.accountInviteMemberAction", AcceptedInviteSuccessMessage, $"Invitee '{childMemberNumber}' acceptance for inviter '{parentMemberNumber}'");

            await RetryHelper.RunWithRetryAsync(async () =>
            {
                using System.Net.Http.HttpResponseMessage retrieveResponse = GraphQL.RetrieveHouseHoldAccount(EnvironmentName, parentToken, string.Empty, HouseholdGroupName).EnsureGQLResponseSuccess();
                JObject retrieveResult = JObject.Parse(await retrieveResponse.Content.ReadAsStringAsync());

                string leaderMemberNumber = retrieveResult.RequireToken("data.retrieveHouseHoldAccount.leaderInfo.fCNumber").ToString();
                Assert.True(leaderMemberNumber.Is(parentMemberNumber), $"Inviter '{parentMemberNumber}': expected the accepted household leader to match the inviter.");

                // Match the invited adult by FC number before checking its Young Flyer classification.
                JArray? linkedMembers = retrieveResult.SelectToken("data.retrieveHouseHoldAccount.linkedMemberInfo") as JArray;
                JObject? linkedAdult = linkedMembers?.FirstOrDefault(member => childMemberNumber.Is(member.RequireToken("fCNumber").ToString())) as JObject;
                bool? isYoungFlyer = linkedAdult?.RequireToken("isYoungFlyer").Value<bool>();
                Assert.True(linkedAdult is not null && isYoungFlyer == false, $"Invited adult member '{childMemberNumber}' should be linked with isYoungFlyer=false.");
            }, maxRetries: 8, delay: 2000);
        }

        [Theory]
        [InlineData("APP-MYA", "App")]
        [InlineData("WEB-MYA", "Web")]
        [Trait("Category", Traits.MyAccountGQLRegressionTests)]
        [Trait("Category", "HouseHoldAccountGraphQl")]
        public async Task TC09_RejectsInviteWhenInviterIsAlreadyLinked(string enrollmentChannel, string mediaCode)
        {
            Dictionary<string, string> household = RetrieveFromStorageTable.RetrieveLoyaltyHouseHoldAccountFromStorageTable();
            Assert.True(!household.ContainsKey("error"), household.GetValueOrDefault("error"));
            string childMemberNumbers = household.GetValueOrDefault("childFcNumbers") ?? string.Empty;
            string linkedInviterMemberNumber = childMemberNumbers.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault() ?? string.Empty;
            Assert.True(!linkedInviterMemberNumber.IsNullOrWhiteSpace(), "Expected LoyaltyHouseHoldAccounts to provide a linked child member number.");
            string legacyPassword = memoryStoreFixture.Get<string>("MYA-LegacyPassword");
            string linkedInviterToken = await EntraTokenHelper.GetIdTokenWithPasswordFallback(EnvironmentName, string.Empty, linkedInviterMemberNumber, Password, legacyPassword);
            (_, string inviteeMemberNumber, _, _, _) = await EnrolmentHelper.GraphQLEnrolment(enrollmentChannel, mediaCode, forHousehold: true);

            // A stored linked child has a HeadOfGroupMemberNumber, so the resolver must reject another household invite.
            using System.Net.Http.HttpResponseMessage inviteResponse = GraphQL.AccountInviteMember(EnvironmentName, linkedInviterToken, inviteeMemberNumber, HouseholdGroupName);
            JObject inviteResult = JObject.Parse(await inviteResponse.Content.ReadAsStringAsync());
            SoftAssertHelper softAssert = new();

            softAssert.True(inviteResult.SelectToken("data.accountInviteMember") is null or { Type: JTokenType.Null }, $"Inviter '{linkedInviterMemberNumber}': expected no invite confirmation when the inviter is already linked.");
            AssertGraphQlErrorEnvelope(softAssert, inviteResponse, inviteResult, InviteLinkedMemberErrorCode, InviteLinkedMemberErrorMessage, $"Inviter '{linkedInviterMemberNumber}' linked-inviter validation", messagePath: "errors[0].message");
            softAssert.True(inviteResult.RequireToken("errors[0].extensions.message").ToString().Is(InviteLinkedMemberErrorMessage), $"Inviter '{linkedInviterMemberNumber}': expected linked-inviter extension message '{InviteLinkedMemberErrorMessage}'.");
            softAssert.True(inviteResult.RequireToken("errors[0].extensions.source").ToString().Is(InviteLinkedMemberErrorSource), $"Inviter '{linkedInviterMemberNumber}': expected linked-inviter error source '{InviteLinkedMemberErrorSource}'.");
            softAssert.AssertAll();
        }

        [Theory]
        [InlineData("APP-MYA", "App")]
        [InlineData("WEB-MYA", "Web")]
        [Trait("Category", Traits.MyAccountGQLRegressionTests)]
        [Trait("Category", "HouseHoldAccountGraphQl")]
        public async Task TC10_UsesDefaultGroupNameWhenGroupNameIsMissing(string enrollmentChannel, string mediaCode)
        {
            (string parentEmailId, string parentMemberNumber, _, _, _) = await EnrolmentHelper.GraphQLEnrolment(enrollmentChannel, mediaCode, forHousehold: true);
            (_, string childMemberNumber, _, _, _) = await EnrolmentHelper.GraphQLEnrolment(enrollmentChannel, mediaCode, forHousehold: true);
            string parentToken = await EntraTokenHelper.GetIdToken(EnvironmentName, parentEmailId, parentMemberNumber, Password);

            // An empty group name must be stored and returned as the resolver's documented default household name.
            using System.Net.Http.HttpResponseMessage inviteResponse = GraphQL.AccountInviteMember(EnvironmentName, parentToken, childMemberNumber, string.Empty).EnsureGQLResponseSuccess();
            JObject inviteResult = JObject.Parse(await inviteResponse.Content.ReadAsStringAsync());
            AssertGraphQlText(inviteResult, "data.accountInviteMember", InviteSuccessMessage, $"Inviter '{parentMemberNumber}' invite for member '{childMemberNumber}'");

            await RetryHelper.RunWithRetryAsync(async () =>
            {
                using System.Net.Http.HttpResponseMessage retrieveResponse = GraphQL.RetrieveHouseHoldAccount(EnvironmentName, parentToken, string.Empty, "Individual").EnsureGQLResponseSuccess();
                JObject retrieveResult = JObject.Parse(await retrieveResponse.Content.ReadAsStringAsync());
                string groupName = retrieveResult.RequireToken("data.retrieveHouseHoldAccount.name").ToString();
                Assert.True(groupName.Is(DefaultHouseholdGroupName), $"Inviter '{parentMemberNumber}': expected default household name '{DefaultHouseholdGroupName}', found '{groupName}'.");
            }, maxRetries: 8, delay: 2000);

            // Remove the pending invitation created to validate the default-name branch.
            using System.Net.Http.HttpResponseMessage cancelInviteResponse = GraphQL.AccountInviteMemberAction(EnvironmentName, parentToken, "Cancelled", parentMemberNumber, childMemberNumber).EnsureGQLResponseSuccess();
            JObject cancelInviteResult = JObject.Parse(await cancelInviteResponse.Content.ReadAsStringAsync());
            AssertGraphQlText(cancelInviteResult, "data.accountInviteMemberAction", CancelledInviteSuccessMessage, $"Inviter '{parentMemberNumber}' cancellation for member '{childMemberNumber}'");
        }

        [Theory]
        [InlineData("APP-MYA", "App")]
        [InlineData("WEB-MYA", "Web")]
        [Trait("Category", Traits.MyAccountGQLRegressionTests)]
        [Trait("Category", "HouseHoldAccountGraphQl")]
        public async Task TC11_RejectsInviteWhenHouseholdLeaderIsInactive(string enrollmentChannel, string mediaCode)
        {
            (string parentEmailId, string parentMemberNumber, string parentCustomerId, string parentFirstName, string parentLastName) = await EnrolmentHelper.GraphQLEnrolment(enrollmentChannel, mediaCode, forHousehold: true);
            (_, string inviteeMemberNumber, _, _, _) = await EnrolmentHelper.GraphQLEnrolment(enrollmentChannel, mediaCode, forHousehold: true);
            // Obtain the token while the fresh leader is active, then use Loops to exercise the non-active-leader validation branch.
            string parentToken = await EntraTokenHelper.GetIdToken(EnvironmentName, parentEmailId, parentMemberNumber, Password);
            using System.Net.Http.HttpResponseMessage statusUpdateResponse = Endpoints.UpdateFCMemberStatus(parentMemberNumber, parentCustomerId, parentFirstName, parentLastName, "01/12/1990", Application.LoyaltyMemberStatus.Inactive.ToString(), "Mr").EnsureIsSuccessStatusCode();
            JObject statusUpdateResult = JObject.Parse(await statusUpdateResponse.Content.ReadAsStringAsync());
            Assert.True(statusUpdateResult.RequireToken("data.cardNumber").ToString().Is(parentMemberNumber), $"Inviter '{parentMemberNumber}': expected Loops to update the fresh household leader to Inactive.");

            await RetryHelper.RunWithRetryAsync(async () =>
            {
                using System.Net.Http.HttpResponseMessage inviteResponse = GraphQL.AccountInviteMember(EnvironmentName, parentToken, inviteeMemberNumber, HouseholdGroupName);
                JObject inviteResult = JObject.Parse(await inviteResponse.Content.ReadAsStringAsync());
                SoftAssertHelper softAssert = new();

                softAssert.True(inviteResult.SelectToken("data.accountInviteMember") is null or { Type: JTokenType.Null }, $"Inviter '{parentMemberNumber}': expected no invite confirmation for an inactive household leader.");
                AssertGraphQlErrorEnvelope(softAssert, inviteResponse, inviteResult, "G022", "Head of the account is not active", $"Inviter '{parentMemberNumber}' inactive-leader validation");
                softAssert.AssertAll();
            }, maxRetries: 5, delay: 2000);
        }
    }
}
