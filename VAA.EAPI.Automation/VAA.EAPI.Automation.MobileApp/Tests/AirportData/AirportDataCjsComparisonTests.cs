using Newtonsoft.Json;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit;
using VAA.EAPI.Automation.MobileApp.Helpers;

namespace VAA.EAPI.Automation.MobileApp.Tests.AirportData
{
    public class AirportDataCjsComparisonTests
    {
        [Fact]
        public async Task SearchAirportInfo_AirportCodes()
        {
            string airportDataPath = Path.Combine(AppContext.BaseDirectory, "Data", "AirportData.json");
            string jsonData = File.ReadAllText(airportDataPath);

            ReturnAirportCodes airportCodeBuilder = new ReturnAirportCodes();

            List<string> allAirportCodes = airportCodeBuilder.ReturnRandomSpecifiedAmountOfAirportCodes(jsonData, 20);

            //Call for all airport codes
            //List<string> allAirportCodes = airportCodeBuilder.ReturnAllAirportCodes(jsonData);

            foreach (string airportCode in allAirportCodes)
            {
                //CJS calls
                HttpResponseMessage cjsAirportDataResponse = Requests.Endpoints.CjsAirportEndpoint.AirportData(airportCode).EnsureIsSuccessStatusCode();
                List<dynamic>? cjsResult = JsonConvert.DeserializeObject<List<dynamic>>(await cjsAirportDataResponse.Content.ReadAsStringAsync());

                List<dynamic> cjsResponseAirportCodes = cjsResult.Select(code => code.airportCode.ToString()).ToList();
                List<dynamic> cjsResponseAirportName = cjsResult.Select(code => code.airportName.ToString()).ToList();
                List<dynamic> cjsResponseCityCode = cjsResult.Select(code => code.cityCode.ToString()).ToList();
                List<dynamic> cjsResponseCityName = cjsResult.Select(code => code.cityName.ToString()).ToList();
                List<dynamic> cjsResponseCityServe = cjsResult.Select(code => code.cityServe.ToString()).ToList();
                List<dynamic> cjsResponseCityType = cjsResult.Select(code => code.cityType.ToString()).ToList();
                List<dynamic> cjsResponseCountryCode = cjsResult.Select(code => code.countryCode.ToString()).ToList();
                List<dynamic> cjsResponseRegion = cjsResult.Select(code => code.region.ToString()).ToList();
                List<dynamic> cjsResponseTimeZone = cjsResult.Select(code => code.timeZone.ToString()).ToList(); //This always returns empty in CJS

                //GQL calls
                HttpResponseMessage gqlAirportDataResponse = Requests.Endpoints.GraphQLEndpoint.SearchAirportInfo(airportCode).EnsureGQLResponseSuccess();
                dynamic? gqlResult = JsonConvert.DeserializeObject<dynamic>(await gqlAirportDataResponse.Content.ReadAsStringAsync());
                string gqlResultString = gqlResult.data.searchAirportInfo.listOfCities.ToString();
                List<dynamic>? listOfCities = JsonConvert.DeserializeObject<List<dynamic>>(gqlResultString);

                List<dynamic> gqlResponseAirportCodes = listOfCities.Select(city => city.airportCode.ToString()).ToList();
                List<dynamic> gqlResponseAirportName = listOfCities.Select(city => city.airportName.ToString()).ToList();
                List<dynamic> gqlResponseCityCode = listOfCities.Select(city => city.cityCode.ToString()).ToList();
                List<dynamic> gqlResponseCityName = listOfCities.Select(city => city.cityName.ToString()).ToList();
                //Not in our GQL data. Will be added but test can be PR'd whilst this is done
                //var gqlResponseCityServe = listOfCities.Select(city => city.cityServe.ToString()).ToList();
                List<dynamic> gqlResponseCityType = listOfCities.Select(city => city.cityType.ToString()).ToList();
                List<dynamic> gqlResponseCountryCode = listOfCities.Select(city => city.countryCode.ToString()).ToList();
                List<dynamic> gqlResponseRegion = listOfCities.Select(city => city.region.ToString()).ToList();
                List<dynamic> gqlResponseTimeZone = listOfCities.Select(city => city.timeZone.ToString()).ToList();

                SoftAssertHelper softAssert = new SoftAssertHelper();

                softAssert.True(cjsResponseAirportCodes.Count == gqlResponseAirportCodes.Count, $"Counts did not match for search {airportCode} CJS count = {cjsResponseAirportCodes.Count} and GQL count = {gqlResponseAirportCodes.Count}");

                softAssert.True(cjsResponseAirportCodes.SequenceEqual(gqlResponseAirportCodes), $"The Airport Codes returned did not match for search {airportCode} {Environment.NewLine} Airport Codes CJS: {string.Join(",", cjsResponseAirportCodes)} {Environment.NewLine} Airport Codes GQL: {string.Join(",", gqlResponseAirportCodes)}");

                softAssert.True(cjsResponseAirportName.SequenceEqual(gqlResponseAirportName), $"The Airport Names returned did not match for search {airportCode}{Environment.NewLine} Airport Names CJS: {string.Join(",", cjsResponseAirportName)} {Environment.NewLine} Airport Names GQL: {string.Join(",", gqlResponseAirportName)}");

                softAssert.True(cjsResponseCityCode.SequenceEqual(gqlResponseCityCode), $"The City Codes returned did not match for search {airportCode}{Environment.NewLine} City Codes CJS: {string.Join(",", cjsResponseCityCode)} {Environment.NewLine} City Codes GQL: {string.Join(",", gqlResponseCityCode)}");

                softAssert.True(cjsResponseCityName.SequenceEqual(gqlResponseCityName), $"The City Names returned did not match for search {airportCode}{Environment.NewLine} City Names CJS: {string.Join(",", cjsResponseCityName)} {Environment.NewLine} City Names GQL: {string.Join(",", gqlResponseCityName)}");

                softAssert.True(cjsResponseCountryCode.SequenceEqual(gqlResponseCountryCode), $"The County Codes returned did not match for search {airportCode}{Environment.NewLine} County Codes CJS: {string.Join(",", cjsResponseCountryCode)} {Environment.NewLine} County Codes GQL: {string.Join(",", gqlResponseCountryCode)}");

                softAssert.True(cjsResponseRegion.SequenceEqual(gqlResponseRegion), $"The Regions returned did not match for search {airportCode}{Environment.NewLine} Regions CJS: {string.Join(",", cjsResponseRegion)} {Environment.NewLine} Regions GQL: {string.Join(",", gqlResponseRegion)}");

                softAssert.AssertAll();
            }
        }

