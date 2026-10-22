namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.FlyingClubToken
{
    public sealed class FlyingClubTokenResponse
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public int ExpiresIn { get; set; }
    }
}
