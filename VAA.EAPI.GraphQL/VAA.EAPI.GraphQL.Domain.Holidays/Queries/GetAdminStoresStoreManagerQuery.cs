using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.Holidays.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
public class GetAdminStoresStoreManagerQuery
{
    private readonly IStoreManagerServiceProvider _storeServiceProvider;

    public GetAdminStoresStoreManagerQuery(IStoreManagerServiceProvider storeServiceProvider)
    {
        _storeServiceProvider = storeServiceProvider ?? throw new ArgumentNullException(nameof(storeServiceProvider));
    }
    [GraphQLName(GraphQlOperationNames.Queries.StoreManagerAdminStores)]
    [GraphQLDescription("List all stores for store management workflows.")]
    [Authorize(Roles = new[] { Roles.StoreManager, Roles.StoreAdmin })]
    public async Task<IEnumerable<StoreManagerStoreResponse>> GetAdminStores()
    {
        return await _storeServiceProvider.GetAdminStoresAsync();
    }
}
