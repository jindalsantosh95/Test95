using VAA.EAPI.GraphQL.Domain.Security.Models.APIM.BaseClasses;

namespace VAA.EAPI.GraphQL.Domain.Security.Models.APIM.Request
{
    public class SubscriptionRetrieveRequest : SubscriptionRequestBase
    {
        public string SubscriptionId { get; set; } = null!;

    }
}
