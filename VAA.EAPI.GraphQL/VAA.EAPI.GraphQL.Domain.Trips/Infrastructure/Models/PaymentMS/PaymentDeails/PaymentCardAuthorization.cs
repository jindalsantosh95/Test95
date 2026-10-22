using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PaymentMS.PaymentDeails
{
    [GraphQLName("GraphqlTripsPaymentCardAuthorization")]
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
