
using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.MemberCardLink
{
    public class PartnerMembershipResponse
    {
        public IEnumerable<PartnerMembership.PartnerMembership> PartnerMembershipDocuments { get; set; }
        public string CustomerId { get; set; } = string.Empty;
    }
}
