using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.FlightDetails
{
    public class OperatingFlightNumber
    {
        [JsonProperty("Airline_Code")]
        public string AirlineCode { get; set; }

        [JsonProperty("Flight_Number")]
        public string FlightNumber { get; set; }

        [JsonProperty("Flight_ID")]
        public string FlightID { get; set; }
    }
}
