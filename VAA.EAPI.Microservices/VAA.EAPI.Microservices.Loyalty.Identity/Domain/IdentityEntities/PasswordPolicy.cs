namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.IdentityEntities
{
    public class PasswordPolicy
    {
        public string PolicyId { get; set; }
        public string PolicyDescription { get; set; }
        public bool IsValid { get; set; }
    }
}
