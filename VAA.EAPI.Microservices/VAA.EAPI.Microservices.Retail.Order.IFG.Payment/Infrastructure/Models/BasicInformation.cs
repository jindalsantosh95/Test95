namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Models
{

    public class BasicInformation
    {
        public string Channel { get; set; }
        public string CountryCode { get; set; }
        public string AgentCode { get; set; }
        public bool NonTicketInd { get; set; }
        public string ClientID { get; set; }
        public bool TASF { get; set; }
    }

}
