using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.FlightOperations.Flights.Application.Models.OperationalSchedule
{
    public class MarketingFlightNumber
    {
        [JsonProperty("Airline_Code")]
        public string AirlineCode { get; set; }

        [JsonProperty("Flight_Number")]
        public string FlightNumber { get; set; }
        public string Suffix { get; set; }
    }
}
