using System;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction
{
    public class Segmentdetail
    {
        public string SourceIdentifier { get; set; }
        public string SegmentId { get; set; }
        public string SourceSystem { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public string BookingClass { get; set; }
        public string DepartureDate { get; set; }
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


        public Segmentdetail()
        {

        }

        public Segmentdetail(SegmentdetailDTO segmentdetail)
        {
            SegmentId = segmentdetail?.SegmentId;
            SourceIdentifier = segmentdetail?.SourceIdentifier;
            SourceSystem = segmentdetail?.SourceSystem;
            CreatedOn = string.IsNullOrEmpty(segmentdetail?.CreatedOn) ? null : DateTime.Parse(segmentdetail?.CreatedOn);
            CreatedBy = segmentdetail?.CreatedBy;
            LastUpdatedOn = string.IsNullOrEmpty(segmentdetail?.LastUpdatedOn) ? null : DateTime.Parse(segmentdetail?.LastUpdatedOn);
            LastUpdatedBy = segmentdetail?.LastUpdatedBy;
            BookingClass = segmentdetail?.BookingClass;
            DepartureDate = segmentdetail?.DepartureDate;
            DestinationAirportcode = segmentdetail?.DestinationAirportcode;
            FareBasisCode = segmentdetail?.FareBasisCode;
            MarketingCarrierCode = segmentdetail?.MarketingCarrierCode;
            MarketingFlightNumber = segmentdetail?.MarketingFlightNumber;
            OperatingFlightNumber = segmentdetail?.OperatingFlightNumber;
            OriginAirportcode = segmentdetail?.OriginAirportcode;
            PartnerCode = segmentdetail?.PartnerCode;
            SegmentMiles = segmentdetail?.SegmentMiles;
            StopOverCode = segmentdetail?.StopOverCode;
            TicketDesignator = segmentdetail?.TicketDesignator;
            CouponNumber = segmentdetail?.CouponNumber;
            FareBreakSequenceNumber = segmentdetail?.FareBreakSequenceNumber;
        }
    }
}
