using VAA.EAPI.GraphQL.Domain.Security.Models.APIM.Request;

namespace VAA.EAPI.GraphQL.Domain.Security.Models.APIM.BaseClasses
{
    public class SubscriptionCreateRequestBase : SubscriptionRequestBase
    {
        public UserAccountDetails UserAccount { get; set; } = null!;
    }
}
