using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.Holidays.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
public class GetClosuresStoreManagerQuery
{
	private readonly IStoreManagerServiceProvider _storeServiceProvider;

	public GetClosuresStoreManagerQuery(IStoreManagerServiceProvider storeServiceProvider)
	{
		_storeServiceProvider = storeServiceProvider ?? throw new ArgumentNullException(nameof(storeServiceProvider));
	}
	[GraphQLName(GraphQlOperationNames.Queries.StoreManagerClosures)]
	[GraphQLDescription("Get closures for a store in a date range.")]
	[Authorize(Roles = new[] { Roles.StoreManager, Roles.StoreAdmin })]
	public async Task<IEnumerable<StoreClosureResponse>> GetClosures(string storeId, string from, string to)
	{
		return await _storeServiceProvider.GetClosuresAsync(storeId, from, to);
	}
}
