using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.MVT
{
    public class FlightMovementDetails : TableEntity
    {
        public string FlightCarrier { get; set; } = string.Empty;
        public int FlightNumber { get; set; }
        public string DepartureDate { get; set; } = string.Empty;
        public string DepartureAirport { get; set; } = string.Empty;
        public string ArrivalAirport { get; set; } = string.Empty;
    }
}
