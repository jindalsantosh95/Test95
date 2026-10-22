using Newtonsoft.Json;
using VAA.EAPI.Automation.FlightOperations.Models;

namespace VAA.EAPI.Automation.FlightOperations.Tests.Microservice
{
    public class MicroserviceRequestBuilder
    {
        public static string FlightScheduleMsRequest(string fromDate, string toDate)
        {
            FlightSchedule flightScheduleRequest = new()
            {
                From = fromDate,
                To = toDate
            };

            return JsonConvert.SerializeObject(flightScheduleRequest);
        }

        public static string FlightDetailsMsRequest(string fromDate, string toDate)
        {
            FlightDetail flightDetailRequest = new()
            {
                From = fromDate,
                To = toDate
            };

            return JsonConvert.SerializeObject(flightDetailRequest);
        }

        public static string FlightCrewMsRosterRequest(string flightDate, string departure, int flightNo)
        {
            FlightCrew flightCrewRosterRequest = new()
            {
                FlightDate = flightDate,
                Departure = departure,
                FlightNo = flightNo
            };

            return JsonConvert.SerializeObject(flightCrewRosterRequest);
        }
    }
}
