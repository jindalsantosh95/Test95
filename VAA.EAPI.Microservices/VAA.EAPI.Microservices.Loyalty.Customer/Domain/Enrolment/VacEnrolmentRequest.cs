namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment
{
    public class VacEnrolmentRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }
        public string DateOfBirth { get; set; }
        public string PhoneNumber { get; set; }
        public string PhoneCountryCode { get; set; }
        public string MediaCode { get; set; }
        public string TierCode { get; set; }
        public string EnrollmentChannel { get; set; }
        public bool IsBasic { get; set; }
    }
}
