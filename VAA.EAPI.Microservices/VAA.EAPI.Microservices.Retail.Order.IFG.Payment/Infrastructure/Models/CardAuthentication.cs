namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Models
{
    public class CardAuthentication
    {
        public string AuthenticationValue { get; set; }
        public string ServerTransactionId { get; set; }
        public string EcomInd { get; set; }
    }
}
