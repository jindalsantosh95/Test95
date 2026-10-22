using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class Trip
    {
        public AccountableDocument AccountableDocument { get; set; }
        public bool Coterminal { get; set; }
        public string CurrencyCode { get; set; }
        public string ConnectionAirports { get; set; }
        public Airport DestinationAirport { get; set; }
        public string DestinationAirportCode { get; set; }
        public string DestinationCityCode { get; set; }
        public bool DifferentDepartureAirport { get; set; }
        public bool NewAirport { get; set; }
        public Distance Distance { get; set; }
        public IEnumerable<FlightLeg> FlightLegs { get; set; }
        public IEnumerable<FlightSegment> FlightSegment { get; set; }
        public bool Domestic { get; set; }
        public bool MultipleCabins { get; set; }
        public bool NearbyAirport { get; set; }
        public bool NewSubFleetType { get; set; }
        public string NewSubfleetTypeCode { get; set; }
        public Airport OriginAirport { get; set; }
        public string OriginAirportCode { get; set; }
        public string OriginCityCode { get; set; }
        public decimal PriorToDepartureHourCnt { get; set; }
        public bool Refundable { get; set; }
        public bool SameDayChangeEligible { get; set; }
        public string ScheduledArrivalLocalTs { get; set; }
        public string ScheduledArrivalUtcTs { get; set; }
        public string ScheduledDepartureLocalTs { get; set; }
        public string ScheduledDepartureUtcTs { get; set; }
        public int StopCnt { get; set; }
        public IEnumerable<string> SummarizedProductId { get; set; }
        public Duration TotalTripTime { get; set; }
        public string TripArrivalDate { get; set; }
        public string TripBrandId { get; set; }
        public string TripDepartureDate { get; set; }
        public EligibilityStatus TripEligibility { get; set; }
        public string TripId { get; set; }
        public bool TripModified { get; set; }
        public bool TripSameDayCountry { get; set; }
        public string TripTypeCode { get; set; }
        public bool TripWithFlownSegments { get; set; }
        public IEnumerable<string> FlightSegmentIdList { get; set; }
        public IEnumerable<Product> TripBadges { get; set; }
    }
}
