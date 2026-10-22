namespace VAA.EAPI.GraphQL.Domain.Security.Models.APIM.BaseClasses
{
    public class SubscriptionRequestBase
    {
        public string Channel { get; set; } = null!;
        public string Environment { get; set; } = null!;
    }
}
