using VAA.EAPI.Microservices.Retail.Basket.Constants;

namespace VAA.EAPI.Microservices.Retail.Basket.Domain
{
    public class FQTMembership
    {
        public string FQTProgramme { get; set; }
        public string FQTNumber { get; set; }
        public FQTValidationStatus FQTValidityStatus { get; set; } = FQTValidationStatus.Unchecked;
        public string Validation { get; set; }
        public string  FQTTierCode { get; set; }
        public string FQTPoints { get; set; }
    }
}