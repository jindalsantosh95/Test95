using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class PromotionUpdateMutationRequest
    {
        [GraphQLDescription("The Promotion ID")]
        public string PromotionId { get; set; }
        [GraphQLDescription("Promotion Name")]
        public string PromotionName { get; set; }
        [GraphQLDescription("Promotion Start Date")]
        public string StartDate { get; set; }
        [GraphQLDescription("Promotion End Date")]
        public string EndDate { get; set; }
        [GraphQLDescription("Promotion Status")]
        public string Active { get; set; }
        [GraphQLDescription("Partner name")]
        public string PartnerName { get; set; }
        [GraphQLDescription("Campaign Linked Status")]
        public string CampaignLinked { get; set; }
        [GraphQLDescription("User who initiated Promotion update")]
        public string InitiatedBy { get; set; }
    }
}