        [Theory]
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
            //CJS calls
            HttpResponseMessage cjsAirportDataResponse = Requests.Endpoints.CjsAirportEndpoint.AirportData(searchCriteria).EnsureIsSuccessStatusCode();
            List<dynamic>? cjsResult = JsonConvert.DeserializeObject<List<dynamic>>(await cjsAirportDataResponse.Content.ReadAsStringAsync());

            List<dynamic> cjsResponseAirportCodes = cjsResult.Select(code => code.airportCode.ToString()).ToList();
            List<dynamic> cjsResponseAirportName = cjsResult.Select(code => code.airportName.ToString()).ToList();
            List<dynamic> cjsResponseCityCode = cjsResult.Select(code => code.cityCode.ToString()).ToList();
            List<dynamic> cjsResponseCityName = cjsResult.Select(code => code.cityName.ToString()).ToList();
            List<dynamic> cjsResponseCityServe = cjsResult.Select(code => code.cityServe.ToString()).ToList();
            List<dynamic> cjsResponseCityType = cjsResult.Select(code => code.cityType.ToString()).ToList();
            List<dynamic> cjsResponseCountryCode = cjsResult.Select(code => code.countryCode.ToString()).ToList();
            List<dynamic> cjsResponseRegion = cjsResult.Select(code => code.region.ToString()).ToList();

