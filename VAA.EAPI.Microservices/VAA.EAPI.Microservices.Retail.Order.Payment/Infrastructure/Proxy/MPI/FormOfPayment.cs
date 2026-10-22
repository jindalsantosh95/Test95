namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.MPI
{
    public class FormOfPayment
    {
        public PaymentMethod PaymentMethod { get; set; }

        public Price Price { get; set; }

        public PaymentCardAuthentication AuthenticationData { get; set; }

        public string PaymentReferenceId { get; set; }

        public string MarketedProductTypeCode { get; set; }
    }
}
