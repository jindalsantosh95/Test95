namespace VAA.EAPI.GraphQL.Domain.Book.Models.Payment
{
    public class PaymentCardAuthorization
    {
        public string ApprovalCode { get; set; }
        public string PaymentReferenceId { get; set; }
        public string VendorPaymentTransactionId { get; set; }
        public string ParentPaymentReferenceId { get; set; }
        public string VendorPaymentReferenceId { get; set; }
        public string ResponseCode { get; set; }
        public string SourceName { get; set; }
        public string AcquirerReferenceDataText { get; set; }
        public string TransactionCurrencyCode { get; set; }
        public string StoredCredentialIndicator { get; set; }
    }
}
