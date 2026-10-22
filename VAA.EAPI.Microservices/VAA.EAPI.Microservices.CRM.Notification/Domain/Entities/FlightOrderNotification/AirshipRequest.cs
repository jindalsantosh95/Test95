namespace VAA.EAPI.Microservices.CRM.Notification.Domain.Entities.FlightOrderNotification
{
    public class AirshipRequest
    {
        private const string DefaultGroup = "Trips";
        public IEnumerable<string> PnrsWithDate { get; set; }
        public string FlightNumber { get; set; }
        public string Group { get; set; } = DefaultGroup;
        public string Message { get; set; }
        public string DeepLink { get; set; }
        public Campaigns Campaigns { get; set; }
        public string CustomEventName { get; set; }
    }
}
