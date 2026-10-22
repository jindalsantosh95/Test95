using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.MembershipChange
{
    public class MembershipChangeResponse
    {
        public IEnumerable<MembershipChange> MembershipChangeDocuments { get; set; }
        public string CustomerId { get; set; }
    }
}
