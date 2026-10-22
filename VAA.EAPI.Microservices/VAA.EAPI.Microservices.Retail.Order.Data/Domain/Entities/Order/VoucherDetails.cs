using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.Order
{
    public class VoucherDetails
    {
        public string? VoucherOwnerFcAccount { get; set; }
        public string? RedemptionType { get; set; }
        public IEnumerable<VoucherRedemption> AppliedVouchers { get; set; } = [];
        public long TotalPointsUsed { get; set; }
    }
}
