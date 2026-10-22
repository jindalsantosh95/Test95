using VAA.EAPI.GraphQL.Domain.Security.Models.APIM.BaseClasses;

namespace VAA.EAPI.GraphQL.Domain.Security.Models.NDC
{
    public class NDCSubscriptionDeleteMutationRequest : SubscriptionRequestBase
    {
        [GraphQLDescription("Party Details")]
        public PartyDetails Party { get; set; } = null!;

    }
}
