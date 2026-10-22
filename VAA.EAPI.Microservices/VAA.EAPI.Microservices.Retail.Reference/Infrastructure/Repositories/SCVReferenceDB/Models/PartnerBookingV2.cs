using System;

namespace VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Repositories.SCVReferenceDB.Models
{
    public class PartnerBookingV2
    {
        public string CabinClass { get; set; }
        public string BookClass { get; set; }
        public string OriginCountryCode { get; set; }
        public string DestinationCountryCode { get; set; }
        public int DistanceInMiles { get; set; }
        
        //New Fields for V2
        public string Type { get; set; }
        public string Value { get; set; }
        public int RetroClaimPeriod { get; set; }
        public DateTime AgreementStartDate { get; set; }
        public DateTime? AgreementEndDate { get; set; }
        public int RetroClaimMinimumPeriod { get; set; }
        public string PartnerCode { get; set; }
        public string RedemptionClassIndicator { get; set; }
        public string BookingClass { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
