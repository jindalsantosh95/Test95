using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Entities;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models.ManagedYoungFlyer;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;
using AccountConstants = VAA.EAPI.GraphQL.Domain.Account.Constants;

namespace VAA.EAPI.GraphQL.Domain.Account.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public sealed class ManagedYoungFlyerAccountsQuery
    {
        private readonly ITokenB2CProvider<string> _tokenProvider;
        private readonly IManagedYoungFlyerAccountProvider _managedYoungFlyerProvider;
        private readonly IAccountCustomerService _customerProvider;
        private readonly ILogger<ManagedYoungFlyerAccountsQuery> _logger;

        public ManagedYoungFlyerAccountsQuery(ITokenB2CProvider<string> tokenProvider, IManagedYoungFlyerAccountProvider managedYoungFlyerProvider, IAccountCustomerService customerProvider, ILogger<ManagedYoungFlyerAccountsQuery> logger)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _managedYoungFlyerProvider = managedYoungFlyerProvider ?? throw new ArgumentNullException(nameof(managedYoungFlyerProvider));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [GraphQLName("accountManagedYoungFlyerAccounts")]
        [GraphQLDescription("Retrieve Young Flyer accounts managed by the authenticated parent or guardian.")]
        // This newly introduced operation has no existing consumers. Keep activitiesCount required, matching
        // accountMemberDetails, so callers explicitly select the activity volume rather than relying on a default.
        public async Task<ManagedYoungFlyerAccountsResponse> GetManagedYoungFlyerAccounts(int activitiesCount, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var jwt = AsyncContext.Current.Context.GetJwtPrincipalFeature()?.AccessToken.Token;

            if (jwt.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(System.Net.HttpRequestHeader.Authorization));

            try
            {
                var tokenResponse = await _tokenProvider.ValidateTokenAsync(jwt);
                var claim = Json.Deserialize<TokenB2CResponse>(tokenResponse) ?? throw new GraphQLException("The authenticated parent identity could not be resolved.");

                if (claim.CustomerId.IsNullOrWhiteSpace() && !claim.FcNumber.IsNullOrWhiteSpace())
                    claim.CustomerId = (await _customerProvider.GetCustomer(string.Empty, claim.FcNumber))?.Customer?.CustomerId;

                if (claim.CustomerId.IsNullOrWhiteSpace())
                    throw new GraphQLException("The authenticated parent customer identity could not be resolved.");

                return await _managedYoungFlyerProvider.GetAsync(claim.CustomerId, claim.FcNumber, activitiesCount, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (GraphQLException)
            {
                throw;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Managed Young Flyer account retrieval failed while calling a downstream service");

                var apiResponse = await ex.ReadResponseContentAsStringAsync();
                var errorDetails = Json.Deserialize<List<ErrorDetails>>(apiResponse);

                if (!errorDetails.IsNullOrEmpty() && errorDetails.Count != 0)
                {
                    var builder = ErrorBuilder.New().SetMessage("Managed Young Flyer account retrieval failed");
                    errorDetails.ForEach(error =>
                    {
                        builder.SetCode(error.Code);
                        builder.SetMessage(error.Message);
                        builder.SetExtension("source", error.ErrorSource);
                    });

                    throw new GraphQLException(builder.Build());
                }

                throw new GraphQLException(ErrorBuilder.New()
                    .SetMessage("Miscellaneous technical error")
                    .SetExtension("Exception", ex.Message)
                    .SetCode(AccountConstants.Application.LinkedMemberRetrievalError)
                    .Build());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Managed Young Flyer account retrieval failed");
                throw new GraphQLException(ErrorBuilder.New()
                    .SetMessage("Miscellaneous technical error")
                    .SetExtension("Exception", ex.Message)
                    .SetCode(AccountConstants.Application.LinkedMemberRetrievalError)
                    .Build());
            }
        }
    }
}
