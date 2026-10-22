using Microsoft.Azure.Cosmos.Table;

namespace VAA.EAPI.Microservices.Flight.Services.Domain
{
    public class SsrDetails : TableEntity
    {
        public string SSR { get; set; }
        public string Description { get; set; }
        public string Classification { get; set; }
        public string ReasonForIssuanceSubCode { get; set; }
        public string ReasonForIssuanceCode { get; set; }
        public string PTC { get; set; }
    }
}
