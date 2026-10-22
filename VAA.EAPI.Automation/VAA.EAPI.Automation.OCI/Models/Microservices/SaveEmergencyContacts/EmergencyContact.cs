namespace VAA.EAPI.Automation.OCI.Models.Microservices.SaveEmergencyContacts
{
    public class EmergencyContact
    {
        public string passengerId { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string phoneContryCode { get; set; }
        public string phoneNumber { get; set; }
        public bool passengerRefused { get; set; }
    }
}