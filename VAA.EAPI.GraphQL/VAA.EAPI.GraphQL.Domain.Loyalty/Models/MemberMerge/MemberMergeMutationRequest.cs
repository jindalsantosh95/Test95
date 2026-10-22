using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.MemberMerge
{
    public class MemberMergeMutationRequest
    {
        [GraphQLDescription("Merged Customer Id")]
        public string MergedCustomerId { get; set; }
        [GraphQLDescription("Merged Member Number")]
        public string MergedMemberNumber { get; set; }
        public string SurvivorMemberNumber { get; set; }
        public string SurvivorCustomerId { get; set; }
        public string InitiatedBy { get; set; }
    }
}
