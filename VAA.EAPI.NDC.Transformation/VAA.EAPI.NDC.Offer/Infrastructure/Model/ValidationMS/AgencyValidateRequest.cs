using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.ValidationMS
{
    public class AgencyValidateRequest
    {
        public string AgentSubscriptionKey { get; set; }
        public string Environment { get; set; }
        public Parties Parties { get; set; }
        public string Keytype { get; set; }
        public bool CheckAgentTktAuthority { get; set; }
    }
}
