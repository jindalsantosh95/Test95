namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerRetro
{
    public class BookingDetails
    {
        public string TicketIssueDate { get; set; }
        public string GdsRecordLocator { get; set; }
        public string TicketNumber { get; set; }
        public string SeatNumber { get; set; }
        public string BoardingNumber { get; set; }
        public RetroFlightDetails FlightDetails { get; set; }
    }
}
