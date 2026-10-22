using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.MyAccount.Constants;
using VAA.EAPI.Automation.MyAccount.Helpers;
using VAA.EAPI.Automation.MyAccount.Requests.Endpoint;

namespace VAA.EAPI.Automation.MyAccount.Tests.GraphQlTests.Mutation;

public sealed class ClaimPartnerFlightPointsTests
{
    private const string MissingRequiredRequestErrorMessage = "Unhandled exception (Value cannot be null. (Parameter 'request'))";
    private const string ValidationErrorCode = "500";
    private static readonly string env = Config.Environment;

    [Theory]
    [InlineData("fcNumber")]
    [InlineData("ticketNumber")]
    [InlineData("departureDate")]
    [InlineData("departureFrom")]
    [InlineData("arrivingAt")]
    [Trait("Category", Traits.MyAccountGQLDevDTScripts)]
    [Trait("Category", Traits.MyAccountGQLRegressionTests)]
    public async Task TC01_RejectsRequestWithMissingRequiredField(string missingField)
    {
        Dictionary<string, string> loyaltyAccount = RetrieveFromStorageTable.FetchLoyaltyAccountInfoExcludingPoints("Red");
        Assert.True(!loyaltyAccount.ContainsKey("error"), loyaltyAccount.GetValueOrDefault("error", "Expected a valid loyalty account for Partner-flight claim validation."));

        string memberNumber = loyaltyAccount["rowKey"];
        string token = await EntraTokenHelper.GetIdToken(env, string.Empty, memberNumber, Application.LoginPassword);
        Assert.True(!token.IsNullOrEmpty(), $"Flying Club member '{memberNumber}': expected a non-empty login token for Partner-flight claim validation.");

        // Null one resolver-required field so this validation test cannot create a Partner-flight claim.
        JObject request = new()
        {
            ["fcNumber"] = memberNumber,
            ["airline"] = "AF",
            ["ticketNumber"] = "5487626683478",
            ["flightNumber"] = "612",
            ["cabinClass"] = "Y-Economy",
            ["departureDate"] = "2024-06-18",
            ["departureFrom"] = "LHR",
            ["arrivingAt"] = "DEL",
            ["seatNumber"] = "35C",
            ["pnrNumber"] = "WR474"
        };
        request[missingField] = JValue.CreateNull();

        using HttpResponseMessage response = GraphQL.ClaimPartnerFlightPoints(env, token, request);
        JObject responseBody = JObject.Parse(await response.Content.ReadAsStringAsync());
        SoftAssertHelper softAssert = new();

        softAssert.True(response.IsSuccessStatusCode, $"Flying Club member '{memberNumber}': expected missing '{missingField}' to return an HTTP 200 GraphQL error envelope.");
        softAssert.True(responseBody.SelectToken("data.claimPartnerFlightPoints") is null or { Type: JTokenType.Null }, $"Flying Club member '{memberNumber}': expected no Partner-flight claim response when '{missingField}' is missing.");
        softAssert.True(responseBody.RequireToken("errors[0].message").ToString().Is(MissingRequiredRequestErrorMessage), $"Flying Club member '{memberNumber}': expected the documented missing '{missingField}' message.");
        softAssert.True(responseBody.RequireToken("errors[0].extensions.code").ToString().Is(ValidationErrorCode), $"Flying Club member '{memberNumber}': expected error code '{ValidationErrorCode}' when '{missingField}' is missing.");
        softAssert.AssertAll();
    }
}
