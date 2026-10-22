using System;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.AccountLinking
{
    public sealed class AccountLinkingProofConflictException : InvalidOperationException
    {
        public AccountLinkingProofConflictException(string message) : base(message) { }
    }
}
