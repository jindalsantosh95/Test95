using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.Holidays.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
public class GetSlotsStoreManagerQuery
{
	private readonly IStoreManagerServiceProvider _storeServiceProvider;

	public GetSlotsStoreManagerQuery(IStoreManagerServiceProvider storeServiceProvider)
	{
		_storeServiceProvider = storeServiceProvider ?? throw new ArgumentNullException(nameof(storeServiceProvider));
	}
	[GraphQLName(GraphQlOperationNames.Queries.StoreManagerSlots)]
	[GraphQLDescription("Get slots for a store in a date-time range.")]
	[Authorize(Roles = new[] { Roles.StoreManager, Roles.StoreAdmin })]
	public async Task<IEnumerable<StoreManagerWindowResponse>> GetSlots(string storeId, string from, string to)
	{
		return await _storeServiceProvider.GetSlotsAsync(storeId, from, to);
	}
}
