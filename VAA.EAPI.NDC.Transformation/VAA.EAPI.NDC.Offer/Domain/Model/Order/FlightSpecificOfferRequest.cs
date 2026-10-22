using System.Collections.Generic;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class FlightSpecificOfferRequest
    {
        public IEnumerable<FlightSegment> FlightSegmentsRequest { get; set; }
        public IEnumerable<FlightRequest> FlightsRequest { get; set; }
        public IEnumerable<OriginDestinationRequest> OriginDestinations { get; set; }
        public IEnumerable<OriginDestinationRequestProductType> ProductType { get; set; }
        public IEnumerable<OriginDestinationRequestCabinType> CabinType { get; set; }
        public IEnumerable<OriginDestinationRequestClass> _Class { get; set; }


    }
}
