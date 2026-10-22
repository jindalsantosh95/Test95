using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Requests;
using VAA.EAPI.Automation.FlightOperations.Constants;

namespace VAA.EAPI.Automation.FlightOperations.Tests.Microservice
{
    public class FlightOperationsTests
    {
        public static string ocpSubKeyFlightOps = Vault.Default.GetSecretValue("FlightOperations-Subscription-Automation-Key");
        public static string ocpSubKeyCrew = Vault.Default.GetSecretValue("Crew-Subscription-Automation-Key");

        [Trait("Category", Traits.FlightOperationsMicroserviceTests)]
        [Fact]
        public async Task FlightDetailsSchemaValidation()
        {
            var fromDate = DateTime.Now.Date.AddMinutes(-30).ToString("yyyy-MM-ddTHH:mm:ss");

            var toDate = DateTime.Now.Date.AddDays(1).AddMilliseconds(-3).ToString("yyyy-MM-ddTHH:mm:ss");

            var flightDetailsRequest = MicroserviceRequestBuilder.FlightDetailsMsRequest(fromDate, toDate);

            var schmeaResponse = Endpoints.GetSchema("fltopsflights", ocpSubKeyFlightOps).EnsureIsSuccessStatusCode().ReadResponseAsString();

            HttpResponseMessage flightDetailsResponse = Requests.Endpoints.Microservice.FlightDetails(flightDetailsRequest, ocpSubKeyFlightOps).EnsureIsSuccessStatusCode();

            var content = await flightDetailsResponse.Content.ReadAsStringAsync();

            string responseCodeReturned = ((int)flightDetailsResponse.StatusCode).ToString();

            string schemaName = "/detail";

            SchemaValidator.JsonSchemaValidator(content, schmeaResponse, schemaName, responseCodeReturned);
        }

        [Trait("Category", Traits.FlightOperationsMicroserviceTests)]
        [Fact]
        public async Task FlightDetailsConsolidated()
        {
            var fromDate = DateTime.Now.Date.AddMinutes(-30).ToString("yyyy-MM-ddTHH:mm:ss");

            var toDate = DateTime.Now.Date.AddDays(1).AddMilliseconds(-3).ToString("yyyy-MM-ddTHH:mm:ss");

            var flightDetailsRequest = MicroserviceRequestBuilder.FlightDetailsMsRequest(fromDate, toDate);

            HttpResponseMessage flightDetailsResponse = Requests.Endpoints.Microservice.FlightDetails(flightDetailsRequest, ocpSubKeyFlightOps).EnsureIsSuccessStatusCode();

            var content = await flightDetailsResponse.Content.ReadAsStringAsync();

            var flightSchedule = JArray.Parse(content);

            foreach (var flight in flightSchedule)
            {
                Assert.NotEmpty(flight["flightNo"]?.ToString());
                Assert.NotEmpty(flight["carrier"]?.ToString());
                Assert.NotEmpty(flight["actualDateOfArrival"]?.ToString());
                Assert.NotEmpty(flight["actualDateOfDeparture"]?.ToString());
                Assert.NotEmpty(flight["arrival"]?.ToString());
                Assert.NotEmpty(flight["flightNo"]?.ToString());
                Assert.NotEmpty(flight["scheduledTimeOfArrival"]?.ToString());
                Assert.NotEmpty(flight["scheduledTimeOfDeparture"]?.ToString());
                Assert.NotEmpty(flight["estimatedDateOfArrival"]?.ToString());
                Assert.NotEmpty(flight["estimatedDateOfDeparture"]?.ToString());
            }
        }

        [Trait("Category", Traits.FlightOperationsMicroserviceTests)]
        [Fact]
        public async Task FlightScheduleEcom()
        {
            var fromDate = DateTime.Now.Date.AddMinutes(-30).ToString("yyyy-MM-ddTHH:mm:ss");

            var toDate = DateTime.Now.Date.AddDays(1).AddMilliseconds(-3).ToString("yyyy-MM-ddTHH:mm:ss");

            var flightScheduleRequest = MicroserviceRequestBuilder.FlightDetailsMsRequest(fromDate, toDate);

            HttpResponseMessage flightScheduleResponse = Requests.Endpoints.Microservice.FlightDetails(flightScheduleRequest, ocpSubKeyFlightOps).EnsureIsSuccessStatusCode();

            var content = await flightScheduleResponse.Content.ReadAsStringAsync();

            var flightSchedule = JArray.Parse(content);

            foreach (var flight in flightSchedule)
            {
                Assert.NotEmpty(flight["aircraftType"]?.ToString());
                Assert.NotEmpty(flight["actualDateOfArrival"]?.ToString());
                Assert.NotEmpty(flight["actualDateOfDeparture"]?.ToString());
                Assert.NotEmpty(flight["arrival"]?.ToString());
                Assert.NotEmpty(flight["carrier"]?.ToString());
                Assert.NotEmpty(flight["flightNo"]?.ToString());
                Assert.NotEmpty(flight["scheduledTimeOfArrival"]?.ToString());
                Assert.NotEmpty(flight["scheduledTimeOfDeparture"]?.ToString());
                Assert.NotEmpty(flight["estimatedDateOfArrival"]?.ToString());
                Assert.NotEmpty(flight["estimatedDateOfDeparture"]?.ToString());
                Assert.NotEmpty(flight["description"]?.ToString());
            }
        }

