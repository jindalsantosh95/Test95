using VAA.EAPI.Automation.OCI.Models.GraphQL.SaveEmergencyContacts;

namespace VAA.EAPI.Automation.OCI.Models.GraphQL.TravelRequirements
{
    public class SaveEmergencyContactTravelRequirementsVariables
    {
        public string travelRequirementsId { get; set; }
        public bool applyToAllPassengers { get; set; }
        public List<Contact> emergencyContacts { get; set; }
    }
}
