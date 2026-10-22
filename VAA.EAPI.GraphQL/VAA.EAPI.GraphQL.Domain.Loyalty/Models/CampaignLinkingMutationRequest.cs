using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class CampaignLinkingMutationRequest
    {
        [GraphQLDescription("CampaignId")]
        public string CampaignId { get; set; }
        [GraphQLDescription("PromotionId")]
        public string PromotionId { get; set; }
        [GraphQLDescription("CreatedBy")]
        public string CreatedBy { get; set; }
    }
}
