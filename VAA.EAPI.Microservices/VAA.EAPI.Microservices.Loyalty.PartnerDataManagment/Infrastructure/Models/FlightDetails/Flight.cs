using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.FlightDetails
{
    public class Flight
    {
        [JsonProperty("Marketing_Flight_Number")]
        public MarketingFlightNumber MarketingFlightNumber { get; set; }

        [JsonProperty("Operating_Flight_Number")]
        public OperatingFlightNumber OperatingFlightNumber { get; set; }

        public IEnumerable<Leg> Legs { get; set; }
    }
}
