using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Text;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using Xunit.Abstractions;


namespace VAA.EAPI.Automation.MyAccount.Tests.MicroserviceTests.AddressLookUpMS
{
    public class AddressLookUpTests : IClassFixture<TokenFixture>
    {
        private readonly ITestOutputHelper output;
        private readonly TokenFixture tokenFixture;

        public static string env = Config.Environment;
        public static string resouceId = Vault.Default.GetSecretValue("MYA-AddressResource");

        public AddressLookUpTests(ITestOutputHelper output, TokenFixture tokenFixture)
        {
            this.output = output;
            this.tokenFixture = tokenFixture;

            this.tokenFixture.AddTokenForMicroservices("msToken", resouceId);
        }

        [Theory]
        [InlineData("GB", "M13 9AX", "37")]
        [InlineData("GB", "M1 1AE", "Apartment 1")]
        [InlineData("GB", "B33 8TX", "1")]
        [InlineData("US", "20500-0005", "Pennsylvania Ave NW")]
        [Trait("Category", "AddressLookUpMicroService")]
        public async Task AddressLookupTests_PostcodeChecks_Success(string countryCode, string postCode, string streetName)
        {
            System.Net.Http.HttpResponseMessage addressLookUpResponse = Requests.Endpoint.Microservice.AddressLookUp(env, tokenFixture.GetToken("msToken"), countryCode, postCode, streetName).EnsureIsSuccessStatusCode();

            JArray addressLookUpResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await addressLookUpResponse.Content.ReadAsStringAsync());

            if (countryCode.Equals("US"))
            {
                Assert.True(addressLookUpResult[0]["postalCode"].ToString().Contains(postCode), $"Validtion for Post Code returned was not as expected, returned {addressLookUpResult[0]["postalCode"].ToString()} and expected post code {postCode}");
            }
            else
            {
                Assert.True(addressLookUpResult[0]["postalCode"].ToString().Equals(postCode), $"Validtion for Post Code returned was not as expected, returned {addressLookUpResult[0]["postalCode"].ToString()} and expected post code {postCode}");
                Assert.True(addressLookUpResult[0]["countryCode"].ToString().Equals(countryCode), $"Validtion for Country code returned was not as expected, returned {addressLookUpResult[0]["countryCode"].ToString()} and expected {countryCode}");
                Assert.True(addressLookUpResult[0]["addressLine1"].ToString().Contains(streetName), $"Validtion for Address Line1 returned was not as expected, returned {addressLookUpResult[0]["addressLine1"].ToString()} and expected street name {streetName}");
            }
        }

        [Theory]
        [InlineData("GB", "M13 9AX", "37")]
        [InlineData("GB", "M1 1AE", "Apartment 1")]
        [InlineData("GB", "B33 8TX", "1")]
        [InlineData("US", "20500-0005", "Pennsylvania Ave NW")]
        [Trait("Category", "AddressLookUpMicroService")]
        public async Task AddressLookupTests_PostcodeChecks_ErrorValidations(string countryCode, string postCode, string streetName)
        {
            System.Net.Http.HttpResponseMessage addressLookUpResponse = Requests.Endpoint.Microservice.AddressDetailsV2(env, tokenFixture.GetToken("msToken"), countryCode, postCode, streetName).EnsureIsSuccessStatusCode();

            JArray addressLookUpResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await addressLookUpResponse.Content.ReadAsStringAsync());

