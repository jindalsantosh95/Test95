namespace VAA.EAPI.GraphQL.Infrastructure.Entities.IdentityMS
{
    public class EntraOtpValidationAPIResponse
    {
        public string TokenType { get; set; }
        public string Scope { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string IdToken { get; set; }
        public string ContinuationToken { get; set; }
        public int ExpiresIn { get; set; }
    }
}
