namespace VAA.EAPI.Microservices.Security.Domain.APIMSubscription.BaseClasses
{
    public class SubscriptionCreateRequest
    {
        public string SubscriptionId { get; set; }
        public string SubscriptionName { get; set; }
        public UserAccountDetails UserAccount { get; set; }
    }
}
