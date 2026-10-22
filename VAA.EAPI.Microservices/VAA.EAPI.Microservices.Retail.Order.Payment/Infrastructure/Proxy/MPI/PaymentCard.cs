namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.MPI
{
    public class PaymentCard
    {
        public string ExpirationMonthNum { get; set; }

        public string ExpirationYearNum { get; set; }

        public string CardVerificationValueNum { get; set; }

        public string PaymentCardNum { get; set; }

        public string PaymentCardTypeCode { get; set; }

    }
}
