using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Domain.Order.SSR
{
    public class OrderAmendment
    {
        public string ProductType { get; set; }
        public IEnumerable<SpecialServiceRequestInfo> SpecialServiceRequestInfo { get; set; }
    }
}
