namespace VAA.EAPI.Automation.OCI.Models.Delta
{
    public class EmergencyContactRequest
    {
        public List<Request> requests { get; set; }

        public class Request
        {
            public string passengerResourceId { get; set; }
            public string @using { get; set; }
            public List<EmergencyContact> emergencyContact { get; set; }
        }

        public class EmergencyContact
        {
            public bool declined { get; set; }
            public string type { get; set; }
            public string countryCode { get; set; }
            public string phoneNumber { get; set; }
            public string firstName { get; set; }
            public string lastName { get; set; }
        }
    }
}