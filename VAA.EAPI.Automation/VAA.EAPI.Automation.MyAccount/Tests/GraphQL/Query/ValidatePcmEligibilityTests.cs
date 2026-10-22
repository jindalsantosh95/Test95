using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.MyAccount.Constants;
using VAA.EAPI.Automation.MyAccount.Requests.Endpoint;

namespace VAA.EAPI.Automation.MyAccount.Tests.GraphQlTests.Query
{
    public sealed class ValidatePcmEligibilityTests
    {
        private readonly string env = Config.Environment;

        [Fact]
        [Trait("Category", Traits.MyAccountGQLRegressionTests)]
        public async Task TC01_ValidatePcmEligibility_RejectsInvalidUniqueId()
        {
            HttpResponseMessage response = GraphQL.ValidatePcmEligibility(env, "UID-INVALID").EnsureIsSuccessStatusCode();
            string body = await response.Content.ReadAsStringAsync();
            JObject result = JObject.Parse(body);

            // Expected GraphQL errors retain an HTTP 200 response, so assert the error contract in the body.
            Assert.Equal("422", result.RequireToken("errors[0].extensions.code").ToString());
            Assert.Contains("Invalid Unique ID.", result.RequireToken("errors[0].extensions.response").ToString());
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [Trait("Category", Traits.MyAccountGQLRegressionTests)]
        public async Task TC02_ValidatePcmEligibility_RejectsMissingUniqueId(string? uniqueId)
        {
            HttpResponseMessage response = GraphQL.ValidatePcmEligibility(env, uniqueId).EnsureIsSuccessStatusCode();
            string body = await response.Content.ReadAsStringAsync();
            JObject result = JObject.Parse(body);

            Assert.Equal("500", result.RequireToken("errors[0].extensions.code").ToString());
            Assert.True(
                result.RequireToken("errors[0].message").ToString().Contains("uniqueId", StringComparison.OrdinalIgnoreCase),
                "Expected the missing unique ID error to identify the uniqueId parameter.");
        }

        [Theory]
        [InlineData("5RZC1P")]
        [InlineData("7M4QZ9")]
        [InlineData("9FQ2XM")]
        [InlineData("A8T2KJ")]
        [Trait("Category", Traits.LocalRun)]
        public async Task TC03_ValidatePcmEligibility_ReturnsEligibleResponseForKnownUniqueId(string uniqueId)
        {
            HttpResponseMessage response = GraphQL.ValidatePcmEligibility(env, uniqueId).EnsureGQLResponseSuccess();
            string body = await response.Content.ReadAsStringAsync();
            JObject result = JObject.Parse(body);

            // A valid PCM ID may be either claimed or unclaimed; eligibility must remain true in both states.
            JToken eligibility = result.RequireToken("data.validatePcmEligibility");
            Assert.True(eligibility.RequireToken("valid").Value<bool>(), $"Expected unique ID '{uniqueId}' to be eligible.");
        }
    }
}
