using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop
{
    public class Add
    {
        public string OriginDestinationId { get; set; }
        public List<string> SegmentId { get; set; }
    }
}
