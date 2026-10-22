using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Text;
using VAA.EAPI.Automation.MyAccount.Constants;
using VAA.EAPI.Automation.MyAccount.Requests.Endpoint;

namespace VAA.EAPI.Automation.MyAccount.Tests.GraphQlTests.Query
{
    public sealed class AccountAddressDetailsTests
    {
        private readonly string env = Config.Environment;

        private static void AssertValidationError(JObject result, string expectedErrorCode, string expectedProviderCode, string expectedMessage)
        {
            // Compare the provider response without emitting it because downstream error bodies can contain sensitive or internal details.
            JToken? actualErrorCodeToken = result.SelectToken("errors[0].extensions.code");
            JToken? actualErrorResponseToken = result.SelectToken("errors[0].extensions.response");
            string actualErrorCodeState = actualErrorCodeToken is null || actualErrorCodeToken.Type == JTokenType.Undefined
                ? "missing"
                : actualErrorCodeToken.Type == JTokenType.Null ? "null" : "present";
            string actualErrorResponseState = actualErrorResponseToken is null || actualErrorResponseToken.Type == JTokenType.Undefined
                ? "missing"
                : actualErrorResponseToken.Type == JTokenType.Null ? "null" : "present";
            bool errorCodeMatches = actualErrorCodeToken is { Type: not JTokenType.Null and not JTokenType.Undefined }
                && actualErrorCodeToken.ToString().Is(expectedErrorCode);
            bool providerCodeMatches = actualErrorResponseToken is { Type: not JTokenType.Null and not JTokenType.Undefined }
                && actualErrorResponseToken.ToString().Contains(expectedProviderCode, StringComparison.OrdinalIgnoreCase);
            bool providerMessageMatches = actualErrorResponseToken is { Type: not JTokenType.Null and not JTokenType.Undefined }
                && actualErrorResponseToken.ToString().Contains(expectedMessage, StringComparison.OrdinalIgnoreCase);

            Assert.True(errorCodeMatches, $"Expected GraphQL validation error code '{expectedErrorCode}', but actual code was {actualErrorCodeState}.");
            Assert.True(providerCodeMatches, $"Expected provider error code '{expectedProviderCode}', but provider response was {actualErrorResponseState}.");
            Assert.True(providerMessageMatches, $"Expected provider message '{expectedMessage}', but provider response was {actualErrorResponseState}.");
        }

        [Theory]
        [InlineData("11 Sunnymead", "GB", "RH11 7DU")]
        [Trait("Category", Traits.MyAccountGQLRegressionTests)]
        public async Task TC01_AccountAddressDetails_ReturnsRequestedAddress(string addressLine1, string countryCode, string postcode)
        {
            HttpResponseMessage response = GraphQL.AccountAddressDetails(env, addressLine1, countryCode, postcode).EnsureGQLResponseSuccess();
            string body = await response.Content.ReadAsStringAsync();

            JObject result = JObject.Parse(body);
            JToken addresses = result.RequireToken("data.accountAddressDetails");
            Assert.Equal(JTokenType.Array, addresses.Type);
            IEnumerable<JToken> returnedAddresses = addresses.Children();
            Assert.NotEmpty(returnedAddresses);

            // The provider may return multiple addresses and expand a house number, so match values instead of array position.
            Assert.Contains(returnedAddresses, address =>
                address.RequireToken("postalCode").ToString().Is(postcode) &&
                address.RequireToken("countryCode").ToString().Is(countryCode) &&
                address.RequireToken("addressLine1").ToString().Contains(addressLine1, StringComparison.OrdinalIgnoreCase));
        }

        [Theory]
        [InlineData("GB", "CR2 6XH", "550")]
        [InlineData("GB", "DE55 1DT", "10")]
        [InlineData("GB", "SW12 0PS", "4")]
        [InlineData("US", "15824-7213", "2427")]
        [Trait("Category", Traits.MyAccountGQLRegressionTests)]
        [Trait("Category", "AddressLookUpGraphQl")]
        [Trait("Category", Traits.MyAccountGQLDevDTScripts)]
        public async Task TC02_AccountAddressDetails_ReturnsAddressForCompletePostcode(string countryCode, string postCode, string streetName)
        {
            HttpResponseMessage response = GraphQL.AccountAddressDetails(env, streetName, countryCode, postCode).EnsureGQLResponseSuccess();
            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
            JArray addresses = Assert.IsType<JArray>(result.RequireToken("data.accountAddressDetails"));

            Assert.Contains(addresses, address =>
                address.RequireToken("countryCode").ToString().Is(countryCode) &&
                address.RequireToken("postalCode").ToString().Is(postCode) &&
                address.RequireToken("addressLine1").ToString().Contains(streetName, StringComparison.OrdinalIgnoreCase));
        }

