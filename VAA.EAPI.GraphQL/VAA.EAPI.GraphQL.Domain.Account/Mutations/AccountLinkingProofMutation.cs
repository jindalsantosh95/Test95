using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using Microsoft.Azure.Functions.Worker;
using System.IdentityModel.Tokens.Jwt;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.AccountLinking;

namespace VAA.EAPI.GraphQL.Domain.Account.Mutations
{
    [ExtendObjectType("Mutation")]
    public sealed class AccountLinkingProofMutation
    {
        private readonly IAccountLinkingProvider _provider;
        private readonly IAccountLinkingEligibilityService _eligibility;
        public AccountLinkingProofMutation(IAccountLinkingProvider provider, IAccountLinkingEligibilityService eligibility)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _eligibility = eligibility ?? throw new ArgumentNullException(nameof(eligibility));
        }

        [Authorize]
        [GraphQLNonNullType]
        [GraphQLName("accountCreateLinkingProof")]
        [GraphQLDescription("Creates a short-lived, transaction-bound proof for linking the authenticated Flying Club account.")]
        public async Task<CreateLinkingProofResponse> CreateAsync([GraphQLNonNullType] CreateLinkingProofInput input)
        {
            if (AsyncContext.Current?.Context?.Features.Get<AccountLinkingProofRequestLimitFeature>() is not null)
                throw AccountLinkingError.InvalidTransaction("The account-linking proof request exceeds the inspection limit.");
            var principal = AsyncContext.Current?.Context?.Features.Get<JwtPrincipalFeature>()?.Principal;
            AccountLinkingInputValidation.Validate(input?.TransactionId, input?.ClientId, input?.Resource);
            if (input.Scope != "account:read")
                throw AccountLinkingError.InvalidTransaction("The account-linking scope must be account:read.");
            var context = AsyncContext.Current?.Context;
            var request = context is null ? null : await context.GetHttpRequestDataAsync();
            var authorization = request?.Headers.TryGetValues("Authorization", out var values) == true ? string.Join(",", values) : null;
            if (!AccountLinkingProofTokenBoundary.MatchesAuthenticatedToken(authorization, context?.Features.Get<JwtPrincipalFeature>()))
                throw AccountLinkingError.Create("ACCOUNT_LINKING_AUTH_REQUIRED", "Authorization token is missing/invalid", 401);
            var path = request?.Url.AbsolutePath;
            if (request?.Method != "POST" || (path != "/api/v1/GraphQL" && path != "/api/v1/accountCreateLinkingProof"))
                throw AccountLinkingError.InvalidTransaction("The account-linking proof request boundary is invalid.");
            string account;
            try { account = await _eligibility.ResolveFlyingClubAccountAsync(principal); }
            catch (RequestValidationException)
            {
                throw AccountLinkingError.Create("ACCOUNT_LINKING_ACCOUNT_INVALID", "Exactly one active Flying Club profile is required.", 403);
            }
            catch (Exception exception) when (exception is HttpRequestException or TimeoutException or TaskCanceledException)
            {
                throw AccountLinkingError.Create("ACCOUNT_LINKING_DEPENDENCY_UNAVAILABLE", "Account linking is temporarily unavailable.", 503, true);
            }
            return await _provider.CreateAsync(new CreateLinkingProofRequest
            {
                FlyingClubAccountNumber = account,
                TransactionId = input.TransactionId,
                ClientId = input.ClientId,
                Resource = input.Resource,
                Scope = input.Scope
            });
        }
    }
}
