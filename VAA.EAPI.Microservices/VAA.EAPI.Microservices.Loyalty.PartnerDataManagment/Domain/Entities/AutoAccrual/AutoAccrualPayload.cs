namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.AutoAccrual
{
    public class AutoAccrualPayload
    {
        public string FlightCarrier { get; set; } = string.Empty;
        public string FlightNumber { get; set; } = string.Empty;
        public string DepartureDate { get; set; } = string.Empty;
        public string DepartureAirport { get; set; } = string.Empty;
        public string ArrivalAirport { get; set; } = string.Empty;
    }
}