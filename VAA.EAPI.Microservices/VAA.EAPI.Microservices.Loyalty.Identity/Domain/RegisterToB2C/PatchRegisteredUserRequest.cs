namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C
{
    public class PatchRegisteredUserRequest
    {
        public string MemberNumber { get; set; }
        public string EmailAddress { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CustomerId { get; set; }
        public string CountryCode { get; set; }
        public string MemberType { get; set; }
    }
}
