namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain
{
    public class MemberLoginRequest
    {
        public string MemberNumber { get; set; }
        public string EmailAddress { get; set; }
        public string Password { get; set; }
    }
}
