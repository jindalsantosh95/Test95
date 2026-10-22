using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.VoucherMonitoring
{
    public class VoucherMonitoringRequest
    {
        public string CustomerId { get; set; }
        public string MemberNumber { get; set; }
        public IEnumerable<string> VoucherNumbers { get; set; }
    }
}