            //Amend the Search criteria to work with the GQL request body
            string gqlsearchCriteria = searchCriteria.Replace("%20", " ");

            //GQL calls
            HttpResponseMessage gqlAirportDataResponse = Requests.Endpoints.GraphQLEndpoint.SearchAirportInfo(gqlsearchCriteria).EnsureGQLResponseSuccess();
            dynamic? gqlResult = JsonConvert.DeserializeObject<dynamic>(await gqlAirportDataResponse.Content.ReadAsStringAsync());
            string gqlResultString = gqlResult.data.searchAirportInfo.listOfCities.ToString();
            List<dynamic>? listOfCities = JsonConvert.DeserializeObject<List<dynamic>>(gqlResultString);

            List<dynamic> gqlResponseAirportCodes = listOfCities.Select(city => city.airportCode.ToString()).ToList();
            List<dynamic> gqlResponseAirportName = listOfCities.Select(city => city.airportName.ToString()).ToList();
            List<dynamic> gqlResponseCityCode = listOfCities.Select(city => city.cityCode.ToString()).ToList();
            List<dynamic> gqlResponseCityName = listOfCities.Select(code => code.cityName.ToString()).ToList();
            List<dynamic> gqlResponseCityType = listOfCities.Select(city => city.cityType.ToString()).ToList();
            List<dynamic> gqlResponseCountryCode = listOfCities.Select(city => city.countryCode.ToString()).ToList();
            List<dynamic> gqlResponseRegion = listOfCities.Select(city => city.region.ToString()).ToList();

            SoftAssertHelper softAssert = new SoftAssertHelper();

            softAssert.True(cjsResponseAirportCodes.Count == gqlResponseAirportCodes.Count, $"Counts did not match CJS count = {cjsResponseAirportCodes.Count} and GQL count = {gqlResponseAirportCodes.Count}: Search Criteria = {searchCriteria}");

            softAssert.True(cjsResponseAirportCodes.SequenceEqual(gqlResponseAirportCodes), $"The Airport Codes returned did not match {Environment.NewLine} Airport Codes CJS: {string.Join(",", cjsResponseAirportCodes)} {Environment.NewLine} Airport Codes GQL: {string.Join(",", gqlResponseAirportCodes)}");

            softAssert.True(cjsResponseAirportName.SequenceEqual(gqlResponseAirportName), $"The Airport Names returned did not match {Environment.NewLine} Airport Names CJS: {string.Join(",", cjsResponseAirportName)} {Environment.NewLine} Airport Names GQL: {string.Join(",", gqlResponseAirportName)}");

            softAssert.True(cjsResponseCityCode.SequenceEqual(gqlResponseCityCode), $"The City Codes returned did not match {Environment.NewLine} City Codes CJS: {string.Join(",", cjsResponseCityCode)} {Environment.NewLine} City Codes GQL: {string.Join(",", gqlResponseCityCode)}");

            softAssert.True(cjsResponseCityName.SequenceEqual(gqlResponseCityName), $"The City Names returned did not match {Environment.NewLine} City Names CJS: {string.Join(",", cjsResponseCityName)} {Environment.NewLine} City Names GQL: {string.Join(",", gqlResponseCityName)}");

            softAssert.True(cjsResponseCountryCode.SequenceEqual(gqlResponseCountryCode), $"The County Codes returned did not match {Environment.NewLine} County Codes CJS: {string.Join(",", cjsResponseCountryCode)} {Environment.NewLine} County Codes GQL: {string.Join(",", gqlResponseCountryCode)}");

            softAssert.True(cjsResponseRegion.SequenceEqual(gqlResponseRegion), $"The Regions returned did not match {Environment.NewLine} Regions CJS: {string.Join(",", cjsResponseRegion)} {Environment.NewLine} Regions GQL: {string.Join(",", gqlResponseRegion)}");

            softAssert.AssertAll();
        }
    }
}
