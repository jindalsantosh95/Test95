namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.MVT
{
    public class MVTMessageRequest
    {
        public string FlightCarrier { get; set; } = string.Empty;
        public int FlightNumber { get; set; }
        public string DepartureAirport { get; set; } = string.Empty;
        public string ArrivalAirport { get; set; } = string.Empty;
        public string DepartureDate { get; set; } = string.Empty;
    }
}