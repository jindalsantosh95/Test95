using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.Holidays.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Mutations;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class DeleteClosureMutation
{
    private readonly IStoreManagerServiceProvider _storeServiceProvider;

    public DeleteClosureMutation(IStoreManagerServiceProvider storeServiceProvider)
    {
        _storeServiceProvider = storeServiceProvider ?? throw new ArgumentNullException(nameof(storeServiceProvider));
    }
    [GraphQLName(GraphQlOperationNames.Mutations.StoreManagerDeleteClosure)]
    [GraphQLDescription("Delete a closure for a store.")]
    [Authorize(Roles = new[] { Roles.StoreManager, Roles.StoreAdmin })]
    public async Task<bool> DeleteClosure(string storeId, string closureId)
    {
        return await _storeServiceProvider.DeleteClosureAsync(storeId, closureId);
    }
}
