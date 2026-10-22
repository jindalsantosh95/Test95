using VAA.EAPI.GraphQL.Domain.Account.Models.AccountLinking;

namespace VAA.EAPI.GraphQL.Domain.Account.Interface
{
    public interface IAccountLinkingProvider
    {
        Task<CreateLinkingProofResponse> CreateAsync(CreateLinkingProofRequest request);
        Task<RedeemLinkingProofResponse> RedeemAsync(RedeemLinkingProofRequest request);
    }
}
