namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.FlyingClubToken
{
    public sealed class FlyingClubTokenRequest
    {
        public string FlyingClubAccountNumber { get; set; }

        public string CallerApplicationId { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public string AuthenticatedCallerApplicationId { get; set; }
    }
}
