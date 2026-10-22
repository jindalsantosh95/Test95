namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerRetro
{
    public class TicketDetails
    {
        public string FFPCarrierCode { get; set; }
        public string FFPAccountNumber { get; set; }
        public string PassengerLastName { get; set; }
        public string PassengerFirstName { get; set; }
        public string FFPMemberEmailAddress { get; set; }
        public string FFPMemberDateOfBirth { get; set; }
        public string OperatingCarrierCode { get; set; }
        public string OperatingFlightNumber { get; set; }
        public string ScheduledFlightSegmentDepartureDate { get; set; }
        public string FlightSegmentOrigin { get; set; }
        public string FlightSegmentDestination { get; set; }
        public string EticketNumber { get; set; }
        public string SeatNumber { get; set; }
        public string OperatingCarrierRecordLocator { get; set; }
    }
}
