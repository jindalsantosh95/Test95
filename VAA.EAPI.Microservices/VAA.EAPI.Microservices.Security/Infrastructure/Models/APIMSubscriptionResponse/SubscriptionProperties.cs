namespace VAA.EAPI.Microservices.Security.Infrastructure.Models.APIMSubscriptionResponse
{
    public class SubscriptionProperties
    {
        public string OwnerId { get; set; }
        public SubscriptionUser User { get; set; }
        public string Scope { get; set; }
        public string DisplayName { get; set; }
        public string State { get; set; }
        public string CreatedDate { get; set; }
        public string StartDate { get; set; }
        public string ExpirationDate { get; set; }
        public string EndDate { get; set; }
        public string NotificationDate { get; set; }
        public string PrimaryKey { get; set; }
        public string SecondaryKey { get; set; }
        public string StateComment { get; set; }
    }
}
