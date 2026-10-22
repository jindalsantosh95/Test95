namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.Enrolment
{
    public class GuardianDetails
    {
        public string? Name { get; set; }
        public string? DateOfBirth { get; set; }
        public string? CardNumber { get; set; }
        public PhoneDetail? PhoneDetails { get; set; }
        public EmailDetail? EmailDetails { get; set; }
        public bool? GuardianTermsAndConditionsAccepted { get; set; }
    }
}
