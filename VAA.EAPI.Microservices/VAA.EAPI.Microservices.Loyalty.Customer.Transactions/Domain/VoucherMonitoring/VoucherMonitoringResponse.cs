using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.VoucherMonitoring
{
    public sealed class VoucherMonitoringResponse
    {
        public string CustomerId { get; set; }
        public string MemberNumber { get; set; }
        public IEnumerable<VoucherResult> Vouchers { get; set; }
    }
}

