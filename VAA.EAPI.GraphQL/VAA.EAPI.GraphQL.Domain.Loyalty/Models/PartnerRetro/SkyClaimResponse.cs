namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerRetro
{
    public class SkyClaimResponse
    {
        public string UniqueIdentificationNumber { get; set; }
        public string FfpCarrierCode { get; set; }
        public string FfpAccountNumber { get; set; }
        public string PassengerLastName { get; set; }
        public string PassengerFirstName { get; set; }
        public string MarketingCarrierCode { get; set; }
        public string MarketingFlightNumber { get; set; }
        public string OperatingCarrierCode { get; set; }
        public string OperatingFlightNumber { get; set; }
        public string ScheduledFlightSegmentDepartureDate { get; set; }
        public string CabinClassCode { get; set; }
        public string MarketingCarrierBookingClass { get; set; }
        public string MarketingCarrierBookingSubClass { get; set; }
        public string OperatingCarierBookingClass { get; set; }
        public string OperatingCarrierBookingSubClass { get; set; }
        public string FlightSegmentOrigin { get; set; }
        public string FlightSegmentDestination { get; set; }
        public string EticketNumber { get; set; }
        public string EticketCouponNumber { get; set; }
        public string EtktMatchingResult { get; set; }
        public string FlightTypeIndicator { get; set; }
    }
}
