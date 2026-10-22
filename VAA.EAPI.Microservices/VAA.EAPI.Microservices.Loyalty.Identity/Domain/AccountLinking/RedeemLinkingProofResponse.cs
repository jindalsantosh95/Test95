namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.AccountLinking
{
    public sealed class RedeemLinkingProofResponse
    {
        public string FlyingClubAccountNumber { get; set; }
        [Newtonsoft.Json.JsonProperty("consentSubject")]
        public string ConsentReference { get; set; }
    }
}
