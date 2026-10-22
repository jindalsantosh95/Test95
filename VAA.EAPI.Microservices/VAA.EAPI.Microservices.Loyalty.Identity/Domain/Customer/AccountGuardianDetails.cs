namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.Customer
{
    public class AccountGuardianDetails
    {
        public string Name { get; set; }
        public string DateOfBirth { get; set; }
        public string CardNumber { get; set; }
        public AccountPhoneDetail PhoneDetails { get; set; }
        public AccountEmailDetail EmailDetails { get; set; }
        public bool? GuardianTermsAndConditionsAccepted { get; set; }
    }
}
