namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderNotification.Domain
{
    public class AgentDetails
    {
        public string RequestAgencyId { get; set; }
        public string RequestAgencyIdType { get; set; }
        public string RequestOrgName { get; set; }
        public string CountryCode { get; set; }
        public string AccountCode { get; set; }
    }
}