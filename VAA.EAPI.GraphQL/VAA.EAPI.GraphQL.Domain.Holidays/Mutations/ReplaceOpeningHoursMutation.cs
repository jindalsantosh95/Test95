using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.Holidays.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Mutations;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class ReplaceOpeningHoursMutation
{
    private readonly IStoreManagerServiceProvider _storeServiceProvider;

    public ReplaceOpeningHoursMutation(IStoreManagerServiceProvider storeServiceProvider)
    {
        _storeServiceProvider = storeServiceProvider ?? throw new ArgumentNullException(nameof(storeServiceProvider));
    }
    [GraphQLName(GraphQlOperationNames.Mutations.StoreManagerReplaceOpeningHours)]
    [GraphQLDescription("Replace opening hours for a store.")]
    [Authorize(Roles = new[] { Roles.StoreManager, Roles.StoreAdmin })]
    public async Task<IEnumerable<OpeningHoursEntryResponse>> ReplaceOpeningHours(string storeId, OpeningHoursReplaceRequest request)
    {
        return await _storeServiceProvider.ReplaceOpeningHoursAsync(storeId, request);
    }
}
