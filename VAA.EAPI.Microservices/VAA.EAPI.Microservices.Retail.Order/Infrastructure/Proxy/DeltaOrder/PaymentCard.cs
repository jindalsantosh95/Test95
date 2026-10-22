namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.DeltaOrder
{
    public class PaymentCard
    {
        public BillingAdr BillingAdr { get; set; }
        public CardHolderName CardHolderName { get; set; }
        public string ExpirationMonthNum { get; set; }
        public string ExpirationYearNum { get; set; }
        public string CardVerificationValueNum { get; set; }
        public string PaymentCardNetworkCode { get; set; }
        public string PaymentCardNum { get; set; }
        public string PaymentCardLast4Num { get; set; }
    }
}
