using VAA.EAPI.GraphQL.Domain.Book.Models.Common;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Payment
{
    public class VoucherMethod
    {
        public string VoucherType { get; set; }
        public string Issuer { get; set; }
        public string VoucherNumber { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public CurrencyAmount Amount { get; set; }
        public int? Percentage { get; set; }
    }
}
