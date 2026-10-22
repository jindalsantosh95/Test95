namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.DeltaOrder
{
    public class SeatInformation
    {
        public string PassengerId { get; set; }
        public string FlightLegId { get; set; }
        public SeatLocation SeatLocation { get; set; }
    }
}