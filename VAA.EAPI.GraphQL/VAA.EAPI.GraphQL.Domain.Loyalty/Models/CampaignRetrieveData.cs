namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class CampaignRetrieveData
    {
        public CampaignSearchData Campaign { get; set; }
        public int AssociatedMemberCount { get; set; }
    }
}
