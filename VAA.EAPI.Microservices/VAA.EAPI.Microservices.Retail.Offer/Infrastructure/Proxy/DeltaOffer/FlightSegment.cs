using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class FlightSegment
    {
        public string ActionCode { get; set; }
        public Aircraft Aircraft { get; set; }
        public string AircraftTypeCode { get; set; }
        public AircraftTypeSpecification AircraftTypeSpecification { get; set; }
        public string CompartmentCode { get; set; }
        public bool DayChange { get; set; }
        public bool DeniedBoarding { get; set; }
        public Airport DestinationAirport { get; set; }
        public string DestinationAirportCode { get; set; }
        public string DisplaySequenceCnt { get; set; }
        public string DestinationCityCode { get; set; }
        public bool DominantSegment { get; set; }
        public string EnrouteStopsNum { get; set; }
        public string IcaoAirlineCode { get; set; }
        public string IcaoOriginAirportCode { get; set; }
        public string IcaoDestinationAirportCode { get; set; }
        public string AircraftRegistrationNum { get; set; }
        public IEnumerable<FlightLeg> FlightLeg { get; set; }
        public FlightSegmentArrivalDepartureStatus FlightSegmentArrivalDepartureStatus { get; set; }
        public string FlightSegmentNum { get; set; }
        public FlightShared FlightShared { get; set; }
        public bool Flown { get; set; }
        public bool Irop { get; set; }
        public Layover Layover { get; set; }
        public Carrier MarketingCarrier { get; set; }
        public string MarketingFlightNum { get; set; }
        public bool NearbyAirport { get; set; }
        public Carrier OperatingCarrier { get; set; }
        public string OperatingCarrierCode { get; set; }
        public string OperatingFlightNum { get; set; }
        public Airport OriginAirport { get; set; }
        public string OriginAirportCode { get; set; }
        public string OriginCityCode { get; set; }
        public bool PendingGovtApproval { get; set; }
        public decimal PriorToDepartureHourCnt { get; set; }
        public string RepriceActionCode { get; set; }
        public string ScheduledArrivalLocalTs { get; set; }
        public string ScheduledArrivalUtcTs { get; set; }
        public bool ScheduleChange { get; set; }
        public string ScheduledDepartureLocalTs { get; set; }
        public string ScheduledDepartureUtcTs { get; set; }
        public int StopCnt { get; set; }
        public string TicketingTimeLimitUtcTs { get; set; }
        public Duration TotalAirTime { get; set; }
        public string TransactionLocalTs { get; set; }
        public string TransactionUtcTs { get; set; }
        public string FlightSegmentId { get; set; }
        public string OperatingClassOfServiceCode { get; set; }
        public string MarketingClassOfServiceCode { get; set; }
        public string FareBasisCode { get; set; }
        public string TicketDesignatorCode { get; set; }
        public IEnumerable<Product> FlightSegmentBadges { get; set; }
    }
}
