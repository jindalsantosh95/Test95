using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.MyAccount.Constants;
using VAA.EAPI.Automation.MyAccount.Helpers;
using VAA.EAPI.Automation.MyAccount.Models.Enrolment;
using VAA.EAPI.Automation.MyAccount.Requests.Endpoint;
using VAA.EAPI.Infrastructure.AzureStorage;

namespace VAA.EAPI.Automation.MyAccount.Tests.GraphQlTests.Mutation;

[Trait("Category", Traits.IdentityMSRelatedGraphqltests)]
[Trait("Category", Traits.CustomerMSRelatedGraphqltests)]
public sealed class AccountEntraPasswordSubmissionTests
{
    private const string EnrollmentChannel = "WEB-MYA";
    private const string MediaCode = "Web";
    private const string PasswordPolicyErrorCode = "10138";
    private const string PasswordPolicyErrorMessage = "Password provided not as per policy";
    private const string PasswordPolicyErrorSource = "Consuming application";
    private static readonly string env = Config.Environment;

    [Fact]
    [Trait("Category", Traits.MyAccountGQLRegressionTests)]
    [Trait("Category", Traits.GqlEntraAuthentication)]
    public async Task TC01_RejectsInvalidPasswordThenResetsAndSignsInFreshMember()
    {
        (string emailId, string memberNumber, _, _, _, _, _, _, _) = await EnrolmentHelper.EntraEnrolment(EnrollmentChannel, MediaCode);

        JObject? resetStartResult = null;

        // Retry until asynchronous enrolment propagation makes a password-reset continuation token available.
        await RetryHelper.RunWithRetryAsync(async () =>
        {
            using HttpResponseMessage resetStartResponse = GraphQL.AccountPasswordResetStart(env, emailId).EnsureGQLResponseSuccess();
            resetStartResult = JObject.Parse(await resetStartResponse.Content.ReadAsStringAsync());
            _ = resetStartResult.RequireToken("data.accountPasswordResetStart.continuationToken");
        }, maxRetries: 4, delay: 1000);

        JObject completedResetStartResult = Assert.IsType<JObject>(resetStartResult);
        string resetToken = completedResetStartResult.RequireToken("data.accountPasswordResetStart.continuationToken").ToString();
        TableStorageService<OtpRequestTableEntity> otpRequestTableStorageService = new() { ConnectionString = Vault.Default.GetSecretValue("IdentityStorageAccountConnectionString-sit"), TableName = "OTPRequest" };
        int resetOtp = await EnrolmentHelper.GetOTPByEmailId(otpRequestTableStorageService, emailId, 8);
        using HttpResponseMessage otpValidationResponse = GraphQL.AccountEntraOtpValidation(env, resetToken, resetOtp.ToString(), Application.ValidationRequestType.ResetPassword).EnsureGQLResponseSuccess();
        JObject otpValidationResult = JObject.Parse(await otpValidationResponse.Content.ReadAsStringAsync());
        string continuationToken = otpValidationResult.RequireToken("data.accountEntraOtpValidation.continuationToken").ToString();

        // This deliberately weak value exercises only the public password-policy contract and is never included in diagnostics.
        using HttpResponseMessage invalidPasswordResponse = GraphQL.AccountEntraPasswordSubmission(env, continuationToken, "VirginAtlantic");
        JObject invalidPasswordResult = JObject.Parse(await invalidPasswordResponse.Content.ReadAsStringAsync());
        SoftAssertHelper invalidPasswordAssert = new();

        invalidPasswordAssert.True(invalidPasswordResponse.IsSuccessStatusCode, $"Member '{memberNumber}': expected GraphQL HTTP 200 with a password-policy error envelope, found HTTP {(int)invalidPasswordResponse.StatusCode}.");
        invalidPasswordAssert.True(invalidPasswordResult.SelectToken("data.accountEntraPasswordSubmission") is null or { Type: JTokenType.Null }, $"Member '{memberNumber}': expected no token response for a password-policy failure.");
        invalidPasswordAssert.True(invalidPasswordResult.RequireToken("errors[0].message").ToString().Is("Password Validation Failed with Policies."), $"Member '{memberNumber}': expected the password-policy failure message.");
        invalidPasswordAssert.True(invalidPasswordResult.RequireToken("errors[0].extensions.code").ToString().Is(PasswordPolicyErrorCode), $"Member '{memberNumber}': expected password-policy code '{PasswordPolicyErrorCode}'.");
        invalidPasswordAssert.True(invalidPasswordResult.RequireToken("errors[0].extensions.message").ToString().Is(PasswordPolicyErrorMessage), $"Member '{memberNumber}': expected the password-policy detail message.");
        invalidPasswordAssert.True(invalidPasswordResult.RequireToken("errors[0].extensions.source").ToString().Is(PasswordPolicyErrorSource), $"Member '{memberNumber}': expected password-policy source '{PasswordPolicyErrorSource}'.");
        invalidPasswordAssert.AssertAll();

        string newPassword = $"Vaa!{Random.Shared.NextInt64(100000000, 999999999)}aA";
        // Avoid shared diagnostics because the request contains the newly created password.
        using HttpResponseMessage passwordSubmissionResponse = GraphQL.AccountEntraPasswordSubmission(env, continuationToken, newPassword);
        string passwordSubmissionBody = await passwordSubmissionResponse.Content.ReadAsStringAsync();
        JObject passwordSubmissionResult = JObject.Parse(passwordSubmissionBody);
        bool passwordSubmissionHasErrors = passwordSubmissionResult["errors"] is JArray { Count: > 0 };

        Assert.True(passwordSubmissionResponse.IsSuccessStatusCode, $"Member '{memberNumber}': accountEntraPasswordSubmission returned HTTP {(int)passwordSubmissionResponse.StatusCode}.");
        Assert.True(!passwordSubmissionHasErrors, $"Member '{memberNumber}': accountEntraPasswordSubmission returned a GraphQL error.");

        JToken passwordSubmission = passwordSubmissionResult.RequireToken("data.accountEntraPasswordSubmission");
        SoftAssertHelper passwordSubmissionAssert = new();

        passwordSubmissionAssert.True(passwordSubmission.RequireToken("tokenType").ToString().Is("Bearer"), $"Member '{memberNumber}': expected password submission token type 'Bearer'.");
        passwordSubmissionAssert.True(passwordSubmission.RequireToken("expiresIn").Value<int>() > 0, $"Member '{memberNumber}': expected password submission token expiry greater than zero.");
        passwordSubmissionAssert.True(!passwordSubmission.RequireToken("accessToken").ToString().IsNullOrWhiteSpace(), $"Member '{memberNumber}': expected password submission access token.");
        passwordSubmissionAssert.True(!passwordSubmission.RequireToken("refreshToken").ToString().IsNullOrWhiteSpace(), $"Member '{memberNumber}': expected password submission refresh token.");
        passwordSubmissionAssert.True(!passwordSubmission.RequireToken("idToken").ToString().IsNullOrWhiteSpace(), $"Member '{memberNumber}': expected password submission ID token.");
        passwordSubmissionAssert.AssertAll();

        // Sign in directly so a failed credential request cannot include the new password in shared diagnostics.
        using HttpResponseMessage signInResponse = GraphQL.AccountMemberSignIn(env, emailId, memberNumber, newPassword);
        string signInBody = await signInResponse.Content.ReadAsStringAsync();
        JObject signInResult = JObject.Parse(signInBody);
        bool signInHasErrors = signInResult["errors"] is JArray { Count: > 0 };

        Assert.True(signInResponse.IsSuccessStatusCode, $"Member '{memberNumber}': accountMemberSignIn returned HTTP {(int)signInResponse.StatusCode} after password reset.");
        Assert.True(!signInHasErrors, $"Member '{memberNumber}': accountMemberSignIn returned a GraphQL error after password reset.");

        JToken signIn = signInResult.RequireToken("data.accountMemberSignIn");
        SoftAssertHelper signInAssert = new();

        signInAssert.True(!signIn.RequireToken("requiresMFA").Value<bool>(), $"Member '{memberNumber}': expected the fresh non-production account not to require MFA.");
        signInAssert.True(signIn.RequireToken("tokenType").ToString().Is("Bearer"), $"Member '{memberNumber}': expected sign-in token type 'Bearer' after password reset.");
        signInAssert.True(!signIn.RequireToken("accessToken").ToString().IsNullOrWhiteSpace(), $"Member '{memberNumber}': expected an access token after password reset sign-in.");
        signInAssert.True(!signIn.RequireToken("refreshToken").ToString().IsNullOrWhiteSpace(), $"Member '{memberNumber}': expected a refresh token after password reset sign-in.");
        signInAssert.True(!signIn.RequireToken("idToken").ToString().IsNullOrWhiteSpace(), $"Member '{memberNumber}': expected an ID token after password reset sign-in.");
        signInAssert.AssertAll();
    }
}
