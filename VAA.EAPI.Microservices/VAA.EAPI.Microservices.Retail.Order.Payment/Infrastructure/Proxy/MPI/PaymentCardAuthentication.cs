namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.MPI
{
    public class PaymentCardAuthentication
    {
        public string UcafIndicator { get; set; }

        public string Eci { get; set; }

        public string Cavv { get; set; }

        public string CavvAlgorithm { get; set; }

        public string Xid { get; set; }

        public string ServerTransactionId { get; set; }

        public string DsTransactionId { get; set; }

        public string DsAuthenticationVersionText { get; set; }

        public string ValidationStatusText { get; set; }
    }
}
