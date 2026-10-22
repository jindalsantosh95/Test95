using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.SSRToPNR
{
    public class OrderAmendment
    {
        public string ProductType { get; set; }
        public IEnumerable<SpecialServiceRequestInfo> SpecialServiceRequestInfo { get; set; }
    }
}
