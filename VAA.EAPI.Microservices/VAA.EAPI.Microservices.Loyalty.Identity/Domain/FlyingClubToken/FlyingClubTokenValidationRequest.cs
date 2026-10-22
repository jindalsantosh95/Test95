namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.FlyingClubToken
{
    public sealed class FlyingClubTokenValidationRequest
    {
        public string AccessToken { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public string AuthenticatedCallerApplicationId { get; set; }
    }
}
