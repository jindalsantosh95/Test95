using Newtonsoft.Json;

namespace VAA.EAPI.LoungeManagement.SkyTeam.Domain
{
    public class RetrieveTicketDetailsRequest
    {
        [JsonProperty("eTicketNumber")]
        public string ETicketNumber { get; set; }
        public string CheckinSequenceNumber { get; set; }
        public string TrackID{ get; set; }
        public string LocationCode {  get; set; }
        public string OperatorID { get; set; }
        public string BookingReference { get; set; }
        public string AgentID { get; set; }
        public string OperatingAirlineCode { get; set; }
        public string PassengerFirstName {  get; set; }
        public string PassengerLastName { get; set; }
        public int NumberOfGuests { get; set; }
    }
}
