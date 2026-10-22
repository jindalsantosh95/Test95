using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.Holidays.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
public class GetOpeningHoursStoreManagerQuery
{
    private readonly IStoreManagerServiceProvider _storeServiceProvider;

    public GetOpeningHoursStoreManagerQuery(IStoreManagerServiceProvider storeServiceProvider)
    {
        _storeServiceProvider = storeServiceProvider ?? throw new ArgumentNullException(nameof(storeServiceProvider));
    }
    [GraphQLName(GraphQlOperationNames.Queries.StoreManagerOpeningHours)]
    [GraphQLDescription("Get opening hours for a store.")]
    [Authorize(Roles = new[] { Roles.StoreManager, Roles.StoreAdmin })]
    public async Task<IEnumerable<OpeningHoursEntryResponse>> GetOpeningHours(string storeId)
    {
        return await _storeServiceProvider.GetOpeningHoursAsync(storeId);
    }
}
