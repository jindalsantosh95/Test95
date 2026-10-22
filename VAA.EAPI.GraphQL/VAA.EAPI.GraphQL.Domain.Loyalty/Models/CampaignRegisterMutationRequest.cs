using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class CampaignRegisterMutationRequest
    {
        [GraphQLDescription("CustomerId of the Fc member")]
        public string CustomerId { get; set; }
        [GraphQLDescription("CampaignCode")]
        public string CampaignCode { get; set; }
        [GraphQLDescription("CampaignId")]
        public string CampaignId { get; set; }
        [GraphQLDescription("FC member")]
        public string MemberNumber { get; set; }
        [GraphQLDescription("targeted membership document id")]
        public string DocumentId { get; set; }
        [GraphQLDescription("CreatedBy ")]
        public string CreatedBy { get; set; }

    }
}

