using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.MyAccount.Constants;
using VAA.EAPI.Automation.MyAccount.Fixtures;
using VAA.EAPI.Automation.MyAccount.Models.GraphQL;
using VAA.EAPI.Automation.MyAccount.Requests.Endpoint;

namespace VAA.EAPI.Automation.MyAccount.Tests.GraphQlTests.Query
{
    [Trait("Category", Traits.IdentityMSRelatedGraphqltests)]
    [Trait("Category", Traits.CustomerMSRelatedGraphqltests)]
    [Trait("Category", Traits.CustomerTransactionMSRelatedGraphqltests)]
    [Trait("Category", Traits.VRedMSRelatedGraphqltests)]
    public class AccountActivityDetailsTests : IClassFixture<LoyaltyMemberAccountFixture>
    {
        private readonly string env = Config.Environment;
        private readonly LoyaltyMemberAccountFixture _loyaltyMemberAccountFixture;
        private readonly FlyingClubMemberHelper flyingClubMemberHelper;

        public AccountActivityDetailsTests(LoyaltyMemberAccountFixture loyaltyMemberAccountFixture)
        {
            _loyaltyMemberAccountFixture = loyaltyMemberAccountFixture;
            flyingClubMemberHelper = new FlyingClubMemberHelper();
        }

        [Fact]
        [Trait("Category", Traits.MyAccountGQLRegressionTests)]
        public async Task TC01_AccountActivityDetails_ReturnsData()
        {
            int points = Random.Shared.Next(100, 10000);
            int pageSize = 10;

            bool isTransactionCreated = await flyingClubMemberHelper.AddLoyaltyPointsToTheFlyingClubAccount(_loyaltyMemberAccountFixture.MemberNumber, points);

            Assert.True(isTransactionCreated, $"Failed to Create a Loyalty Points Credit Transaction for the Member {_loyaltyMemberAccountFixture.MemberNumber}");

            ActivityDetail? createdActivity = null;

            // The loyalty activity projection is asynchronous, so wait only until this test's credit transaction is queryable.
            await RetryHelper.RunWithRetryAsync(async () =>
            {
                using HttpResponseMessage response = GraphQL.AccountActivityDetails(env, _loyaltyMemberAccountFixture.Token, page: 1, pageSize: pageSize).EnsureGQLResponseSuccess();
                string body = await response.Content.ReadAsStringAsync();
                AccountActivityDetailsResponse? parsedActivities = JsonConvert.DeserializeObject<AccountActivityDetailsResponse>(body);
                Assert.True(parsedActivities is not null, $"Expected a deserializable {nameof(AccountActivityDetailsResponse)} response for member '{_loyaltyMemberAccountFixture.MemberNumber}', but found an invalid response.");
                AccountActivityDetailsResponse activities = parsedActivities ?? new AccountActivityDetailsResponse();

                Assert.True(activities.Data is not null, $"Expected data in the accountActivityDetails response for member '{_loyaltyMemberAccountFixture.MemberNumber}', but found null.");
                AccountActivityDetailsResponseData data = activities.Data ?? new AccountActivityDetailsResponseData();
                Assert.True(data.AccountActivityDetails is not null, $"Expected accountActivityDetails for member '{_loyaltyMemberAccountFixture.MemberNumber}', but found null.");
                ActivityDetail[] activityDetails = data.AccountActivityDetails ?? Array.Empty<ActivityDetail>();

                Assert.NotEmpty(activityDetails);
                Assert.True(activityDetails.Length <= pageSize, $"Expected no more than {pageSize} activity records for member '{_loyaltyMemberAccountFixture.MemberNumber}', but received {activityDetails.Length}.");

                createdActivity = activityDetails.FirstOrDefault(activity => activity.VirginPoints?.Is($"+{points}") == true);
                Assert.True(createdActivity is not null, $"No Credit Transaction found for the Member {_loyaltyMemberAccountFixture.MemberNumber} with Points {points}. \n Response Returned is \n {body}");
            }, maxRetries: 5, delay: 2000);

            ActivityDetail activity = createdActivity ?? new ActivityDetail();

            Assert.True(activity.VirginPoints?.StartsWith("+") == true, $"Expected credit transaction points to be prefixed with '+', but found '{activity.VirginPoints}'.");
            Assert.True(activity.TransactionType?.Is("Accrual") == true, $"Expected created credit transaction to be an Accrual, but found '{activity.TransactionType}'.");
            Assert.False(string.IsNullOrWhiteSpace(activity.TransactionId), "Expected the created activity to include a transaction ID.");
            Assert.False(string.IsNullOrWhiteSpace(activity.PartnerName), "Expected the created activity to include a partner name.");
            Assert.False(string.IsNullOrWhiteSpace(activity.PartnerCode), "Expected the created activity to include a partner code.");
            Assert.False(string.IsNullOrWhiteSpace(activity.Statement), "Expected the created activity to include a statement.");
            Assert.True(activity.ActivityDate.HasValue, "Expected the created activity to include an activity date.");
            Assert.True(activity.TransactionProcessedDate.HasValue, "Expected the created activity to include a transaction processed date.");
        }
    }
}

