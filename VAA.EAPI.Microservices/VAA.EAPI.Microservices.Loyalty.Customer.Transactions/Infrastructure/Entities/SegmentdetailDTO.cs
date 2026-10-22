using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities
{
    public class SegmentdetailDTO
    {
        public string SourceIdentifier { get; set; }
        public string SegmentId { get; set; }
        public string SourceSystem { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedOn { get; set; }
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

        public SegmentdetailDTO()
        {

        }

        public static explicit operator SegmentdetailDTO(Segmentdetail segmentdetail)
        {

            return new()
            {
                SegmentId = string.IsNullOrEmpty(segmentdetail?.SegmentId) ? Utility.GetId() : segmentdetail?.SegmentId,
                SourceIdentifier = segmentdetail?.SourceIdentifier,
                SourceSystem = segmentdetail?.SourceSystem,
                CreatedOn = string.IsNullOrEmpty(segmentdetail?.SegmentId) ? DateTime.UtcNow.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat) : segmentdetail?.CreatedOn?.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat),
                CreatedBy = segmentdetail?.CreatedBy,
                LastUpdatedOn = string.IsNullOrEmpty(segmentdetail?.SegmentId) ? DateTime.UtcNow.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat) : segmentdetail?.LastUpdatedOn?.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat),
                LastUpdatedBy = segmentdetail?.LastUpdatedBy,
                BookingClass = segmentdetail?.BookingClass,
                DepartureDate = string.IsNullOrEmpty(segmentdetail?.DepartureDate) ? null : DateTime.Parse(segmentdetail?.DepartureDate).ToString(Constants.Application.DBTimeFormat),
                DestinationAirportcode = segmentdetail?.DestinationAirportcode,
                FareBasisCode = segmentdetail?.FareBasisCode,
                MarketingCarrierCode = segmentdetail?.MarketingCarrierCode,
                MarketingFlightNumber = segmentdetail?.MarketingFlightNumber,
                OperatingFlightNumber = segmentdetail?.OperatingFlightNumber,
                OriginAirportcode = segmentdetail?.OriginAirportcode,
                PartnerCode = segmentdetail?.PartnerCode,
                SegmentMiles = !string.IsNullOrEmpty(segmentdetail?.SegmentMiles) ? ((int)Convert.ToDouble(segmentdetail?.SegmentMiles)).ToString() : string.Empty,
                StopOverCode = segmentdetail?.StopOverCode,
                TicketDesignator = segmentdetail?.TicketDesignator,
                CouponNumber = segmentdetail?.CouponNumber,
                FareBreakSequenceNumber = segmentdetail?.FareBreakSequenceNumber
            };

        }
    }
}
