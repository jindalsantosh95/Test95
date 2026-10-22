namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain.EmergencyContact
{
    public class EmergencyContactRequest
    {
        public string TravelPartyId { get; set; }
        public string RequirementsId { get; set; }
        public IEnumerable<string> SelectedPassengersIds { get; set; }
        public bool ApplyToAllPassengers { get; set; }
        public IEnumerable<EmergencyContact> EmergencyContacts { get; set; }
    }
}
