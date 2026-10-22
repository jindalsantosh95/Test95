using VAA.EAPI.Microservices.Loyalty.Identity.Domain.AccountLinking;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces
{
    public interface ILinkingTransactionValidator
    {
        ValidatedLinkingTransaction ValidateCreation(CreateLinkingProofRequest request);
        ValidatedLinkingTransaction ValidateRedemption(RedeemLinkingProofRequest request);
    }
}
