using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models;

public class PromotionSearchData
{
    [GraphQLDescription("The List of Promotions")]
    public string PromotionId { get; set; }
    public string PromotionName { get; set; }
    public string StartDate { get; set; }
    public string EndDate { get; set; }
    public string Active { get; set; }
    public string PartnerName { get; set; }
    public string PartnerId { get; set; }
    public string CampaignLinked { get; set; }

}