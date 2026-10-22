using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain
{
    public class Retain
    {
        public string OriginDestinationId { get; set; }
        public List<string> SegmentId { get; set; }
    }
}
