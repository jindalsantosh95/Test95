using System;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.AccountLinking
{
    public sealed class CreateLinkingProofRequest
    {
        public string FlyingClubAccountNumber { get; set; }
        public string TransactionId { get; set; }
        public string ClientId { get; set; }
        public string Resource { get; set; }
        public string Scope { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string TransactionBindingHash { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string AssertionReplayId { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string Nonce { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string ProofReplayId { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public string AuthenticatedCallerApplicationId { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        public DateTime ExpiresAtUtc { get; set; }
    }
}
