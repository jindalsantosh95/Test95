using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.FlyingClubMember
{
    public class FlyingClubMemberPatchEvent
    {
        public FlyingClubMember FlyingClubMemberDocument { get; set; }
        public string CustomerId { get; set; }
    }
}
