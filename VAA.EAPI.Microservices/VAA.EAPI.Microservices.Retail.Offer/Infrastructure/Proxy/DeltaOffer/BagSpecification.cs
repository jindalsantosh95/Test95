using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class BagSpecification
    {
        public IEnumerable<Weight> Weight { get; set; }
        public IEnumerable<DimensionDetail> DimensionDetails { get; set; }
    }
}
