using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.MyAccount.Constants;
using VAA.EAPI.Automation.MyAccount.Helpers;
using VAA.EAPI.Automation.MyAccount.Requests.Endpoint;

namespace VAA.EAPI.Automation.MyAccount.Tests.GraphQlTests.Query;

[Collection("Sequential")]
[Trait("Category", Traits.IdentityMSRelatedGraphqltests)]
[Trait("Category", Traits.MemberManagementMSRelatedGraphqltests)]
[Trait("Category", Traits.CustomerMSRelatedGraphqltests)]
[Trait("Category", Traits.CustomerTransactionMSRelatedGraphqltests)]
public sealed class AccountManagedYoungFlyerAccountsTests
{
    private const string EnrollmentChannel = "WEB-MYA";
    private const string MediaCode = "Web";
    private static readonly string env = Config.Environment;

    [Fact]
    [Trait("Category", Traits.MyAccountGQLRegressionTests)]
    public async Task TC01_AccountManagedYoungFlyerAccounts_ReturnsCreatedChild()
    {
        // Create an isolated parent because managed Young Flyer relationships are created by the parent-authorised mutation.
        (string parentEmailId, string parentMemberNumber, string parentCustomerId, _, _) = await EnrolmentHelper.GraphQLEnrolment(EnrollmentChannel, MediaCode, forHousehold: true);
        string parentToken = await EntraTokenHelper.GetIdToken(env, parentEmailId, parentMemberNumber, Application.LoginPassword);
        string childFirstName = DataGenerator.RandomFirstName();
        string childLastName = $"Automation{Random.Shared.Next(100000, 999999)}";

        using HttpResponseMessage createChildResponse = GraphQL.AccountCreateYoungFlyerAccount(
            env,
            parentToken,
            childFirstName,
            childLastName,
            DateTime.UtcNow.AddYears(-10).ToString("yyyy-MM-dd"),
            "M",
            EnrollmentChannel).EnsureGQLResponseSuccess();
        JObject createChildResult = JObject.Parse(await createChildResponse.Content.ReadAsStringAsync());
        string childMemberNumber = createChildResult.RequireToken("data.accountCreateYoungFlyerAccount.memberDetails.memberNumber").ToString();

        Assert.False(childMemberNumber.IsNullOrWhiteSpace(), "Expected the created Young Flyer to have a member number.");

        // The relationship is persisted asynchronously, so retry only until the created child is available to the query.
        await RetryHelper.RunWithRetryAsync(async () =>
        {
            using HttpResponseMessage response = GraphQL.AccountManagedYoungFlyerAccounts(env, parentToken).EnsureGQLResponseSuccess();
            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
            JObject managedAccounts = Assert.IsType<JObject>(result.RequireToken("data.accountManagedYoungFlyerAccounts"));
            JArray youngFlyers = Assert.IsType<JArray>(managedAccounts.RequireToken("youngFlyers"));
            JObject createdYoungFlyer = Assert.IsType<JObject>(youngFlyers.Single(youngFlyer =>
                childMemberNumber.Is(youngFlyer.RequireToken("fcNumber").ToString())));

            SoftAssertHelper softAssert = new();
            softAssert.True(managedAccounts.RequireToken("parentCustomerId").ToString().Is(parentCustomerId),
                $"Expected parent customer '{parentCustomerId}' to be returned.");
            softAssert.True(managedAccounts.RequireToken("parentFcNumber").ToString().Is(parentMemberNumber),
                $"Expected parent member '{parentMemberNumber}' to be returned.");
            softAssert.True(createdYoungFlyer.RequireToken("hoHManaged").Value<bool>(),
                $"Expected created Young Flyer '{childMemberNumber}' to be marked as parent-managed.");
            softAssert.True(createdYoungFlyer.RequireToken("firstName").ToString().Is(childFirstName),
                $"Expected Young Flyer '{childMemberNumber}' to have first name '{childFirstName}'.");
            softAssert.True(createdYoungFlyer.RequireToken("lastName").ToString().Is(childLastName),
                $"Expected Young Flyer '{childMemberNumber}' to have last name '{childLastName}'.");
            softAssert.False(createdYoungFlyer.RequireToken("customerId").ToString().IsNullOrWhiteSpace(),
                $"Expected Young Flyer '{childMemberNumber}' to have a customer ID.");
            softAssert.AssertAll();
        }, maxRetries: 8, delay: 2000);
    }

}
