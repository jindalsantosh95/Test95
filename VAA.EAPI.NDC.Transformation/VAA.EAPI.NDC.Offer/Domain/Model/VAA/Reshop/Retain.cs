using System.Collections.Generic;

namespace VAA.EAPI.NDC.Offer.Domain.Model.VAA.Reshop
{
    public class Retain
    {
        public string OriginDestinationId { get; set; }
        public IEnumerable<string> SegmentId { get; set; }
    }
}
