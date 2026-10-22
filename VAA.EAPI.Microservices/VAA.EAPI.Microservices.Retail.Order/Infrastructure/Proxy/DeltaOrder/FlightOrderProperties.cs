namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.DeltaOrder
{
    public class FlightOrderProperties
    {
        public string OrderBookingTypeText { get; set; }
        public string OrderTypeText { get; set; }
        public bool LoyaltyAccountPriority { get; set; }
        public bool Refundable { get; set; }
        public bool Changeable { get; set; }
    }
}
