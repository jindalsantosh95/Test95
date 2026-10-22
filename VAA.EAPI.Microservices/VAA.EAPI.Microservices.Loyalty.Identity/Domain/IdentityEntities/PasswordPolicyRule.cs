namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.IdentityEntities
{
    internal class PasswordPolicyRule : PasswordPolicy
    {
        public string Pattern { get; set; }
        public int MinOccurances { get; set; }
        public int MaxOccurances { get; set; }
    }
}
