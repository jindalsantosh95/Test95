namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.IdentityEntities
{
    public class PasswordProfile
    {
        public bool ForceChangePasswordNextSignIn { get; set; }
        public string Password { get; set; }
    }
}
