namespace VAA.EAPI.GraphQL.Domain.Account.Models.Campaign
{
    public class AssignCampaignRequest
    {
        public string CustomerId { get; set; }
        public string MemberNumber { get; set; }
        public string CampaignCode { get; set; }
        public string CampaignId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Title { get; set; }
        public string Email { get; set; }
        public string CreatedBy { get; set; }
    }
}
