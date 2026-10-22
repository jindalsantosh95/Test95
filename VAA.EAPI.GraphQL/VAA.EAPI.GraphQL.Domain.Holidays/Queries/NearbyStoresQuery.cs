using HotChocolate;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.Holidays.Interfaces;
using VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreLocator;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
public class NearbyStoresQuery
{
    private readonly IStoreLocatorServiceProvider _storeServiceProvider;

    public NearbyStoresQuery(IStoreLocatorServiceProvider storeServiceProvider)
    {
        _storeServiceProvider = storeServiceProvider ?? throw new ArgumentNullException(nameof(storeServiceProvider));
    }

    [GraphQLName(GraphQlOperationNames.Queries.RetrieveAllNearByStores)]
    [GraphQLDescription("Retrieve all nearby stores for the provided search parameter.")]
    public async Task<NearbyStoresResponse> GetNearbyStores(string? searchParameter = null)
    {
        return await _storeServiceProvider.GetNearbyStoresAsync(searchParameter);
    }
}
