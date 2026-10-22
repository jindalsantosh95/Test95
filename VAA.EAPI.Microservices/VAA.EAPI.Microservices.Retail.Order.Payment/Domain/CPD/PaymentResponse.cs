using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD
{
    public class PaymentResponse
    {
        public IEnumerable<ErrorDetails> Errors { get;set; }
        public string TransactionId { get; set; }
        public double? Amount { get; set; }
        public string OrderID { get; set; }
        public string TransactionStatus { get; set; }
        public string MaskedCardNumber { get; set; }
        public string ExpiryDate { get; set; }
        public string CardType { get; set; }
        public string PaymentCommitedDateTime { get; set; }
        public string FormOfPayment { get; set; }
        public string ApprovalCode { get; set; }
    }
}
