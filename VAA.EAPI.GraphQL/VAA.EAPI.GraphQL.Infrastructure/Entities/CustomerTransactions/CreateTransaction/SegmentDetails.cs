using System;

namespace VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions.CreateTransaction
{
    public class SegmentDetails
    {
        public string SourceIdentifier { get; set; }
        public string SegmentId { get; set; }
        public string SourceSystem { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public string BookingClass { get; set; }
        public DateTime? DepartureDate { get; set; }
        public string DestinationAirportcode { get; set; }
        public string FareBasisCode { get; set; }
        public string MarketingCarrierCode { get; set; }
        public string MarketingFlightNumber { get; set; }
        public string OperatingFlightNumber { get; set; }
        public string OriginAirportcode { get; set; }
        public string PartnerCode { get; set; }
        public string SegmentMiles { get; set; }
        public string StopOverCode { get; set; }
        public string TicketDesignator { get; set; }
        public string CouponNumber { get; set; }
        public string FareBreakSequenceNumber { get; set; }
    }
}
