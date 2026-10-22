namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignCreate
{
    public class CampaignCreateRequestData
    {
        public string CampaignCode { get; set; }
        public string CampaignName { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public bool RegistrationRequiredFlag { get; set; }
        public string LinkToTemplate { get; set; }
        public string InitiatedBy { get; set; }
    }
}
