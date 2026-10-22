namespace VAA.EAPI.Microservices.CRM.Notification.Domain.Entities.FlightOrderNotification
{
    public class NotificationRequest
    {
        public string Message { get; set; }
        public string DeepLink { get; set; }
        public Campaigns Campaigns { get; set; }
        public string CustomEventName { get; set; }
        public IEnumerable<FlightOrderData> FlightOrderData { get; set; }     

    }
}
