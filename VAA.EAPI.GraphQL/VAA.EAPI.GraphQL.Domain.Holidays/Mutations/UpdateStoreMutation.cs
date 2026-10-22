using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.Holidays.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Mutations;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class UpdateStoreMutation
{
    private readonly IStoreManagerServiceProvider _storeServiceProvider;

    public UpdateStoreMutation(IStoreManagerServiceProvider storeServiceProvider)
    {
        _storeServiceProvider = storeServiceProvider ?? throw new ArgumentNullException(nameof(storeServiceProvider));
    }
    [GraphQLName(GraphQlOperationNames.Mutations.StoreManagerUpdateStore)]
    [GraphQLDescription("Update a store.")]
    [Authorize(Roles = new[] { Roles.StoreManager, Roles.StoreAdmin })]
    public async Task<StoreManagerStoreResponse> UpdateStore(string id, StoreRequest request)
    {
        return await _storeServiceProvider.UpdateStoreAsync(id, request);
    }
}
