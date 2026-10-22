namespace VAA.EAPI.GraphQL.Domain.Account.Models
{
    public class EntraEnrolTokenResponse : TokenResponse
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? MemberNumber { get; set; }
    }
}