        [Trait("Category", Traits.FlightOperationsMicroserviceTests)]
        [Fact]
        public async Task FlightScheduleForCrewRosterDetails()
        {
            var fromDate = DateTime.Now.Date.AddMinutes(-30).ToString("yyyy-MM-ddTHH:mm:ss");

            var toDate = DateTime.Now.Date.AddDays(1).AddMilliseconds(-3).ToString("yyyy-MM-ddTHH:mm:ss");

            var flightScheduleRequest = MicroserviceRequestBuilder.FlightScheduleMsRequest(fromDate, toDate);

            HttpResponseMessage flightScheduleResponse = Requests.Endpoints.Microservice.FlightSchedule(flightScheduleRequest, ocpSubKeyFlightOps).EnsureIsSuccessStatusCode();

            var content = await flightScheduleResponse.Content.ReadAsStringAsync();

            var flightSchedule = JArray.Parse(content);

            foreach (var flight in flightSchedule)
            {
                Assert.NotEmpty(flight["flightNo"]?.ToString());
                Assert.NotEmpty(flight["scheduledTimeOfDeparture"]?.ToString());
                Assert.NotEmpty(flight["actualDateOfDeparture"]?.ToString());
            }
        }

        [Trait("Category", Traits.FlightOperationsMicroserviceTests)]
        [Fact]
        public async Task FlightCrewRosterSchemaValidation()
        {
            var flightDate = DateTime.Now.ToString("yyyy-MM-dd");

            var departureAirportCode = "LHR";

            int flightNo = 137;

            var flightCrewRosterRequest = MicroserviceRequestBuilder.FlightCrewMsRosterRequest(flightDate, departureAirportCode, flightNo);

            HttpResponseMessage flightCrewRosterResponse = Requests.Endpoints.Microservice.FlightCrew(flightCrewRosterRequest, ocpSubKeyFlightOps).EnsureIsSuccessStatusCode();

            var content = await flightCrewRosterResponse.Content.ReadAsStringAsync();

            var schmeaResponse = Endpoints.GetSchema("fltopsflights", ocpSubKeyFlightOps).EnsureIsSuccessStatusCode().ReadResponseAsString();

            string responseCodeReturned = ((int)flightCrewRosterResponse.StatusCode).ToString();

            string schemaName = "/crewroster";

            SchemaValidator.JsonSchemaValidator(content, schmeaResponse, schemaName, responseCodeReturned);
        }

        [Trait("Category", Traits.FlightOperationsMicroserviceTests)]
        [Fact]
        public async Task FlightCrewRoster()
        {
            var flightDate = DateTime.Now.ToString("yyyy-MM-dd");

            var departureAirportCode = "LHR";

            int flightNo = 137;

            var flightCrewRosterRequest = MicroserviceRequestBuilder.FlightCrewMsRosterRequest(flightDate, departureAirportCode, flightNo);

            HttpResponseMessage flightCrewRosterResponse = Requests.Endpoints.Microservice.FlightCrew(flightCrewRosterRequest, ocpSubKeyFlightOps).EnsureIsSuccessStatusCode();

            var content = await flightCrewRosterResponse.Content.ReadAsStringAsync();

            var crewRoster = JArray.Parse(content);

            foreach (var crewMember in crewRoster)
            {
                Assert.NotEmpty(crewMember["id"]?.ToString());
                Assert.NotEmpty(crewMember["base"]?.ToString());
                Assert.NotEmpty(crewMember["firstName"]?.ToString());
                Assert.NotEmpty(crewMember["lastName"]?.ToString());
                Assert.NotEmpty(crewMember["name"]?.ToString());
                Assert.NotEmpty(crewMember["pos"]?.ToString());
            }
        }
    }
}