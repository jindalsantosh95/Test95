using System.Collections.Generic;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class OriginDestinationRequest
    {
        public string Origin { get; set; }
        public string Destination { get; set; }
        public LocalAndUTCDateTime DepartureDateTime { get; set; }
        public LocalAndUTCDateTime ArrivalDateTime { get; set; }
        public string TripType { get; set; }
        public OriginDestinationRequestCalendarDates CalendarDates { get; set; }
        public bool? LowFareIndicator { get; set; }
        public IEnumerable<OriginDestinationRequestProductType> ProductType { get; set; }
        public IEnumerable<OriginDestinationRequestCabinType> CabinType { get; set; }
        public IEnumerable<OriginDestinationRequestClass> _Class { get; set; }


    }
}
