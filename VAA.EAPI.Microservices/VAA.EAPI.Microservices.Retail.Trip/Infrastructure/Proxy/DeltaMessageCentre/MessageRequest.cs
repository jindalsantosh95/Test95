namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMessageCentre
{
    public class MessageRequest
    {
        public string RecordLocatorId { get; set; }
        public string MessageKey { get; set; }
        public string MessageType { get; set; }
        public string LoyaltyAccountNum { get; set; }
        public Passenger Passenger { get; set; }
    }
}
