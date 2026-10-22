namespace VAA.EAPI.Automation.Core.Delta.Models.OrderCreate
{
    public class PaymentCard
    {
        public BillingAdr billingAdr { get; set; }
        public CardHolderName cardHolderName { get; set; }
        public string expirationMonthNum { get; set; }
        public string expirationYearNum { get; set; }
        public string cardVerificationValueNum { get; set; }
        public string paymentCardNetworkCode { get; set; }
        public string paymentCardNum { get; set; }
        public string paymentCardLast4Num { get; set; }
        public string paymentCardTypeCode { get; set; }
    }
}