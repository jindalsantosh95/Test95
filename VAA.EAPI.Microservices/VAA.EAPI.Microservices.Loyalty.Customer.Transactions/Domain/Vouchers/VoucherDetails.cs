namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Vouchers
{
    public class VoucherDetails
    {
        public string VoucherNumber { get; set; }
        public string VoucherName { get; set; }
        public int? PointsToAward { get; set; }
    }
}
