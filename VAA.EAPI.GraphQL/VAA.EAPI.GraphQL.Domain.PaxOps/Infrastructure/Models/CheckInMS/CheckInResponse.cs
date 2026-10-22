namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.CheckInMS
{
    public class CheckInResponse
    {
        public string TravelPartyId { get; set; }
        public string RequirementsId { get; set; }
        public Reference Reference { get; set; }
        public IEnumerable<Passenger> Passengers { get; set; }
        public Slice CurrentSlice { get; set; }
        public OnlineCheckInStatus OnlineCheckInStatus { get; set; }
        public bool ApplyEmergencyContactToAllPax { get; set; }
        public bool ApplyContactTracingToAllPax { get; set; }
        public bool ShowBaggageAllowance { get; set; }
    }
}
