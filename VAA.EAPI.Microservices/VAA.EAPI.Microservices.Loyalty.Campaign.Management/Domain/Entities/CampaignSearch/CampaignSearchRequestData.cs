namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignSearch
{
    public class CampaignSearchRequestData
    {
        public string CampaignId { get; set; }
        public string CampaignName { get; set; }
        public string StartDateF { get; set; }
        public string EndDateF { get; set; }
        public string StartDateT { get; set; }
        public string EndDateT { get; set; }
        public string CampaignCode { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
    }
}
