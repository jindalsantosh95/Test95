namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Domain.Entities
{
    public class GuardianDetails
    {
        public string Name { get; set; }
        public string DateOfBirth { get; set; }
        public string CardNumber { get; set; }
        public bool? GuardianTermsAndConditionsAccepted { get; set; }
        public PhoneDetails PhoneDetails { get; set; }
        public EmailDetails EmailDetails { get; set; }
    }
}