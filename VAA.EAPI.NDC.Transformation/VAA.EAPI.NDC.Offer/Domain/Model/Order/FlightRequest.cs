using System.Collections.Generic;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class FlightRequest
    {
        public int? TotalJourneyTime { get; set; }
        public DistanceUnits TotalJourneyDistance { get; set; }
        public IEnumerable<FlightSegmentReference> FlightSegmentReferences { get; set; }
        public IEnumerable<OriginDestinationRequestCabinType> CabinType { get; set; }
        public IEnumerable<OriginDestinationRequestClass> _Class { get; set; }
    }
}
