
using System;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.InviteHouseHold
{
    public class Audit
    {
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
