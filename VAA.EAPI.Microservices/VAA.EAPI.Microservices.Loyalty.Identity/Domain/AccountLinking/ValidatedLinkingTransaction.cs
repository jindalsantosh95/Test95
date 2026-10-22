using System;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.AccountLinking
{
    public sealed class ValidatedLinkingTransaction
    {
        public string TransactionId { get; set; }
        public string BindingHash { get; set; }
        public string AssertionReplayId { get; set; }
        public string Nonce { get; set; }
        public string ProofReplayId { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
    }
}