            if (countryCode.Equals("US"))
            {
                Assert.True(addressLookUpResult[0]["postalCode"].ToString().Contains(postCode), $"Validtion for Post Code returned was not as expected, returned {addressLookUpResult[0]["postalCode"].ToString()} and expected post code {postCode}");
            }
            else
            {
                Assert.True(addressLookUpResult[0]["postalCode"].ToString().Equals(postCode), $"Validtion for Post Code returned was not as expected, returned {addressLookUpResult[0]["postalCode"].ToString()} and expected post code {postCode}");
                Assert.True(addressLookUpResult[0]["countryCode"].ToString().Equals(countryCode), $"Validtion for Country code returned was not as expected, returned {addressLookUpResult[0]["countryCode"].ToString()} and expected {countryCode}");
                Assert.True(addressLookUpResult[0]["addressLine1"].ToString().Contains(streetName), $"Validtion for Address Line1 returned was not as expected, returned {addressLookUpResult[0]["addressLine1"].ToString()} and expected street name {streetName}");
            }
        }

        [Fact]
        [Trait("Category", "AddressLookUpMicroService")]
        public async Task AddressLookupTests_PostcodeChecks_CountryCodeIsBlank()
        {
            string postCode = "SO140LT";
            string streetName = "70";

            System.Net.Http.HttpResponseMessage addressLookUpResponse = Requests.Endpoint.Microservice.AddressLookUp(env, tokenFixture.GetToken("msToken"), "", postCode, streetName);

            Assert.True(addressLookUpResponse.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity, $"422 Unprocessable Entity was not returned from AddressLookUp MS endpoint, returned {addressLookUpResponse.ReadResponseAsString()}");

            JArray addressLookUpResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await addressLookUpResponse.Content.ReadAsStringAsync());

            string errorMessage = "One of the mandatory field is missing in the request";

            Assert.True(addressLookUpResult[0]["code"].ToString().Equals("G018"), $"G018 error code not returned, returned {addressLookUpResult[0]["code"].ToString()}");

            Assert.True(addressLookUpResult[0]["message"].ToString().Equals(errorMessage), $"Error Message is not correct, response is {addressLookUpResult[0]["message"].ToString()}");
        }

        [Theory]
        [InlineData("GB")]
        [InlineData("US")]
        [Trait("Category", "AddressLookUpMicroService")]
        public async Task AddressLookupTests_PostcodeChecks_PostCodeIsBlank(string countryCode)
        {
            string streetName = "70";

            System.Net.Http.HttpResponseMessage addressLookUpResponse = Requests.Endpoint.Microservice.AddressLookUp(env, tokenFixture.GetToken("msToken"), countryCode, "", streetName);

            Assert.True(addressLookUpResponse.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity, $"422 Unprocessable Entity was not returned from AddressLookUp MS endpoint, returned {addressLookUpResponse.ReadResponseAsString()}");

            JArray addressLookUpResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await addressLookUpResponse.Content.ReadAsStringAsync());

            string errorMessage = "One of the mandatory field is missing in the request";

            Assert.True(addressLookUpResult[0]["code"].ToString().Equals("10149"), $"10149 error code not returned, returned {addressLookUpResult[0]["code"].ToString()}");

            Assert.True(addressLookUpResult[0]["message"].ToString().Equals(errorMessage), $"Error Message is not correct, response is {addressLookUpResult[0]["message"].ToString()}");
        }

        [Theory]
        [InlineData("GB", "CR2 6XH")]
        [InlineData("US", "15824-7213")]
        [Trait("Category", "AddressLookUpMicroService")]
        public async Task AddressLookupTests_PostcodeChecks_AddressLine1IsBlank(string countryCode, string postCode)
        {
            System.Net.Http.HttpResponseMessage addressLookUpResponse = Requests.Endpoint.Microservice.AddressLookUp(env, tokenFixture.GetToken("msToken"), countryCode, postCode, "");

            Assert.True(addressLookUpResponse.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity, $"422 Unprocessable Entity was not returned from AddressLookUp MS endpoint, returned {addressLookUpResponse.ReadResponseAsString()}");

            JArray addressLookUpResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await addressLookUpResponse.Content.ReadAsStringAsync());

            string errorMessage = "One of the mandatory field is missing in the request";

            Assert.True(addressLookUpResult[0]["code"].ToString().Equals("G016"), $"G016 error code not returned, returned {addressLookUpResult[0]["code"].ToString()}");

            Assert.True(addressLookUpResult[0]["message"].ToString().Equals(errorMessage), $"Error Message is not correct, response is {addressLookUpResult[0]["message"].ToString()}");
        }

        [Theory]
        [InlineData("IN", "700029", "37")]
        [InlineData("IL", "9985000", "Jerusalem")]
        [InlineData("AT", "5020", "1")]
        [InlineData("FR", "75000", "Avenue")]
        [Trait("Category", "AddressLookUpMicroService")]
        public async Task AddressLookupTests_PostcodeChecks_CountryIsOtherThanUKAndUS(string countryCode, string postCode, string streetName)
        {
            System.Net.Http.HttpResponseMessage addressLookUpResponse = Requests.Endpoint.Microservice.AddressLookUp(env, tokenFixture.GetToken("msToken"), countryCode, postCode, streetName);

            Assert.True(addressLookUpResponse.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity, $"422 Unprocessable Entity was not returned from AddressLookUp MS endpoint, returned {addressLookUpResponse.ReadResponseAsString()}");

            JArray addressLookUpResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await addressLookUpResponse.Content.ReadAsStringAsync());

            string errorMessage = "Country is not valid for address lookup";

            Assert.True(addressLookUpResult[0]["code"].ToString().Equals("10150"), $"10150 error code not returned, returned {addressLookUpResult[0]["code"].ToString()}");

            Assert.True(addressLookUpResult[0]["message"].ToString().Equals(errorMessage), $"Error Message is not correct, response is {addressLookUpResult[0]["message"].ToString()}");
        }


        [Theory]
        [InlineData("US", "34101-0841", "1")]
        [InlineData("GB", "SO140LT", "10")]
        [Trait("Category", "AddressLookUpMicroService")]
        public async Task AddressLookupTests_PostcodeChecks_NoAddressFound(string countryCode, string postCode, string streetName)
        {
            System.Net.Http.HttpResponseMessage addressLookUpResponse = Requests.Endpoint.Microservice.AddressLookUp(env, tokenFixture.GetToken("msToken"), countryCode, postCode, streetName);

            Assert.True(addressLookUpResponse.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity, $"422 Unprocessable Entity was not returned from AddressLookUp MS endpoint, returned {addressLookUpResponse.ReadResponseAsString()}");

            JArray addressLookUpResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await addressLookUpResponse.Content.ReadAsStringAsync());

            string errorMessage = "Data is unavailable for the set of input provided";

            Assert.True(addressLookUpResult[0]["code"].ToString().Equals("G007"), $"G007 error code not returned, returned {addressLookUpResult[0]["code"].ToString()}");

            Assert.True(addressLookUpResult[0]["message"].ToString().Equals(errorMessage), $"Error Message is not correct, response is {addressLookUpResult[0]["message"].ToString()}");
        }

        [Theory]
        [InlineData("GB", "GBR", "RH10 1DJ", "127")]
        [InlineData("GB", "GBR", "RH11 7TU", "28")]
        [InlineData("US", "USA", "15824-1219", "1805")]
        [Trait("Category", "AddressLookUpMicroService")]
        public async Task AddressLookupTests_PostcodeChecks_WesalResponse(string countryCode, string countryCodeWesal, string postCode, string streetName)
        {
            System.Net.Http.HttpResponseMessage addressLookUpWesalResponse = Requests.Endpoint.Microservice.AddressLookUpWesalEndpoint(countryCodeWesal, postCode, streetName).EnsureIsSuccessStatusCode();

            string result = addressLookUpWesalResponse.ReadResponseAsString();

            System.Net.Http.HttpResponseMessage addressLookUpResponse = Requests.Endpoint.Microservice.AddressLookUp(env, tokenFixture.GetToken("msToken"), countryCode, postCode, streetName);

            Assert.True(addressLookUpResponse.StatusCode == System.Net.HttpStatusCode.OK, $"200 OK was not returned from AddressLookUp GraphQl endpoint, returned {addressLookUpResponse.ReadResponseAsString()}");

            JArray addressLookUpResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await addressLookUpResponse.Content.ReadAsStringAsync());

            Assert.True(addressLookUpResult[0]["postalCode"].ToString().Equals(RegexCompiled.Match(result, @"(postOrZipCode="")([^""]*)("")").Groups[2].Value), $"Validtion for Post code returned was not as expected, returned {addressLookUpResult[0]["postalCode"].ToString()} and expected post code is {RegexCompiled.Match(result, @"(postOrZipCode="")([^""]*)("")").Groups[2].Value}, request {addressLookUpWesalResponse.ReadRequestAsString()}");
            if ((RegexCompiled.Match(result, @"(country="")([^""]*)("")").Groups[2].Value) == "UNITED STATES OF AMERICA")
            {
                Assert.True(addressLookUpResult[0]["countryName"].ToString().Equals("United States"));
            }
            else
                Assert.True(addressLookUpResult[0]["countryName"].ToString().Is(RegexCompiled.Match(result, @"(country="")([^""]*)("")").Groups[2].Value), $"Validtion for Country was not as expected, returned {addressLookUpResult[0]["countryName"].ToString()} and expected {RegexCompiled.Match(result, @"(country="")([^""]*)("")").Groups[2].Value}");
            Assert.True(addressLookUpResult[0]["city"].ToString().Is(RegexCompiled.Match(result, @"(city="")([^""]*)("")").Groups[2].Value), $"Validtion for City was not as expected, returned {addressLookUpResult[0]["city"].ToString()} and expected street name {RegexCompiled.Match(result, @"(city="")([^""]*)("")").Groups[2].Value}");
            Assert.True(addressLookUpResult[0]["addressLine1"].ToString().Contains(RegexCompiled.Match(result, @"(address1="")([^""]*)("")").Groups[2].Value), $"Validtion for Address Line1 was not as expected, returned {addressLookUpResult[0]["addressLine1"].ToString()} and expected street name {RegexCompiled.Match(result, @"(address1="")([^""]*)("")").Groups[2].Value}");
        }
    }
}
