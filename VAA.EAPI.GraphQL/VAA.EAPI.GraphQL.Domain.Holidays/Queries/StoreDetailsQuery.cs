using HotChocolate;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.Holidays.Interfaces;
using VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreLocator;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
public class StoreDetailsQuery
{
    private readonly IStoreLocatorServiceProvider _storeServiceProvider;

    public StoreDetailsQuery(IStoreLocatorServiceProvider storeServiceProvider)
    {
        _storeServiceProvider = storeServiceProvider ?? throw new ArgumentNullException(nameof(storeServiceProvider));
    }

    [GraphQLName(GraphQlOperationNames.Queries.RetrieveStoreDetailsByID)]
    [GraphQLDescription("Retrieve details for a store using store id and search origin.")]
    public async Task<StoreResponse> GetStoreDetailsById(string storeId, string origin)
    {
        return await _storeServiceProvider.GetStoreDetailsByIdAsync(storeId, origin);
    }
}
