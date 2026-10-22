using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Domain.Trips.Interface;
using VAA.EAPI.GraphQL.Domain.Trips.Models.Trips.Virgin.DeepLink;

namespace VAA.EAPI.GraphQL.Domain.Trips.Queries.DeepLink
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class ProxyDeeplinkQuery
    {
        private ITripsDetailsQueryProvider _tripsDetailsQueryProvider;        
        private readonly ILogger<ProxyDeeplinkQuery> _logger;

        public ProxyDeeplinkQuery(ITripsDetailsQueryProvider tripsDetailsQueryProvider, ILogger<ProxyDeeplinkQuery> logger)
        {           
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _tripsDetailsQueryProvider = tripsDetailsQueryProvider ?? throw new ArgumentNullException(nameof(tripsDetailsQueryProvider));
        }

        [GraphQLName("GetDeepLinkForPost")]
        [GraphQLDescription("Retrieve Deeplink URL with Details from CacheId profided")]
        public async Task<LinkDetail> GetDeepLinkDetails(string id)
        {
            ArgumentNullException.ThrowIfNull(id);
            return (LinkDetail)await _tripsDetailsQueryProvider.GenerateDeeplinkForProxy(id);            
        }
    }
}