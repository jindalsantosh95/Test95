using VAA.EAPI.Automation.Core.Constants;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Heimdall.Requests.Endpoints;

namespace VAA.EAPI.Automation.Heimdall.Tests.HealthChecks.VirginHolidays
{
    public class EieSeatsV8
    {
        [Trait("Category", GlobalTraits.HolidaysHealthCheck)]
        [Trait("Tag", GlobalTraits.HolidaysTag)]
        [Theory]
        [InlineData("EIESeatV8Url887")]
        [InlineData("EIESeatV8Url888")]
        [InlineData("EIESeatV8Url889")]
        [InlineData("EIESeatV8Url890")]
        public static void EieHolidaysSeatsV8(string endpoint)
        {
            string DepartureDate = DateTime.Now.AddMonths(1).ToString("yyyy-MM-dd");

            var response = EieEndpoints.SeatV8Facade(DepartureDate, endpoint);

            if (response.ReadResponseAsString().Has("10000 ~ AL CD/FLT/BRD/OFF SKD CHG NOOP")
                || response.ReadResponseAsString().Has("10000 ~ AL CD/FLT/BRD/OFF NOOP")
                || response.ReadResponseAsString().Has("10000 ~ CK FLT/DTE"))
                return;

            Assert.True(response.StatusCode == System.Net.HttpStatusCode.OK, $"EIE Seats V8 error. \r\n Endpoint hit was: {ConfigReader.Get(endpoint)} \r\n Response code was not 200 OK, returned {response.StatusCode} \r\n Request payload was {response.ReadRequestAsString()} \r\n Response was {response.ReadResponseAsString()}");
        }
    }
}