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

public sealed class AccountEntraOTPGenerateTests
{
    private const string InvalidOrMissingEmailErrorCode = "G013";
    private const string InvalidOrMissingEmailErrorMessage = "Email id in the request is Invalid or Missing";
    private const string EmailErrorSource = "Consuming application";
    private static readonly string env = Config.Environment;

    [Fact]
    [Trait("Category", Traits.MyAccountGQLRegressionTests)]
    [Trait("Category", Traits.GqlEntraAuthentication)]
    [Trait("Category", Traits.IdentityMSRelatedGraphqltests)]
    [Trait("Category", Traits.CustomerMSRelatedGraphqltests)]
    public async Task TC01_GeneratesOtpTokenForNewEmail()
    {
        string emailId = $"entraotpgenerate{Random.Shared.NextInt64(100000000000, 999999999999)}@example.com";

        // A new email follows the IdentityMS StartSignUp path and returns an OTP validation token.
        using HttpResponseMessage otpResponse = GraphQL.AccountEntraOTPGenerate(env, emailId).EnsureGQLResponseSuccess();
        JObject otpResult = JObject.Parse(await otpResponse.Content.ReadAsStringAsync());
        string otpToken = otpResult.RequireToken("data.accountEntraOTPGenerate").ToString();

        // Entra OTPs are persisted in the shared SIT identity table before the validation step consumes them.
        TableStorageService<OtpRequestTableEntity> otpRequestTableStorageService = new() { ConnectionString = Vault.Default.GetSecretValue("IdentityStorageAccountConnectionString-sit"), TableName = "OTPRequest" };
        int otp = await EnrolmentHelper.GetOTPByEmailId(otpRequestTableStorageService, emailId, 8);
        SoftAssertHelper softAssert = new();

        softAssert.True(!otpToken.IsNullOrWhiteSpace(), $"Email '{emailId}': expected accountEntraOTPGenerate to return a non-empty OTP token.");
        softAssert.True(otp is > 9999999 and <= 99999999, $"Email '{emailId}': expected the OTPRequest table to contain an eight-digit numeric OTP.");
        softAssert.AssertAll();
    }

    [Fact]
    [Trait("Category", Traits.MyAccountGQLRegressionTests)]
    [Trait("Category", Traits.GqlEntraAuthentication)]
    public async Task TC02_RejectsMissingEmail()
    {
        // The mutation validates a blank email before it calls the customer or IdentityMS services.
        using HttpResponseMessage otpResponse = GraphQL.AccountEntraOTPGenerate(env, string.Empty);
        JObject otpResult = JObject.Parse(await otpResponse.Content.ReadAsStringAsync());
        SoftAssertHelper softAssert = new();

        softAssert.True(otpResponse.IsSuccessStatusCode, "Email '<empty>': expected GraphQL to return HTTP 200 with an error envelope for a missing email.");
        softAssert.True(otpResult.SelectToken("data.accountEntraOTPGenerate") is null or { Type: JTokenType.Null }, "Email '<empty>': expected no OTP token for a missing email.");
        softAssert.True(otpResult.RequireToken("errors[0].message").ToString().Is("OTP Generation failed"), "Email '<empty>': expected the documented OTP-generation error message.");
        softAssert.True(otpResult.RequireToken("errors[0].extensions.code").ToString().Is(InvalidOrMissingEmailErrorCode), $"Email '<empty>': expected error code '{InvalidOrMissingEmailErrorCode}'.");
        softAssert.True(otpResult.RequireToken("errors[0].extensions.message").ToString().Is(InvalidOrMissingEmailErrorMessage), $"Email '<empty>': expected error message '{InvalidOrMissingEmailErrorMessage}'.");
        softAssert.True(otpResult.RequireToken("errors[0].extensions.source").ToString().Is(EmailErrorSource), $"Email '<empty>': expected error source '{EmailErrorSource}'.");
        softAssert.AssertAll();
    }
}
