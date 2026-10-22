using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.MobileApp.Constants;
using VAA.EAPI.Automation.MobileApp.Helpers;

namespace VAA.EAPI.Automation.MobileApp.Tests.AirportData
{
    public class AirportDataTests
    {
        [Fact]
        [Trait("Category", Traits.AirportData)]
        public async Task SearchAirportInfo_AirportCodes()
        {
            string airportDataPath = Path.Combine(AppContext.BaseDirectory, "Data", "AirportData.json");
            string jsonData = File.ReadAllText(airportDataPath);

            ReturnAirportCodes airportCodeBuilder = new ReturnAirportCodes();

            List<string> allAirportCodes = airportCodeBuilder.ReturnRandomSpecifiedAmountOfAirportCodes(jsonData, 20);

            //Call for all airport codes - leaving in for future potential use.
            //List<string> allAirportCodes = airportCodeBuilder.ReturnAllAirportCodes(jsonData);

            foreach (string airportCode in allAirportCodes)
            {
                //GQL calls
                HttpResponseMessage gqlAirportDataResponse = Requests.Endpoints.GraphQLEndpoint.SearchAirportInfo(airportCode).EnsureGQLResponseSuccess();
                dynamic? gqlResult = JsonConvert.DeserializeObject<dynamic>(await gqlAirportDataResponse.Content.ReadAsStringAsync());
                string gqlResultString = gqlResult.data.searchAirportInfo.listOfCities.ToString();
                List<dynamic>? listOfCities = JsonConvert.DeserializeObject<List<dynamic>>(gqlResultString);

                List<dynamic> gqlResponseAirportCodes = listOfCities.Select(city => city.airportCode.ToString()).ToList();
                List<dynamic> gqlResponseAirportName = listOfCities.Select(city => city.airportName.ToString()).ToList();
                List<dynamic> gqlResponseAirportFullName = listOfCities.Select(city => city.airportFullName.ToString()).ToList();
                List<dynamic> gqlResponseCityCode = listOfCities.Select(city => city.cityCode.ToString()).ToList();
                List<dynamic> gqlResponseCityName = listOfCities.Select(code => code.cityName.ToString()).ToList();
                List<dynamic> gqlResponseCityType = listOfCities.Select(city => city.cityType.ToString()).ToList();
                List<dynamic> gqlResponseCountryCode = listOfCities.Select(city => city.countryCode.ToString()).ToList();
                List<dynamic> gqlResponseRegion = listOfCities.Select(city => city.region.ToString()).ToList();
                List<dynamic> gqlResponseTimeZone = listOfCities.Select(city => city.timeZone.ToString()).ToList();

                Assert.NotEmpty(gqlResponseAirportCodes);
                Assert.NotEmpty(gqlResponseAirportName);
                Assert.NotEmpty(gqlResponseAirportFullName);
                Assert.NotEmpty(gqlResponseCityCode);
                Assert.NotEmpty(gqlResponseCityName);
                Assert.NotEmpty(gqlResponseCityType);
                Assert.NotEmpty(gqlResponseCountryCode);
                Assert.NotEmpty(gqlResponseRegion);
                Assert.NotEmpty(gqlResponseTimeZone);

            }
        }

        [Theory]
        [Trait("Category", Traits.AirportData)]
        [InlineData("London%20Heathrow")]
        [InlineData("lhr")]
        [InlineData("New%20York")]
        [InlineData("Lon")]
        [InlineData("London%20all")]
        [InlineData("Manchester")]
        [InlineData("jfk")]
        [InlineData("Orlando")]
        [InlineData("London")]
        [InlineData("new")]
        [InlineData("Los%20Angeles")]
        [InlineData("lond")]
        [InlineData("Atlanta")]
        [InlineData("San%20Francisco")]
        [InlineData("New%20York%20Area%20Airports")]
        [InlineData("Miami")]
        [InlineData("Lhr")]
        [InlineData("Man")]
        [InlineData("Boston")]
        [InlineData("lax")]
        [InlineData("Toronto")]
        public async Task SearchAirportInfo_TopSearchCriteria(string searchCriteria)
        {
            //Amend the Search criteria to work with the GQL request body
            string gqlsearchCriteria = searchCriteria.Replace("%20", " ");

            //GQL calls
            HttpResponseMessage gqlAirportDataResponse = Requests.Endpoints.GraphQLEndpoint.SearchAirportInfo(gqlsearchCriteria).EnsureGQLResponseSuccess();
            dynamic? gqlResult = JsonConvert.DeserializeObject<dynamic>(await gqlAirportDataResponse.Content.ReadAsStringAsync());
            string gqlResultString = gqlResult.data.searchAirportInfo.listOfCities.ToString();
            List<dynamic>? listOfCities = JsonConvert.DeserializeObject<List<dynamic>>(gqlResultString);

            List<dynamic> gqlResponseAirportCodes = listOfCities.Select(city => city.airportCode.ToString()).ToList();
            List<dynamic> gqlResponseAirportName = listOfCities.Select(city => city.airportName.ToString()).ToList();
            List<dynamic> gqlResponseAirportFullName = listOfCities.Select(city => city.airportFullName.ToString()).ToList();
            List<dynamic> gqlResponseCityCode = listOfCities.Select(city => city.cityCode.ToString()).ToList();
            List<dynamic> gqlResponseCityName = listOfCities.Select(code => code.cityName.ToString()).ToList();
            List<dynamic> gqlResponseCityType = listOfCities.Select(city => city.cityType.ToString()).ToList();
            List<dynamic> gqlResponseCountryCode = listOfCities.Select(city => city.countryCode.ToString()).ToList();
            List<dynamic> gqlResponseRegion = listOfCities.Select(city => city.region.ToString()).ToList();
            List<dynamic> gqlResponseTimeZone = listOfCities.Select(city => city.timeZone.ToString()).ToList();

            Assert.NotEmpty(gqlResponseAirportCodes);
            Assert.NotEmpty(gqlResponseAirportName);
            Assert.NotEmpty(gqlResponseAirportFullName);
            Assert.NotEmpty(gqlResponseCityCode);
            Assert.NotEmpty(gqlResponseCityName);
            Assert.NotEmpty(gqlResponseCityType);
            Assert.NotEmpty(gqlResponseCountryCode);
            Assert.NotEmpty(gqlResponseRegion);
            Assert.NotEmpty(gqlResponseTimeZone);

        }
    }
}
