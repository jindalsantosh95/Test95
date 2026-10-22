using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Enrolment;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.UpgradeVACMember
{
    public class UpgradeVACMemberRequest
    {
        [GraphQLDescription("Customer Id")]
        public string CustomerId { get; set; }
        [GraphQLDescription("Member Number")]
        public string MemberNumber { get; set; }
        public string InitiatedBy { get; set; }
        public bool? FCNumAutoStampingConsent { get; set; }
        public CustomerDetails CustomerDetails { get; set; }
    }
}
