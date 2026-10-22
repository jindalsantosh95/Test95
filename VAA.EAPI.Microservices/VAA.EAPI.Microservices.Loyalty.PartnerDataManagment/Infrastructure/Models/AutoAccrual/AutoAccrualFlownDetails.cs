
namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual
{
    public class AutoAccrualFlownDetails
    {
        public string? FFPAccountNumber { get; set; }
        public string? FFPCarrierCode { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PnrName { get; set; }
        public string? MarketingAirlineCode { get; set; }
        public string? MarketingFlightNum { get; set; }
        public string? OperatingAirlineCode { get; set; }
        public string? OperatingFlightNum { get; set; }
        public string? DestinationPort { get; set; }
        public string? OriginPort { get; set; }
        public string? DepartureDate { get; set; }
        public string? FlownCabinClass { get; set; }
        public string? MarketingCarrierBookingClass { get; set; }
        public string? OperatingCarrierBookingClass { get; set; }
        public string? OperatingRecordLocator { get; set; }
        public string? CouponNumber { get; set; }
        public string? SeatNumber { get; set; }
        public string? TicketNumber { get; set; }
        public string? ProcessingStatus { get; set; }
        public string? OperatingAirAuthNumber { get; set; }
        public string? Error { get; set; }
        public string? FdNoShow { get; set; }
        public string? PostingDate { get; set; }
        public string? SentToPartner { get; set; }
    }
}
