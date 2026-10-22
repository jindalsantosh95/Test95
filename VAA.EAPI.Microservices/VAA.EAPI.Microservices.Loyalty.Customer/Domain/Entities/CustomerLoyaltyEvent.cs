namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities
{
    public class CustomerLoyaltyEvent
    {
        public string SpecVersion { get; set; }
        public string EventType { get; set; }
        public string Source { get; set; }
        public string Subject { get; set; }
        public string EventId { get; set; }
        public string TimeStamp { get; set; }
        public string ContentType { get; set; }
        public string DataVersion { get; set; }
        public DataClass Data { get; set; }
    }
}
