namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaPassengerInfo
{
    public class Notification
    {
        public NotificationActionType Action { get; set; }
        public Error Content { get; set; }
    }
}