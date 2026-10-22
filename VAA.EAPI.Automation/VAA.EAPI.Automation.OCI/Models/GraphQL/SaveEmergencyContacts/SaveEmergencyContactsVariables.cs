namespace VAA.EAPI.Automation.OCI.Models.GraphQL.SaveEmergencyContacts
{
    public class SaveEmergencyContactsVariables
    {
        public string checkInId { get; set; }
        public bool applyToAllPassengers { get; set; }
        public List<Contact> emergencyContacts { get; set; }
    }
}