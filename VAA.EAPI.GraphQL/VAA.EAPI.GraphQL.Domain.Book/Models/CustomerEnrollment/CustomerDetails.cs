namespace VAA.EAPI.GraphQL.Domain.Book.Models.CustomerEnrollment
{
    public class Customerdetails
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Middlename { get; set; }
        public string Gender { get; set; }
        public string CustomerType { get; set; }
        public DateOnly DateOfBirth { get; set; }
        public Contactdetails ContactDetails { get; set; }
        public string MemberType { get; set; }
        public string ProgramName { get; set; }
        public string MemberNumber { get; set; }
        public string FlyingClubCardNumber { get; set; }
        public string Status { get; set; }
        public string CampaignCode { get; set; }
        public string MediaCode { get; set; }
        public string EnrollmentChannel { get; set; }
        public bool TermsAndConditionsAccepted { get; set; }
        public IEnumerable<Preferences> Preferences { get; set; }
    }
}
