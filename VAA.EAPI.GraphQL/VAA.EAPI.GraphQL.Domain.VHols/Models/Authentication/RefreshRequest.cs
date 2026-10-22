namespace VAA.EAPI.GraphQL.Domain.VHols.Models.Authentication
{
    public class RefreshRequest
    {
        public string Scope { get; set; } = "refresh";
        public string RefreshToken { get; set; }
    }
}
