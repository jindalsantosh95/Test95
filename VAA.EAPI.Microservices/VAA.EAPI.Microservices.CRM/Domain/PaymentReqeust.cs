namespace VAA.EAPI.Microservices.CRM.Domain
{
    public class PaymentRequest
    {
        public string PaymentNumber { get; set; }
        public string BankCountryName { get; set; }
        public string SortCode { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string Iban { get; set; }
        public string PayeeAddress { get; set; }
        public string RoutingCode { get; set; }
        public string IfscCode { get; set; }
        public string AccountHolderName { get; set; }
        public string SwiftCode { get; set; }
        public string ClabeNumber { get; set; }
        public string PaymentId { get; set; }
        public string CountryId { get; set; }
        public string BankCodeNumber { get; set; }
        public string Street { get; set; }
        public string Town { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string CustomerCountryId { get; set; }
        public string PostCode { get; set; }
        public string AccountSuffix { get; set; }
    }
}
