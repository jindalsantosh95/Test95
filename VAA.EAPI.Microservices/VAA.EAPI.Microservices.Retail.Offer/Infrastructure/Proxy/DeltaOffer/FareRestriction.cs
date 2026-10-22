using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class FareRestriction
    {
        public Price BaseFare { get; set; }
        public string CarrierCode { get; set; }
        public bool ConnectionStopOver { get; set; }
        public string DestinationAirportCode { get; set; }
        public string OriginCityCode { get; set; }
        public string DestinationCityCode { get; set; }
        public IEnumerable<string> FareBasisCode { get; set; }
        public string FareRestrictionFlightSegmentNum { get; set; }
        public string FareTypeCode { get; set; }
        public string FlightNum { get; set; }
        public string NotValidAfterLocalTs { get; set; }
        public string NotValidAfterUtcTs { get; set; }
        public string NotValidBeforeLocalTs { get; set; }
        public string NotValidBeforeUtcTs { get; set; }
        public string OriginAirportCode { get; set; }
        public Price SalesAdjustedPrice { get; set; }
        public string ScheduledArrivalLocalTs { get; set; }
        public string ScheduledArrivalUtcTs { get; set; }
        public string ScheduledDepartureLocalTs { get; set; }
        public string ScheduledDepartureUtcTs { get; set; }
        public IEnumerable<Surcharge> Surcharge { get; set; }
        public IEnumerable<TravelRelatedServiceTax> Tax { get; set; }
        public IEnumerable<string> TicketDesignator { get; set; }
        public string FlightLegNum { get; set; }
        public string FlightSegmentNum { get; set; }
        public string TripId { get; set; }
    }
}
