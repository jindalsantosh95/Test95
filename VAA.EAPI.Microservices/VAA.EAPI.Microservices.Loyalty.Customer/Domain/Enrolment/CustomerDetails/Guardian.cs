namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CustomerDetails
{
    public partial class Guardian
    {
        public string Name { get; set; }
        public string DateOfBirth { get; set; }
        public string CardNumber { get; set; }
        public Phone PhoneDetails { get; set; }
        public Email EmailDetails { get; set; }
        public bool? GuardianTermsAndConditionsAccepted { get; set; }
    }
}
