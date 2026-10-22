using System.Collections.Generic;

namespace VAA.EAPI.Web.Search.Models.OfferMS
{
    public class OriginDestinationAssociation
    {
        public string OdLabel { get; set; }
        public OriginDestination OriginDestination { get; set; }
        public IEnumerable<string> JourneyRefIds { get; set; }
    }
}
