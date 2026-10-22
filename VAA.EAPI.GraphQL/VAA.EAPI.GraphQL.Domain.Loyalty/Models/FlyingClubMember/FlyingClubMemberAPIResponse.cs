using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.FlyingClubMember
{
    public class FlyingClubMemberAPIResponse
    {
        public IEnumerable<FlyingClubMember> FlyingClubMemberDocuments { get; set; }
    }
}