using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Reference
{
    public class PartnerDataPayload
    {
        public IEnumerable<Partner> Partners { get; set; }
    }
}
