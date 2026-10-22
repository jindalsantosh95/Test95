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
public sealed class AccountEntraOtpValidationTests
{
    private static readonly string env = Config.Environment;

    [Fact]
    [Trait("Category", Traits.MyAccountGQLRegressionTests)]
    [Trait("Category", Traits.GqlEntraAuthentication)]
    public async Task TC01_ReturnsContinuationTokenForValidSignUpOtp()
    {
        (string emailId, string otpToken, int otp) = await GenerateEntraOtpAsync();

        // A valid SignUp OTP exchanges the generation token for the continuation token consumed by Entra enrolment.
        using HttpResponseMessage validationResponse = GraphQL.AccountEntraOtpValidation(env, otpToken, otp.ToString(), Application.ValidationRequestType.SignUp).EnsureGQLResponseSuccess();
        JObject validationResult = JObject.Parse(await validationResponse.Content.ReadAsStringAsync());
        string continuationToken = validationResult.RequireToken("data.accountEntraOtpValidation.continuationToken").ToString();

        Assert.True(!continuationToken.IsNullOrWhiteSpace(), $"Email '{emailId}': expected accountEntraOtpValidation to return a non-empty continuation token.");
    }

    [Fact]
    [Trait("Category", Traits.MyAccountGQLRegressionTests)]
    [Trait("Category", Traits.GqlEntraAuthentication)]
    public async Task TC02_RejectsIncorrectSignUpOtp()
    {
        (string emailId, string otpToken, int otp) = await GenerateEntraOtpAsync();
        int incorrectOtp = otp == 12345678 ? 87654321 : 12345678;

        // Use a different eight-digit code so the test exercises IdentityMS validation without consuming the valid OTP.
        using HttpResponseMessage validationResponse = GraphQL.AccountEntraOtpValidation(env, otpToken, incorrectOtp.ToString(), Application.ValidationRequestType.SignUp);
        JObject validationResult = JObject.Parse(await validationResponse.Content.ReadAsStringAsync());
        SoftAssertHelper softAssert = new();

        softAssert.True(validationResponse.IsSuccessStatusCode, $"Email '{emailId}': expected GraphQL to return HTTP 200 with an error envelope for an incorrect OTP.");
        softAssert.True(validationResult.SelectToken("data.accountEntraOtpValidation") is null or { Type: JTokenType.Null }, $"Email '{emailId}': expected no continuation token for an incorrect OTP.");
        softAssert.True(validationResult.RequireToken("errors[0].message").ToString().Is("OTP validation failed"), $"Email '{emailId}': expected the documented OTP-validation error message.");
        softAssert.True(!validationResult.RequireToken("errors[0].extensions.code").ToString().IsNullOrWhiteSpace(), $"Email '{emailId}': expected IdentityMS to return an OTP-validation error code.");
        softAssert.True(!validationResult.RequireToken("errors[0].extensions.message").ToString().IsNullOrWhiteSpace(), $"Email '{emailId}': expected IdentityMS to return an OTP-validation error message.");
        softAssert.True(!validationResult.RequireToken("errors[0].extensions.source").ToString().IsNullOrWhiteSpace(), $"Email '{emailId}': expected IdentityMS to return an OTP-validation error source.");
        softAssert.AssertAll();
    }

    [Fact]
    [Trait("Category", Traits.MyAccountGQLRegressionTests)]
    [Trait("Category", Traits.GqlEntraAuthentication)]
    [Trait("Category", Traits.CustomerMSRelatedGraphqltests)]
    public async Task TC03_ReturnsContinuationTokenForValidResetPasswordOtp()
    {
        (string emailId, string memberNumber, _, _, _, _, _, _, _) = await EnrolmentHelper.EntraEnrolment("WEB-MYA", "Web");

        // Entra password-reset OTPs are eight digits and always persist in the shared SIT identity table.
        using HttpResponseMessage resetStartResponse = GraphQL.AccountPasswordResetStart(env, emailId).EnsureGQLResponseSuccess();
        JObject resetStartResult = JObject.Parse(await resetStartResponse.Content.ReadAsStringAsync());
        string resetToken = resetStartResult.RequireToken("data.accountPasswordResetStart.continuationToken").ToString();
        TableStorageService<OtpRequestTableEntity> otpRequestTableStorageService = new() { ConnectionString = Vault.Default.GetSecretValue("IdentityStorageAccountConnectionString-sit"), TableName = "OTPRequest" };
        int resetOtp = await EnrolmentHelper.GetOTPByEmailId(otpRequestTableStorageService, emailId, 8);

        using HttpResponseMessage validationResponse = GraphQL.AccountEntraOtpValidation(env, resetToken, resetOtp.ToString(), Application.ValidationRequestType.ResetPassword).EnsureGQLResponseSuccess();
        JObject validationResult = JObject.Parse(await validationResponse.Content.ReadAsStringAsync());
        string continuationToken = validationResult.RequireToken("data.accountEntraOtpValidation.continuationToken").ToString();
        SoftAssertHelper softAssert = new();

        softAssert.True(!resetToken.IsNullOrWhiteSpace(), $"Member '{memberNumber}': expected accountPasswordResetStart to return a reset token.");
        softAssert.True(resetOtp is >= 10000000 and <= 99999999, $"Member '{memberNumber}': expected OTPRequest to contain an eight-digit reset OTP.");
        softAssert.True(!continuationToken.IsNullOrWhiteSpace(), $"Member '{memberNumber}': expected ResetPassword OTP validation to return a continuation token.");
        softAssert.AssertAll();
    }

    // Generate and read a fresh OTP per test so validation remains independent during parallel runs.
    private static async Task<(string emailId, string otpToken, int otp)> GenerateEntraOtpAsync()
    {
        string emailId = $"entraotpvalidation{Random.Shared.NextInt64(100000000000, 999999999999)}@example.com";
        using HttpResponseMessage generationResponse = GraphQL.AccountEntraOTPGenerate(env, emailId).EnsureGQLResponseSuccess();
        JObject generationResult = JObject.Parse(await generationResponse.Content.ReadAsStringAsync());
        string otpToken = generationResult.RequireToken("data.accountEntraOTPGenerate").ToString();
        TableStorageService<OtpRequestTableEntity> otpRequestTableStorageService = new() { ConnectionString = Vault.Default.GetSecretValue("IdentityStorageAccountConnectionString-sit"), TableName = "OTPRequest" };
        int otp = await EnrolmentHelper.GetOTPByEmailId(otpRequestTableStorageService, emailId, 8);

        return (emailId, otpToken, otp);
    }
}
