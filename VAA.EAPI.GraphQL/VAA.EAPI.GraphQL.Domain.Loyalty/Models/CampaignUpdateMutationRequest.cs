using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class CampaignUpdateMutationRequest
    {
        [GraphQLDescription("CampaignId")]
        public string CampaignId { get; set; }
        [GraphQLDescription("Campign Code")]
        public string CampaignCode { get; set; }
        [GraphQLDescription("Campign Name")]
        public string CampaignName { get; set; }
        [GraphQLDescription("Date that Campign starts")]
        public string StartDate { get; set; }
        [GraphQLDescription("Date that Campign ebds")]
        public string EndDate { get; set; }
        [GraphQLDescription("Status of Campign")]
        public string Status { get; set; }
        [GraphQLDescription("Type of Campign")]
        public string Type { get; set; }
        [GraphQLDescription("Description of Campign")]
        public string Description { get; set; }
        public bool RegistrationRequiredFlag { get; set; }
        public string LinkToTemplate { get; set; }
        [GraphQLDescription("User who initiated Campaign update")]
        public string InitiatedBy { get; set; }
    }
}
