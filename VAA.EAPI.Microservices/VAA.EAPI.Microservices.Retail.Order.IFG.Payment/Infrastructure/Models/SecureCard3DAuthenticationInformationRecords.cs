namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Models
{
    public class SecureCard3DAuthenticationInformationRecords
    {
        public int cardAuthenticationSequenceNumber { get; set; }
        public string secureCard3DAuthenticationInformation { get; set; }
        public Securecard3dauthenticationinformationdetail secureCard3DAuthenticationInformationDetail { get; set; }
    }
}