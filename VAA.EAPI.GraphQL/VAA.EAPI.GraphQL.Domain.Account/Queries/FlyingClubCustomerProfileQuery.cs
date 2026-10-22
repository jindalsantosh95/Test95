using HotChocolate;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models;

namespace VAA.EAPI.GraphQL.Domain.Account.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public sealed class FlyingClubCustomerProfileQuery
    {
        private readonly IFlyingClubCustomerProfileProvider _provider;

        public FlyingClubCustomerProfileQuery(IFlyingClubCustomerProfileProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        [GraphQLName("loyaltyFlyingClubCustomerProfile")]
        [GraphQLDescription("Retrieves the customer profile and Flying Club account identified by a verified Identity broker token.")]
        public Task<FlyingClubCustomerProfileResponse> GetAsync()
        {
            var token = AsyncContext.Current?.Context?.Features.Get<FlyingClubTokenFeature>()?.Token;
            if (token.IsNullOrWhiteSpace())
                throw new RequestValidationException("A Flying Club application bearer token is required.");

            return _provider.GetAsync(token);
        }
    }
}
