using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Middleware;
using VAA.EAPI.GraphQL.Domain.Account.Interface;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Domain.Account.Services;
namespace VAA.EAPI.GraphQL.Domain.Account.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public sealed class FlyingClubAccountSummaryQuery
    {
        private readonly IFlyingClubAccountSummaryProvider _provider;
        public FlyingClubAccountSummaryQuery(IFlyingClubAccountSummaryProvider provider) => _provider = provider ?? throw new ArgumentNullException(nameof(provider));

        [Authorize(Roles = new[] { Constants.Application.FlyingClubAccountSummaryRole })]
        [McpCallerContextRequired]
        [GraphQLName("loyaltyFlyingClubAccountSummary")]
        [GraphQLDescription("Returns the least-data Flying Club account summary identified by a verified Identity broker token.")]
        public async Task<FlyingClubAccountSummary> GetAsync(CancellationToken cancellationToken)
        {
            var token = AsyncContext.Current?.Context?.Features.Get<FlyingClubTokenFeature>()?.Token;
            if (token.IsNullOrWhiteSpace()) throw FlyingClubAccountSummaryProvider.InvalidBrokerToken();
            return await _provider.GetAsync(token, cancellationToken);
        }
    }
}
