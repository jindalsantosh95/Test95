using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class Promotion
    {
        [GraphQLDescription("Promotion ID")]
        public string PromotionId { get; set; }
        [GraphQLDescription("Promotion Name")]
        public string PromotionName { get; set; }
        [GraphQLDescription("Start Date")]
        public string StartDate { get; set; }
        [GraphQLDescription("End Date")]
        public string EndDate { get; set; }
        [GraphQLDescription("is Active Flag")]
        public string Active { get; set; }
        [GraphQLDescription("Partner Name")]
        public string PartnerName { get; set; }
        [GraphQLDescription("Partner ID")]
        public string PartnerId { get; set; }
        [GraphQLDescription("Is Campaign Linked flag")]
        public string CampaignLinked { get; set; }
    }
}
