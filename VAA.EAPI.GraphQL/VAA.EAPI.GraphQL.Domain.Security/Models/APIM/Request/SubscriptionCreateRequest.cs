using VAA.EAPI.GraphQL.Domain.Security.Models.APIM.BaseClasses;

namespace VAA.EAPI.GraphQL.Domain.Security.Models.APIM.Request
{
    public class SubscriptionCreateRequest : SubscriptionCreateRequestBase
    {
        public string SubscriptionId { get; set; } = null!;
        public string SubscriptionName { get; set; } = null!;
    }
}
