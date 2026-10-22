using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class CampaignUnassignMutationRequest
    {
        [GraphQLDescription("CustomerId of the FC member")]
        public string CustomerId { get; set; }

        [GraphQLDescription("Targeted Membeship Document ID")]
        public string DocumentId { get; set; }
    }
}
