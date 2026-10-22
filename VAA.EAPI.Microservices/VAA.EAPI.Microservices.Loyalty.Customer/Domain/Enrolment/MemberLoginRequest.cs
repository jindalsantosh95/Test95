namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment
{
    public class MemberLoginRequest
    {
        public string MemberNumber { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
    }
}
