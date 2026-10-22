namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.BackendAPI
{

    public class RedemptionTransaction
    {
        public string MemberNumber { get; set; }
        public string TransactionType { get; set; }
        public string TransactionProcessedDate { get; set; }
        public string TransactionChannel { get; set; }
        public string ActivityDate { get; set; }
        public string ProductName { get; set; }
        public string Points { get; set; }
        public string Status { get; set; }
        public FlightDetail FlightDetail { get; set; }
        public List<Passenger> Passengers { get; set; }
    }

    public class FlightDetail
    {
        public string BookingReference { get; set; }
        public string OperatedFlightNumber { get; set; }
        public string MarketingFlightNumber { get; set; }
        public string MarketingBookingClass { get; set; }
        public string SeatNumber { get; set; }
        public string OriginAirportCode { get; set; }
        public string DestinationAirportCode { get; set; }
        public string TicketIssueDate { get; set; }
        public string TicketNumber { get; set; }
        public string DepartureDate { get; set; }
        public string BookingClass { get; set; }
        public string CabinClass { get; set; }
        public string RetroFlag { get; set; }
        public string RetroOverrideReason { get; set; }
        public string RetroOverrideFlag { get; set; }
    }

    public class Passenger
    {
        public string PassengerName { get; set; }
        public string PointsRedeemed { get; set; }
        public string TicketNumber { get; set; }
        public List<FlightSegment> flightSegments { get; set; }
    }

    public class FlightSegment
    {
        public string DepartureDate { get; set; }
        public string MarketingCarrierCode { get; set; }
        public string MarketingFlightNumber { get; set; }
        public string DestinationAirportCode { get; set; }
        public string PartnerCode { get; set; }
        public string OperatedFlightNumber { get; set; }
        public string OriginAirportCode { get; set; }
        public string SegmentMiles { get; set; }
        public string BookingClass { get; set; }

    }

}
