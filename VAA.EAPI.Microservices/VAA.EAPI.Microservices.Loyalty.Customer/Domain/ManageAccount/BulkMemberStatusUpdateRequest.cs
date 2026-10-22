using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.ManageAccount
{
    public class BulkMemberStatusUpdateRequest
    {
        public IEnumerable<string> MemberNumber { get; set; }
        public string MemberStatus { get; set; }
    }
}
