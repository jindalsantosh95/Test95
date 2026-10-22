using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class TargetedMembershipResponse
    {
        public IEnumerable<TargetedMembership> TargetedMembershipDocuments { get; set; }
        public TargetedMembershipResponse() { }
    }
}