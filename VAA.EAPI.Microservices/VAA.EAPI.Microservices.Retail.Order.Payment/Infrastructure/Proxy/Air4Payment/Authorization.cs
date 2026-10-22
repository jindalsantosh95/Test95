namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.Air4Payment
{
    public class Authorization
    {
        public string ApprovalCode { get; set; }
        public string ResponseCode { get; set; }
        public string SourceName { get; set; }
        public string AcquirerReferenceDataText { get; set; }
        public string TransactionCurrencyCode { get; set; }
        public string StoredCredentialIndicator { get; set; }
        public string TransactionId { get; set; }
        public string VendorTransactionId { get; set; }
        public string FopCode { get; set; }
    }
}