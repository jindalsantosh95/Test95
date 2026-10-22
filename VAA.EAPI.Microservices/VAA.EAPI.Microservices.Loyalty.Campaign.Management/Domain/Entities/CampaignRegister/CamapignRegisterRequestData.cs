namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRegister
{
    public class CamapignRegisterRequestData
    {
        public string CustomerId { get; set; }
        public string CampaignCode { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DocumentId { get; set; }
        public string CampaignId { get; set; }
        public string MemberNumber { get; set; }
        public string Title { get; set; }
        public string CreatedBy { get; set; }
    }
}
