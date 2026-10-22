namespace VAA.EAPI.Automation.OCI.Models.GraphQL.ContactTracing
{
    public class ContactTracingInfo
    {
        public string passengerId { get; set; }
        public string streetAddress { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip { get; set; }
        public string email { get; set; }
        public bool inTransit { get; set; }
        public bool noEmail { get; set; }
        public bool noSecondaryPhoneNumber { get; set; }
        public string primaryPhoneNumber { get; set; }
        public string primaryPhoneNumberCountryCode { get; set; }
        public string secondaryPhoneNumber { get; set; }
        public string secondaryPhoneNumberCountryCode { get; set; }
    }
}
