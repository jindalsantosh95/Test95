using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.MyAccount.Constants;
using VAA.EAPI.Automation.MyAccount.Helpers;
using VAA.EAPI.Automation.MyAccount.Requests.Endpoint;

namespace VAA.EAPI.Automation.MyAccount.Tests.GraphQlTests.Mutation;

[Trait("Category", Traits.CustomerMSRelatedGraphqltests)]
public sealed class AccountPasswordResetStartTests
{
    private const string EnrollmentChannel = "WEB-MYA";
    private const string MediaCode = "Web";
    private const string InvalidUsernameMessage = "Error during account member password reset, invalid username";
    private static readonly string env = Config.Environment;

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [Trait("Category", Traits.MyAccountGQLRegressionTests)]
    [Trait("Category", Traits.GqlEntraAuthentication)]
    [Trait("Category", Traits.IdentityMSRelatedGraphqltests)]
    public async Task TC01_ReturnsContinuationTokenForMemberEmailOrNumber(bool useMemberNumber)
    {
        (string emailId, string memberNumber, _, _, _, _, _, _, _) = await EnrolmentHelper.EntraEnrolment(EnrollmentChannel, MediaCode);
        string resetIdentifier = useMemberNumber ? memberNumber : emailId;

        // The resolver accepts either member number or email and always starts reset against the preferred email address.
        using HttpResponseMessage resetStartResponse = GraphQL.AccountPasswordResetStart(env, resetIdentifier).EnsureGQLResponseSuccess();
        JObject resetStartResult = JObject.Parse(await resetStartResponse.Content.ReadAsStringAsync());
        string continuationToken = resetStartResult.RequireToken("data.accountPasswordResetStart.continuationToken").ToString();

        Assert.True(!continuationToken.IsNullOrWhiteSpace(), $"Member '{memberNumber}': expected a non-empty password-reset continuation token when using {(useMemberNumber ? "member number" : "email")}.");
    }

    [Fact]
    [Trait("Category", Traits.MyAccountGQLRegressionTests)]
    [Trait("Category", Traits.GqlEntraAuthentication)]
    public async Task TC02_RejectsUnregisteredEmailWithoutStartingPasswordReset()
    {
        string unregisteredEmail = $"passwordreset{Random.Shared.NextInt64(100000000000, 999999999999)}@example.com";

        // An unregistered address must not return a continuation token or invoke IdentityMS reset.
        using HttpResponseMessage resetStartResponse = GraphQL.AccountPasswordResetStart(env, unregisteredEmail);
        JObject resetStartResult = JObject.Parse(await resetStartResponse.Content.ReadAsStringAsync());
        SoftAssertHelper softAssert = new();

        softAssert.True(resetStartResponse.IsSuccessStatusCode, $"Unregistered email: expected GraphQL HTTP 200 with an error envelope, found HTTP {(int)resetStartResponse.StatusCode}.");
        softAssert.True(resetStartResult.SelectToken("data.accountPasswordResetStart") is null or { Type: JTokenType.Null }, "Unregistered email: expected no password-reset continuation token.");
        softAssert.True(resetStartResult.RequireToken("errors[0].message").ToString().Is(InvalidUsernameMessage), "Unregistered email: expected the invalid-username error message.");
        softAssert.True(resetStartResult.RequireToken("errors[0].extensions.code").ToString().Is("Unauthorized"), "Unregistered email: expected GraphQL error code 'Unauthorized'.");
        softAssert.True(resetStartResult.RequireToken("errors[0].extensions.Exception").ToString().Is("Invalid username"), "Unregistered email: expected the invalid-username extension detail.");
        softAssert.True(resetStartResult.RequireToken("errors[0].extensions.httpStatusCode").Value<int>() == 401, "Unregistered email: expected HTTP-status extension '401'.");
        softAssert.AssertAll();
    }
}
