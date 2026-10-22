using System;
namespace VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange
{
    public class VoucherDetails
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