        [Theory]
        [InlineData("GB", "GU16", "28")]
        [InlineData("US", "15824", "2427")]
        [Trait("Category", Traits.MyAccountGQLRegressionTests)]
        [Trait("Category", "AddressLookUpGraphQl")]
        [Trait("Category", Traits.MyAccountGQLDevDTScripts)]
        public async Task TC03_AccountAddressDetails_ReturnsAddressForPartialPostcode(string countryCode, string postCode, string streetName)
        {
            HttpResponseMessage response = GraphQL.AccountAddressDetails(env, streetName, countryCode, postCode).EnsureGQLResponseSuccess();
            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
            JArray addresses = Assert.IsType<JArray>(result.RequireToken("data.accountAddressDetails"));

            Assert.Contains(addresses, address =>
                address.RequireToken("countryCode").ToString().Is(countryCode) &&
                address.RequireToken("postalCode").ToString().Contains(postCode, StringComparison.OrdinalIgnoreCase) &&
                address.RequireToken("addressLine1").ToString().Contains(streetName, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        [Trait("Category", Traits.MyAccountGQLRegressionTests)]
        [Trait("Category", "AddressLookUpGraphQl")]
        [Trait("Category", Traits.MyAccountGQLDevDTScripts)]
        public async Task TC04_AccountAddressDetails_ReturnsValidationErrorForMissingCountryCode()
        {
            HttpResponseMessage response = GraphQL.AccountAddressDetails(env, "70", string.Empty, "SO140LT");
            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());

            AssertValidationError(result, "422", "G018", "One of the mandatory field is missing in the request");
        }

        [Theory]
        [InlineData("GB")]
        [InlineData("US")]
        [Trait("Category", Traits.MyAccountGQLRegressionTests)]
        [Trait("Category", "AddressLookUpGraphQl")]
        [Trait("Category", Traits.MyAccountGQLDevDTScripts)]
        public async Task TC05_AccountAddressDetails_ReturnsValidationErrorForMissingPostcode(string countryCode)
        {
            HttpResponseMessage response = GraphQL.AccountAddressDetails(env, "70", countryCode, string.Empty);
            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());

            AssertValidationError(result, "422", "10149", "One of the mandatory field is missing in the request");
        }

        [Theory]
        [InlineData("GB", "CR2 6XH")]
        [InlineData("US", "15824-7213")]
        [Trait("Category", Traits.MyAccountGQLRegressionTests)]
        [Trait("Category", "AddressLookUpGraphQl")]
        [Trait("Category", Traits.MyAccountGQLDevDTScripts)]
        public async Task TC06_AccountAddressDetails_ReturnsValidationErrorForMissingAddressLine1(string countryCode, string postCode)
        {
            HttpResponseMessage response = GraphQL.AccountAddressDetails(env, string.Empty, countryCode, postCode);
            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());

            AssertValidationError(result, "422", "G016", "One of the mandatory field is missing in the request");
        }

        [Theory]
        [InlineData("IN", "700029", "37")]
        [InlineData("IL", "9985000", "Jerusalem")]
        [InlineData("AT", "5020", "1")]
        [InlineData("FR", "75000", "Avenue")]
        [Trait("Category", Traits.MyAccountGQLRegressionTests)]
        [Trait("Category", "AddressLookUpGraphQl")]
        [Trait("Category", Traits.MyAccountGQLDevDTScripts)]
        public async Task TC07_AccountAddressDetails_ReturnsValidationErrorForUnsupportedCountry(string countryCode, string postCode, string streetName)
        {
            HttpResponseMessage response = GraphQL.AccountAddressDetails(env, streetName, countryCode, postCode);
            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());

            AssertValidationError(result, "422", "10150", "Country is not valid for address lookup");
        }

        [Theory]
        [InlineData("GB", "RH117DU", "20")]
        [Trait("Category", Traits.MyAccountGQLRegressionTests)]
        [Trait("Category", "AddressLookUpGraphQl")]
        [Trait("Category", Traits.MyAccountGQLDevDTScripts)]
        public async Task TC08_AccountAddressDetails_ReturnsValidationErrorWhenAddressIsUnavailable(string countryCode, string postCode, string streetName)
        {
            HttpResponseMessage response = GraphQL.AccountAddressDetails(env, streetName, countryCode, postCode);
            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());

            AssertValidationError(result, "422", "G007", "Data is unavailable for the set of input provided");
        }

        [Theory]
        [InlineData("GB", "GBR", "RH11 9LG", "20")]
        [InlineData("GB", "GBR", "SO14 0LT", "72")]
        [Trait("Category", Traits.MyAccountGQLRegressionTests)]
        [Trait("Category", "AddressLookUpGraphQl")]
        [Trait("Category", Traits.MyAccountGQLDevDTScripts)]
        public async Task TC09_AccountAddressDetails_MatchesWesalAddress(string countryCode, string wesalCountryCode, string postCode, string streetName)
        {
            // Wesal is the downstream address source, so compare its returned address with the GraphQL projection.
            HttpResponseMessage wesalResponse = Requests.Endpoint.Microservice.AddressLookUpWesalEndpoint(wesalCountryCode, postCode, streetName).EnsureIsSuccessStatusCode();
            string wesalResult = wesalResponse.ReadResponseAsString();
            HttpResponseMessage response = GraphQL.AccountAddressDetails(env, streetName, countryCode, postCode).EnsureGQLResponseSuccess();
            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
            JArray addresses = Assert.IsType<JArray>(result.RequireToken("data.accountAddressDetails"));
            string expectedPostCode = RegexCompiled.Match(wesalResult, @"(postOrZipCode="")([^""]*)("")").Groups[2].Value;
            string expectedCountry = RegexCompiled.Match(wesalResult, @"(country="")([^""]*)("")").Groups[2].Value;
            string expectedCity = RegexCompiled.Match(wesalResult, @"(city="")([^""]*)("")").Groups[2].Value;
            string expectedAddressLine1 = RegexCompiled.Match(wesalResult, @"(address1="")([^""]*)("")").Groups[2].Value;

            // Address search can return multiple candidates, so match the Wesal address instead of its response position.
            Assert.Contains(addresses, address =>
                address.RequireToken("postalCode").ToString().Is(expectedPostCode) &&
                address.RequireToken("countryName").ToString().Is(expectedCountry) &&
                address.RequireToken("city").ToString().Is(expectedCity) &&
                address.RequireToken("addressLine1").ToString().Contains(expectedAddressLine1, StringComparison.OrdinalIgnoreCase));
        }

    }
}
