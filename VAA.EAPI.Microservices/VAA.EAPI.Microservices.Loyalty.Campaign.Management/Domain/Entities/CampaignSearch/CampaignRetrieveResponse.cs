namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignSearch
{
    public class CampaignRetrieveResponse
    {
        public CampaignData Campaign { get; set; }
        public int AssociatedMemberCount { get; set; }
    }
}
