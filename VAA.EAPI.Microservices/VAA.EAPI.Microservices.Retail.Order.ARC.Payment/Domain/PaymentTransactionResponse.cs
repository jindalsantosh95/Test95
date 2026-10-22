using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Retail.Order.ARC.Payment.Domain
{
    public class PaymentTransactionResponse
    {
        public string RequestId { get; set; }
        public string Status { get; set; }
        public DateTime TimeStamp { get; set; }
        public IEnumerable<ErrorDetails> errorDetails { get; set; }
    }
}
