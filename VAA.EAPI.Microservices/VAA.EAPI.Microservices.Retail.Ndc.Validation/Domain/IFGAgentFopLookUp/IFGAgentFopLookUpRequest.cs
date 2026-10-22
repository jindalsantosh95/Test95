namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Domain.IFGAgentFopLookUp
{
    public class IFGAgentFopLookUpRequest
    {       
        public string CountryCode { get; set; }
        public string AgentCode { get; set; }
        public string ClientID { get; set; }

    }
}
