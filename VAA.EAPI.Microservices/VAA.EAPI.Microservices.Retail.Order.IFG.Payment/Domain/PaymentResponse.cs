using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Domain
{
    public class PaymentResponse
    {
        public IEnumerable<ErrorDetails> Errors { get; set; }
        public string TransactionId { get; set; }
        public string ApprovalCode { get; set; }
        public double? Amount { get; set; }
        public string OrderID { get; set; }
        public string TransactionStatus { get; set; }
        public string MaskedCardNumber { get; set; }
        public string ExpiryDate { get; set; }
        public string CardType { get; set; }
        public string PaymentCommitedDateTime { get; set; }        
        public string TrackId { get; set; }
        public int HttpStatus { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string PaymentStatus { get; set; }
        public AmountAuthorized AmountAuthorized { get; set; }
    }
}
