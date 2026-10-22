using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.Facade.NDC.IFGFacade.Infrastructure.Models.PaymentMS
{
    public class RefundPayment : Payment
    {
        public string PaymentTransactionType { get; set; }
        public RefundAmount RefundAmount { get; set; }
        public string CardType { get; set; }
        public string RefundStatus { get; set; }
    }
}
