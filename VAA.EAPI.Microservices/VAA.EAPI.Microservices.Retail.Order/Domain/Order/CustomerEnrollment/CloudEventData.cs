namespace VAA.EAPI.Microservices.Retail.Order.Domain.Order.CustomerEnrollment
{
    public class CloudEventData
    {
        public CustomerDetails CustomerDetails { get; set; }
        public string ChannelUserId { get; set; }
    }
}
