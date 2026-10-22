using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Domain.Virgin.Models.Payment
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

        public static explicit operator PaymentCardAuthorization(JObject source)
        {
            return new PaymentCardAuthorization()
            {
                ApprovalCode = source["ApprovalCode"]?.ToString(),
                PaymentReferenceId = source["PaymentReferenceId"]?.ToString(),
                VendorPaymentTransactionId = source["VendorPaymentTransactionId"]?.ToString(),
                ParentPaymentReferenceId = source["ParentPaymentReferenceId"]?.ToString(),
                VendorPaymentReferenceId = source["VendorPaymentReferenceId"]?.ToString(),
                ResponseCode = source["ResponseCode"]?.ToString(),
                SourceName = source["SourceName"]?.ToString(),
                AcquirerReferenceDataText = source["AcquirerReferenceDataText"]?.ToString(),
                TransactionCurrencyCode = source["TransactionCurrencyCode"]?.ToString(),
                StoredCredentialIndicator = source["StoredCredentialIndicator"]?.ToString()
            };
        }

    }
}
