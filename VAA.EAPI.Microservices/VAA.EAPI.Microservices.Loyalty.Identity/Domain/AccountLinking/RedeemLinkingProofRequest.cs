namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.AccountLinking
{
    public sealed class RedeemLinkingProofRequest
    {
        [Newtonsoft.Json.JsonProperty("proof")]
        public string Proof { get; set; }
        public string TransactionId { get; set; }
        public string ClientId { get; set; }
        public string Resource { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string TransactionBindingHash { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string AuthenticatedCallerApplicationId { get; set; }
    }
}
