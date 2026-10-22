namespace VAA.EAPI.Microservices.Security.Infrastructure.Models.APIMSubscriptionResponse
{
    public class APIMSubscriptionResponse
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public SubscriptionProperties Properties { get; set; }
    }
}
