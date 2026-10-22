using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class CampaignCreateMutationRequest
    {
        [GraphQLDescription("Campaign Code")]
        public string CampaignCode { get; set; }
        [GraphQLDescription("Campaign Name")]
        public string CampaignName { get; set; }
        [GraphQLDescription("Date that Campaign starts")]
        public string StartDate { get; set; }
        [GraphQLDescription("Date that Campaign ends")]
        public string EndDate { get; set; }
        [GraphQLDescription("Status of Campaign")]
        public string Status { get; set; }
        [GraphQLDescription("Type of Campaign")]
        public string Type { get; set; }
        [GraphQLDescription("Description of Campaign")]
        public string Description { get; set; }
        [GraphQLDescription(" Flag to denote if Registration Required for given campaign")]
        public bool RegistrationRequiredFlag { get; set; }
        [GraphQLDescription("Link To campaign Template")]
        public string LinkToTemplate { get; set; }
        [GraphQLDescription("User who initiated Campaign Create")]
        public string InitiatedBy { get; set; }
    }
}
