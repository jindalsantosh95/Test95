namespace VAA.EAPI.Automation.OCI.Models.Microservices.SaveEmergencyContacts
{
    public class EmergencyContactRequest
    {
        public string TravelPartyId { get; set; }
        public string RequirementsId { get; set; }
        public bool ApplyToAllPassengers { get; set; }
        public List<EmergencyContact> EmergencyContacts { get; set; }
    }
}