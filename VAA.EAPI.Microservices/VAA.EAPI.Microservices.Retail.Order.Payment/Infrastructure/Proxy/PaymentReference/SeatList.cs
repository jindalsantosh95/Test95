namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.PaymentReference
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
        public string ArrivalDate { get; set; }
        public string ArrivalTime { get; set; }
        public string DepartureCity { get; set; }
        public string DepartureDate { get; set; }
        public string DepartureTime { get; set; }
        public string AssignedSeatNumber { get; set; }
        public string PendingSeatNumber { get; set; }
        public string PendingSeatPriceAmount { get; set; }
        public string PendingSeatDecimalPrecisionCount { get; set; }
        public string PendingSeatCurrencyCode { get; set; }
        public string PendingSeatBrandId { get; set; }
        public string PendingSeatBrandDescription { get; set; }
        public string PendingSeatProductCode { get; set; }
        public string PendingSeatProductDescription { get; set; }
        public string PendingSeatOfferId { get; set; }
        public string PendingSeatOfferItemNumber { get; set; }
        public bool FreeOfCharge { get; set; }
        public bool ProductSeat { get; set; }
        public bool ExitSeat { get; set; }
    }
}