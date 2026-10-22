using System;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Vouchers
{
    public class RedeemVoucherFlightDetails
    {
        public DateTime? ActivityDate { get; set; }
        public string BookingClass { get; set; }
        public DateTime? BookingDate { get; set; }
        public string OriginAirport { get; set; }
        public string DestinationAirport { get; set; }
        public string BookingReference { get; set; }
        public string TicketNumber { get; set; }
        public string OperatingFlightNumber { get; set; }
        public int? NoOfSegments { get; set; }
        public int? NoOfPassenger { get; set; }
        public string MarketingFlightNumber { get; set; }
        public string OperatingCarrierCode { get; set; }
        public string MarketingCarrierCode { get; set; }
    }
}
