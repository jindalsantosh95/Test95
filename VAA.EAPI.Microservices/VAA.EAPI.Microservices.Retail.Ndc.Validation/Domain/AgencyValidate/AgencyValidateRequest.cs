using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Domain.AgencyValidate
{
    public class AgencyValidateRequest
    {
        public Parties Parties { get; set; }
        public string KeyType { get; set; }
        public bool checkAgentTktAuthority { get; set; }
        public bool OCNValidation { get; set; }
    }
}