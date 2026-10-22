using Newtonsoft.Json;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.OperationalSchedule
{
    public class OperatingFlightNumber
    {
        [JsonProperty("Airline_Code")]
        public string AirlineCode { get; set; }

        [JsonProperty("Flight_Number")]
        public string FlightNumber { get; set; }
    }
}
