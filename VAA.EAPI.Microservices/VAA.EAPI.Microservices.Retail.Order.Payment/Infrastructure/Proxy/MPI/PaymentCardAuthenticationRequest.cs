namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.MPI
{

    public class PaymentCardAuthenticationRequest
    {
        public FormOfPayment FormOfPayment { get; set; }

        public string DFReferenceId { get; set; }

        public string EmailAdr { get; set; }

        public string PhoneNum { get; set; }

        public string LoyaltyMemberId { get; set; }

    }
}
