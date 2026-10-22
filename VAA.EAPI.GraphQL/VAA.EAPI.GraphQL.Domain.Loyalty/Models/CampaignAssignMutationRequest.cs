using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class CampaignAssignMutationRequest
    {
        [GraphQLDescription("CustomerId of the Fc member")]
        public string CustomerId { get; set; }

        [GraphQLDescription("MemberNumber")]
        public string MemberNumber { get; set; }

        [GraphQLDescription("CampaignCode")]
        public string CampaignCode { get; set; }

        [GraphQLDescription("CampaignId")]
        public string CampaignId { get; set; }

        [GraphQLDescription("CreatedBy")]
        public string CreatedBy { get; set; }
    }
}
