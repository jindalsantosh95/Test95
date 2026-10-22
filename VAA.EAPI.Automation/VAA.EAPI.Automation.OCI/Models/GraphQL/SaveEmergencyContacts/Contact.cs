namespace VAA.EAPI.Automation.OCI.Models.GraphQL.SaveEmergencyContacts
{
    public class Contact
    {
        public string passengerId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string phoneCountryCode { get; set; }
        public string phoneNumber { get; set; }
        public bool passengerRefused { get; set; }
    }
}