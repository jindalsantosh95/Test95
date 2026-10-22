using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class FlightLeg
    {
        public Aircraft Aircraft { get; set; }
        public AircraftTypeSpecification AircraftTypeSpecification { get; set; }
        public string BaggageArrivalCarouselNum { get; set; }
        public string CarrierCode { get; set; }
        public bool CodeShareOnly { get; set; }
        public bool? ConnectionCarrierOnly { get; set; }
        public bool DayChange { get; set; }
        public Airport DestinationAirport { get; set; }
        public string DestinationAirportCode { get; set; }
        public IEnumerable<AirportGroup> DestinationAirportGroups { get; set; }
        public string DestinationCountryCode { get; set; }
        public string DestinationRegionCode { get; set; }
        public int DisplaySequenceCnt { get; set; }
        public Distance Distance { get; set; }
        public bool Domestic { get; set; }
        public bool? DominantLeg { get; set; }
        public Duration Duration { get; set; }
        public bool EarnLoyaltyMiles { get; set; }
        public bool FeeRestricted { get; set; }
        public string FlightDepartureDate { get; set; }
        public FlightLegArrivalDepartureStatus FlightLegArrivalDepartureStatus { get; set; }
        public string FlightLegNum { get; set; }
        public string FlightLocalTime { get; set; }
        public string FlightNum { get; set; }
        public string FlightSegmentNum { get; set; }
        public FlightShared FlightShared { get; set; }
        public string FlightTypeCode { get; set; }
        public string GroundHandlingCarrierCode { get; set; }
        public Layover Layover { get; set; }
        public string LegId { get; set; }
        public int LineOfFlightSequenceCnt { get; set; }
        public IEnumerable<Link> Links { get; set; }
        public bool LoyaltyMemberBenefits { get; set; }
        public Carrier MarketingCarrier { get; set; }
        public string MarketingCarrierCode { get; set; }
        public string MarketingFlightNum { get; set; }
        public Performance OnTimePerformance { get; set; }
        public bool OperatedByOwnerCarrier { get; set; }
        public Carrier OperatingCarrier { get; set; }
        public string OperatingCarrierCode { get; set; }
        public string OperatingFlightNum { get; set; }
        public Airport OriginAirport { get; set; }
        public string OriginAirportCode { get; set; }
        public string OriginCityCode { get; set; }
        public IEnumerable<AirportGroup> OriginAirportGroups { get; set; }
        public string OriginCountryCode { get; set; }
        public string OriginRegionCode { get; set; }
        public bool RedEye { get; set; }
        public string ReturnedCode { get; set; }
        public string ScheduledArrivalLocalTs { get; set; }
        public string ScheduledArrivalUtcTs { get; set; }
        public string ScheduledDepartureLocalTs { get; set; }
        public string ScheduledDepartureUtcTs { get; set; }
        public string ShipNum { get; set; }
        public string TakeOffNum { get; set; }
        public bool Transcon { get; set; }
        public bool Usable { get; set; }
        public string DestinationCityCode { get; set; }
        public IEnumerable<Product> FlightLegBadges { get; set; }
    }
}
