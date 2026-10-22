using System.Collections.Generic;
namespace VAA.EAPI.Domain.Virgin.Models.Products.Flight
{
    public class OriginDestinationAssociation
    {
        public string OdLabel { get; set; }
        public OriginDestination OriginDestination { get; set; }
        public List<string> JourneyRefIds { get; set; }
        public IEnumerable<string> FlightSegmentRefIds { get; set; }
        public string BrandId { get; set; }
        public string OriginDestinationId { get; set; }
    }
}
