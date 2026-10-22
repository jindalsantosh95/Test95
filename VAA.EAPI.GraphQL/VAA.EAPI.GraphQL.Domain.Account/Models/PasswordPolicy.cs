namespace VAA.EAPI.GraphQL.Domain.Account.Models
{
    public class PasswordPolicy
    {
        public string PolicyId { get; set; }
        public string PolicyDescription { get; set; }
        public bool IsValid { get; set; }
    }
}