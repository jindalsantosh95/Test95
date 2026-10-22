
namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS.Payment
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

        public static explicit operator PaymentCardAuthorization(PaymentMS.PaymentDeails.PaymentCardAuthorization source)
        {
            if (source == null) return null;

            return new PaymentCardAuthorization
            {
                ApprovalCode = source.ApprovalCode,
                PaymentReferenceId = source.PaymentReferenceId,
                VendorPaymentTransactionId = source.VendorPaymentTransactionId,
                ParentPaymentReferenceId = source.ParentPaymentReferenceId,
                VendorPaymentReferenceId = source.VendorPaymentReferenceId,
                ResponseCode = source.ResponseCode,
                SourceName = source.SourceName,
                AcquirerReferenceDataText = source.AcquirerReferenceDataText,
                TransactionCurrencyCode = source.TransactionCurrencyCode,
                StoredCredentialIndicator = source.StoredCredentialIndicator
            };
        }
    }
}