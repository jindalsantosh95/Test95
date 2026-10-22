using System;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment.PaymentReference
{
    public class SeatList
    {
        public string PassengerSequence { get; set; }
        public string FirstNIN { get; set; }
        public string LastNIN { get; set; }
        public string TripId { get; set; }
        public string TripTag { get; set; }
        public string TripSeq { get; set; }
        public string MarketingAirlineCode { get; set; }
        public string MarketingFlightNumber { get; set; }
        public string MarketingClassOfService { get; set; }
        public string OperatingAirline { get; set; }
        public string ArrivalCity { get; set; }
        public DateTime ArrivalDateTime { get; set; }
        public string DepartureCity { get; set; }
        public DateTime DepartureDateTime { get; set; }
        public string AssignedSeatNumber { get; set; }
        public string PendingSeatNumber { get; set; }
        public string PendingSeatPriceAmount { get; set; }
        public string PendingSeatDecimalPrecisionCount { get; set; }
        public string PendingSeatCurrencyCode { get; set; }
        public SeatOfferDetails SeatOfferDetails { get; set; }
        public bool FreeOfCharge { get; set; }
        public bool ProductSeat { get; set; }
        public bool ExitSeat { get; set; }
    }
}