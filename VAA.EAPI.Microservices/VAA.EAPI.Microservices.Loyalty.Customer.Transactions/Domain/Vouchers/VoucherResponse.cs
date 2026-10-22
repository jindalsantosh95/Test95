
namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Vouchers
{
    public class VoucherResponse
    {
        public string VoucherName { get; set; }
        public string VoucherNumber { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string StartDate { get; set; }
        public string ExpiryDate { get; set; }
        public string UsedDate { get; set; }
        public string DateGifted { get; set; }
        public string Source { get; set; }
        public int MaxValue { get; set; }
        public string Category { get; set; }
        public bool IsGifted { get; set; }

    }
}
