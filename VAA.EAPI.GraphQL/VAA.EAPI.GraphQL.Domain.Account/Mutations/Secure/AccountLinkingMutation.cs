using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.AccountLinking;

namespace VAA.EAPI.GraphQL.Domain.Account.Mutations.Secure
{
    [ExtendObjectType("Mutation")]
    public sealed class AccountLinkingMutation
    {
        private readonly IAccountLinkingProvider _provider;
        public AccountLinkingMutation(IAccountLinkingProvider provider) => _provider = provider;

        [Authorize(Roles = new[] { Constants.Application.AccountLinkingProofRedeemRole })]
        [McpCallerContextRequired]
        [GraphQLNonNullType]
        [GraphQLName("loyaltyRedeemLinkingProof")]
        [GraphQLDescription("Atomically redeems a transaction-bound account-linking proof.")]
        public Task<RedeemLinkingProofResponse> RedeemAsync([GraphQLNonNullType] RedeemLinkingProofRequest input)
        {
            if (input is null)
                throw AccountLinkingError.InvalidTransaction("An account-linking proof input is required.");
            if (input.Proof.IsNullOrWhiteSpace())
                throw AccountLinkingError.Create("ACCOUNT_LINKING_PROOF_INVALID", "Proof is required.", 400);
            AccountLinkingInputValidation.Validate(input.TransactionId, input.ClientId, input.Resource);
            if (input.Proof.Length > 8192)
                throw AccountLinkingError.Create("ACCOUNT_LINKING_PROOF_INVALID", "Account-linking proofs must not exceed 8192 characters.", 400);
            return _provider.RedeemAsync(input);
        }
    }
}
