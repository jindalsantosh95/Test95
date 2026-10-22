using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Domain.Order.Remarks
{
    public class VoucherRemarkDetails
    {
        public string LoyaltyNumber { get; set; }
        public string RedemptionType { get; set; }
        public long TotalPoints { get; set; }
        public IEnumerable<VoucherRemark> Vouchers { get; set; }
    }
}
