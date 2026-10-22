using VAA.EAPI.GraphQL.Domain.Security.Models.APIM.BaseClasses;

namespace VAA.EAPI.GraphQL.Domain.Security.Models.APIM.Request
{
    public class SubscriptionUpdateRequest : SubscriptionUpdateRequestBase
    {
        public string SubscriptionId { get; set; } = null!;
    }
}
