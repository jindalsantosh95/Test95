namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.AccountLinking
{
    public sealed class CreateLinkingProofResponse
    {
        [Newtonsoft.Json.JsonProperty("proof")]
        public string Proof { get; set; }
        public System.DateTime ExpiresAtUtc { get; set; }
    }
}
