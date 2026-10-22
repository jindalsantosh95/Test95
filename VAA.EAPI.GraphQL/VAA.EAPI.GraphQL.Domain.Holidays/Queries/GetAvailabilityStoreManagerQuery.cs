using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.Holidays.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
public class GetAvailabilityStoreManagerQuery
{
	private readonly IStoreManagerServiceProvider _storeServiceProvider;

	public GetAvailabilityStoreManagerQuery(IStoreManagerServiceProvider storeServiceProvider)
	{
		_storeServiceProvider = storeServiceProvider ?? throw new ArgumentNullException(nameof(storeServiceProvider));
	}
	[GraphQLName(GraphQlOperationNames.Queries.StoreManagerAvailability)]
	[GraphQLDescription("Get available slots for a store on a date.")]
	[Authorize(Roles = new[] { Roles.StoreManager, Roles.StoreAdmin })]
	public async Task<IEnumerable<AvailabilityWindowResponse>> GetAvailability(string storeId, string date)
	{
		return await _storeServiceProvider.GetAvailabilityAsync(storeId, date);
	}
}
