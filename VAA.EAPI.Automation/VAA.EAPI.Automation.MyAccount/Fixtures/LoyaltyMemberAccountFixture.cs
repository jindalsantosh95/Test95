using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.MyAccount.Helpers;
using VAA.EAPI.Automation.MyAccount.Requests.Endpoint;
using Application = VAA.EAPI.Automation.MyAccount.Constants.Application;

namespace VAA.EAPI.Automation.MyAccount.Fixtures
{
    public sealed class LoyaltyMemberAccountFixture : IAsyncLifetime
    {
        public string Token { get; private set; } = string.Empty;
        public string MemberNumber { get; private set; } = string.Empty;
        public string CustomerId { get; private set; } = string.Empty;

        public async Task InitializeAsync()
        {
            string env = Config.Environment;

            (string _, string memberNumber) = await EnrolmentHelper.GraphQLEnrolment();

            string token = await EntraTokenHelper.GetIdToken(env, "", memberNumber, Application.LoginPassword);

            HttpResponseMessage memberDetailsResponse = GraphQL.AccountMemberDetails(env, token).EnsureGQLResponseSuccess();

            string content = await memberDetailsResponse.Content.ReadAsStringAsync();

            JObject memberDetailsResult = JObject.Parse(content);

            string returnedMembershipNumber = memberDetailsResult
                .RequireToken("data.accountMemberDetails.flyingClubMemberDetails.fCMembershipNumber")
                .ToString();

            string returnedCustomerId = memberDetailsResult
                .RequireToken("data.accountMemberDetails.flyingClubMemberDetails.customerID")
                .ToString();

            Assert.True(returnedMembershipNumber.Is(memberNumber), $"Expected membership number {memberNumber} but found {returnedMembershipNumber}. Response: {memberDetailsResult}");
            Assert.True(!returnedCustomerId.IsNullOrWhiteSpace(), $"Expected a non-empty customerID for member {memberNumber}. Response: {memberDetailsResult}");

            Token = token;
            MemberNumber = memberNumber;
            CustomerId = returnedCustomerId;
        }

        public Task DisposeAsync() => Task.CompletedTask;
    }
}
