namespace VAA.EAPI.Microservices.Security.Domain.APIMSubscription.BaseClasses
{
    public class SubscriptionRequestBase
    {
        public string Channel { get; set; }
        public string Environment { get; set; }
    }
}
