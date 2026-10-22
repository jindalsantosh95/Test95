using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD
{
    public class RefundPayment : Payment
    {
        public string PaymentTransactionType { get; set; }
        public RefundAmount RefundAmount { get; set; }
        public string CardType { get; set; }
        public string RefundStatus { get; set; }
    }
}
