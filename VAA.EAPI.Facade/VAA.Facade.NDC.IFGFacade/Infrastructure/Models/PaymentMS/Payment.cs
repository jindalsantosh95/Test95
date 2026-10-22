using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Payment;

namespace VAA.Facade.NDC.IFGFacade.Infrastructure.Models.PaymentMS
{
    public class Payment : PaymentDetails
    {
        public CurrencyAmount Amount { get; set; }
        public CardInfo CardMethod { get; set; }
        public PaymentPayer Payer { get; set; }
        public PaymentCardAuthentication PaymentCardAuthentication { get; set; }
        public int TransactionTypeId { get; set; }
        public string PaymentTransactionId { get; set; }
    }
}
