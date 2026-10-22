using System.Collections.Generic;
using VAA.EAPI.Domain.Models.B2CGraph;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain
{
    public class MemberInfo
    {
        public string Id { get; set; }
        public string MemberNumber { get; set; }
        public string MemberStatus { get; set; }
        public string MemberEmail { get; set; }
        public List<IdentityType> Identities { get; set; }
    }
}
