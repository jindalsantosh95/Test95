using System.Collections.Generic;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class GeneralFlightRequest
    {
        public IEnumerable<Preferences> OriginDestinationOs { get; set; }
        public string OutboundOrigin { get; set; }
        public string OutboundDestination { get; set; }
        public string ReturnOrigin { get; set; }
        public string ReturndDestination { get; set; }
        public LocalAndUTCDateTime DepartureDate { get; set; }
        public LocalAndUTCDateTime ReturnDate { get; set; }
        public int? Adults { get; set; }
        public int? YoungAdults { get; set; }
        public int? Children { get; set; }
        public int? Infants { get; set; }
        public string Cabin { get; set; }
        public string TripType { get; set; }
        public string SearchType { get; set; }
        public bool? LowFareIndicator { get; set; }
        public IEnumerable<OriginDestinationRequestProductType> ProductType { get; set; }

    }
}
