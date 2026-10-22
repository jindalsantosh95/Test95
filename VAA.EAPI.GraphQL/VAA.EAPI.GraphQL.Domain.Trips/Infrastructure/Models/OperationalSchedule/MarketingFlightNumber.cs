using Newtonsoft.Json;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.OperationalSchedule
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
