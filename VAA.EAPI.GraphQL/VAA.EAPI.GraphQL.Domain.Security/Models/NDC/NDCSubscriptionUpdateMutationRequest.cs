using VAA.EAPI.GraphQL.Domain.Security.Models.APIM.BaseClasses;

namespace VAA.EAPI.GraphQL.Domain.Security.Models.NDC
{
    public class NDCSubscriptionUpdateMutationRequest : SubscriptionUpdateRequestBase
    {
        public PartyDetails Party { get; set; } = null!;

    }
}
