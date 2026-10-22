using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class OfferPreferences
    {
        public IEnumerable<string> Carriers { get; set; }
        public IEnumerable<string> FlightNumbers { get; set; }
        public IEnumerable<string> FlightServiceProducts { get; set; }
        public IEnumerable<int> StopCnt { get; set; }
        public IEnumerable<string> SeatTypeCodes { get; set; }
        public decimal FlightSegmentDurationCnt { get; set; }
        public bool FareDowngrade { get; set; }
        public bool NonStopOnly { get; set; }
        public IEnumerable<string> PermittedClassOfService { get; set; }
        public IEnumerable<string> ProhibitedClassOfService { get; set; }
        public bool RefundableOnly { get; set; }
        public bool ShowGlobalRegionalUpgradeCertificate { get; set; }
        public bool UnrestrictedFaresOnly { get; set; }
        public IEnumerable<string> DestinationAirportCode { get; set; }
        public Range ConnectionTimeRange { get; set; }
        public IEnumerable<string> OriginAirportCode { get; set; }
        public Range FlightDurationRange { get; set; }
        public IEnumerable<string> LayoverAirportCode { get; set; }
        public Range TotalMilesRange { get; set; }
        public Range TotalPriceRange { get; set; }
        public OfferResultCriteria OfferResultCriteria { get; set; }
        public bool IncludeBagAllowances { get; set; }
        public bool IncludeBagCharges { get; set; }
        public bool IncludeBagEmbargoes { get; set; }
        public bool IncludeBagDisclosures { get; set; }
        public bool OfferAsBooked { get; set; }
        public bool LowestFare { get; set; }
        public bool ExcludeNewTicketReissue { get; set; }
    }
}
