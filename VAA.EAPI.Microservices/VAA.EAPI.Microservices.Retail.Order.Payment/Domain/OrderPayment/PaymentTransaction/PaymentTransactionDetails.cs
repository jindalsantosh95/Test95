using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Payment;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment.PaymentTransaction
{
    public class PaymentTransactionDetails
    {
        public string PaymentReferenceId { get; set; }
        public IEnumerable<PaymentDetails> PaymentDetails { get; set; }
        public string parentPaymentReferenceId { get; set; }
    }
}