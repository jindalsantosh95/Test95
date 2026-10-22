namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.Air4Payment
{
    public class PaymentCard
    {
        public BillingAddress BillingAdr { get; set; }
        public CardHolderName CardHolderName { get; set; }
        public string ExpirationMonthNum { get; set; }
        public string ExpirationYearNum { get; set; }
        public string CardVerificationValueNum { get; set; }
        public string PaymentCardNetworkName { get; set; }
        public string PaymentCardNum { get; set; }
        public string PaymentCardLast4Num { get; set; }
        public string PaymentCardTypeCode { get; set; }
    }

}