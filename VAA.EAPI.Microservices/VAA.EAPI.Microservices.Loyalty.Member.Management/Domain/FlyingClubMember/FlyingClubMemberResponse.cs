using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FlyingClubMember
{
    public class FlyingClubMemberResponse
    {
        public IEnumerable<FlyingClubMember> FlyingClubMemberDocuments { get; set; }
        public string CustomerId { get; set; }
    }
}
