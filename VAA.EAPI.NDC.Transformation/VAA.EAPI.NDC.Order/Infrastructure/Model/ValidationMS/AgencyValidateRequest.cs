namespace VAA.EAPI.NDC.Order.Infrastructure.Model.ValidationMS
{
    public class AgencyValidateRequest
    {
        public string AgentSubscriptionKey { get; set; }
        public string Environment { get; set; }
        public Parties Parties { get; set; }
        public string Keytype { get; set; }
        public bool CheckAgentTktAuthority { get; set; }
        public bool OCNValidation { get; set; }
    }
}
