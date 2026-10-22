namespace VAA.EAPI.Microservices.Retail.Trip.Domain.MessageDetails
{

    public class NotificationMessage
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public NotificationCodeType Type { get; set; }
    }
}
