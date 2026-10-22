using System;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.MPI
{
    public class VoucherMethod
    {
        public string VoucherType { get; set; }

        public string Issuer { get; set; }

        public string VoucherNumber { get; set; }

        public DateTime IssueDate { get; set; }

        public DateTime ExpiryDate { get; set; }

        public Amount Amount { get; set; }

        public int Percentage { get; set; }
    }
}
