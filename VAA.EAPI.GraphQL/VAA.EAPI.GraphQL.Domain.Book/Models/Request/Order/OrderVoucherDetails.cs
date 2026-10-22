namespace VAA.EAPI.GraphQL.Domain.Book.Models.Request.Order
{
    public class OrderVoucherDetails
    {
        public string? VoucherOwnerFcAccount { get; set; }
        public string? RedemptionType { get; set; }
        public IEnumerable<OrderVoucherRedemption> AppliedVouchers { get; set; } = [];
        public long TotalPointsUsed { get; set; }
    }
}
