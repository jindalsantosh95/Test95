using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.Holidays.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Mutations;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class CreateClosureMutation
{
    private readonly IStoreManagerServiceProvider _storeServiceProvider;

    public CreateClosureMutation(IStoreManagerServiceProvider storeServiceProvider)
    {
        _storeServiceProvider = storeServiceProvider ?? throw new ArgumentNullException(nameof(storeServiceProvider));
    }
    [GraphQLName(GraphQlOperationNames.Mutations.StoreManagerCreateClosure)]
    [GraphQLDescription("Create a closure for a store.")]
	[Authorize(Roles = new[] { Roles.StoreManager, Roles.StoreAdmin })]

	public async Task<StoreClosureResponse> CreateClosure(string storeId, StoreClosureRequest request)
    {
        return await _storeServiceProvider.CreateClosureAsync(storeId, request);
    }
}
