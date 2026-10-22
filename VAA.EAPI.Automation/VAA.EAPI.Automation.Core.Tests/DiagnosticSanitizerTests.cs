using VAA.EAPI.Automation.Core.Diagnostics;
using Xunit;

namespace VAA.EAPI.Automation.Core.Tests;

public sealed class DiagnosticSanitizerTests
{
    [Theory]
    [InlineData("Authorization: Bearer header-secret", "header-secret")]
    [InlineData("Authorization=Basic basic-secret", "basic-secret")]
    [InlineData("request failed with Bearer standalone-secret", "standalone-secret")]
    [InlineData("paymentReferenceId=payment-secret", "payment-secret")]
    [InlineData("token: \"quoted-secret\"", "quoted-secret")]
    [InlineData("X-Vaa_digitalfirst: hpp-access-secret", "hpp-access-secret")]
    [InlineData("Ocp-Apim-Subscription-Key=subscription-secret", "subscription-secret")]
    [InlineData("{\"paymentReferenceId\":\"json-payment-secret\"}", "json-payment-secret")]
    [InlineData("{\"client_secret\":\"json-client-secret\"}", "json-client-secret")]
    [InlineData("{\"authorization\":\"Bearer json-auth-secret\"}", "json-auth-secret")]
    public void Sanitize_RemovesSensitiveValues(string input, string secret)
    {
        string result = DiagnosticSanitizer.Sanitize(input);

        Assert.DoesNotContain(secret, result);
        Assert.Contains("[REDACTED]", result);
    }

    [Fact]
    public void Sanitize_RemovesUrlQueryAndFragmentButRetainsUsefulPath()
    {
        const string input =
            "Navigation failed at https://hpp.example.test/pay?paymentReferenceId=secret#card";

        string result = DiagnosticSanitizer.Sanitize(input);

        Assert.Equal("Navigation failed at https://hpp.example.test/pay", result);
    }

    [Fact]
    public void SanitizeUrl_RemovesUserInformation()
    {
        string result = DiagnosticSanitizer.SanitizeUrl(
            "https://diagnostic-user:diagnostic-password@hpp.example.test/pay?reference=secret");

        Assert.Equal("https://hpp.example.test/pay", result);
        Assert.DoesNotContain("diagnostic-user", result);
        Assert.DoesNotContain("diagnostic-password", result);
    }

    [Fact]
    public void Sanitize_RetainsNonSensitiveFailureEvidence()
    {
        const string input = "POST /graphQL/book/public returned 422 reason=invalid passenger data";

        Assert.Equal(input, DiagnosticSanitizer.Sanitize(input));
    }
}
