namespace VAA.EAPI.Microservices.Retail.Order.Search.Domain
{
    public class VoucherMethod
    {
        public string VoucherType { get; set; }
        public string Issuer { get; set; }
        public string VoucherNumber { get; set; }
        public string IssueDate { get; set; }
        public string ExpiryDate { get; set; }
        public Amount Amount { get; set; }
        public int Percentage { get; set; }
    }
}
