using VAA.EAPI.Microservices.Retail.Ndc.Validation.Domain.AgencyValidate;

namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Domain.AgencyOCNDetails
{
    public class AgencyOCNDetails
    {
        public AgencyDetails AgencyDetails { get; set; }
        public AggregatorDetails AggregatorDetails { get; set; }
        public EndpointDetails EndpointDetails { get; set; }
        public string Environment { get; set; }
        public string KeyType { get; set; }
    }
}
