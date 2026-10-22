using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models;

public class PromotionCampaignSearchData
{
    [GraphQLDescription("The List of Campaings")]
    public string CampaignCode { get; set; }
    public string CampaignName { get; set; }
    public string Status { get; set; }
    public string StartDate { get; set; }
    public string EndDate { get; set; }
    public string Type { get; set; }

}