namespace VAA.EAPI.GraphQL.Domain.Account.Models
{
    public class TokenResponse
    {
        public string ContinuationToken { get; set; }
        public string TokenType { get; set; }
        public string Scope { get; set; }
        public int ExpiresIn { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string IdToken { get; set; }
    }
}
