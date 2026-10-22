using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.Holidays.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
public class GetStoreByIdStoreManagerQuery
{
	private readonly IStoreManagerServiceProvider _storeServiceProvider;

	public GetStoreByIdStoreManagerQuery(IStoreManagerServiceProvider storeServiceProvider)
	{
		_storeServiceProvider = storeServiceProvider ?? throw new ArgumentNullException(nameof(storeServiceProvider));
	}
	[GraphQLName(GraphQlOperationNames.Queries.StoreManagerStoreById)]
	[GraphQLDescription("Get store by id.")]
	[Authorize(Roles = new[] { Roles.StoreManager, Roles.StoreAdmin })]
	public async Task<StoreManagerStoreResponse> GetStoreById(string id)
	{
		return await _storeServiceProvider.GetStoreAsync(id);
	}
}
