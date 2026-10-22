using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.Holidays.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Mutations;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class UpdateAdminStoreMutation
{
    private readonly IStoreManagerServiceProvider _storeServiceProvider;

    public UpdateAdminStoreMutation(IStoreManagerServiceProvider storeServiceProvider)
    {
        _storeServiceProvider = storeServiceProvider ?? throw new ArgumentNullException(nameof(storeServiceProvider));
    }
    [GraphQLName(GraphQlOperationNames.Mutations.StoreManagerUpdateAdminStore)]
    [GraphQLDescription("Update a store using the administrative update workflow.")]
    [Authorize(Roles = new[] { Roles.StoreManager, Roles.StoreAdmin })]
    public async Task<StoreManagerStoreResponse> UpdateAdminStore(string id, StoreRequest request)
    {
        return await _storeServiceProvider.UpdateAdminStoreAsync(id, request);
    }
}
