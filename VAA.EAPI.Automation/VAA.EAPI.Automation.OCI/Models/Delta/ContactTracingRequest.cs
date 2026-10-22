namespace VAA.EAPI.Automation.OCI.Models.Delta
{
    public class ContactTracingRequest
    {
        public List<Request> requests { get; set; }

        public class Request
        {
            public string passengerResourceId { get; set; }
            public string tripId { get; set; }
            public string @using { get; set; }
            public List<ContactInformation> contactInformation { get; set; }
        }

        public class ContactInformation
        {
            public Address address { get; set; }
            public List<Email> email { get; set; }
            public List<Phone> phone { get; set; }
        }

        public class Address
        {
            public string streetAddress { get; set; }
            public string countryCode { get; set; }
            public string stateCode { get; set; }
            public string cityCode { get; set; }
            public string zipCode { get; set; }
            public bool declined { get; set; }
        }

        public class Email
        {
            public bool declined { get; set; }
            public string value { get; set; }
        }

        public class Phone
        {
            public bool declined { get; set; }
            public string type { get; set; }
            public string preference { get; set; }
            public string countryCode { get; set; }
            public string phoneNumber { get; set; }
        }
    }
}