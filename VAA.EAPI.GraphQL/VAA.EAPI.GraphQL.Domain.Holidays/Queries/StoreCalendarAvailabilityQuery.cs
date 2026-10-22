using HotChocolate;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.Holidays.Interfaces;
using VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreLocator;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
public class StoreCalendarAvailabilityQuery
{
    private readonly IStoreLocatorServiceProvider _storeServiceProvider;

    public StoreCalendarAvailabilityQuery(IStoreLocatorServiceProvider storeServiceProvider)
    {
        _storeServiceProvider = storeServiceProvider ?? throw new ArgumentNullException(nameof(storeServiceProvider));
    }

    [GraphQLName(GraphQlOperationNames.Queries.RetrieveStoreCalendarAvailability)]
    [GraphQLDescription("Retrieve monthly slot availability for a store.")]
    public async Task<StoreCalendarAvailabilityResponse> GetStoreCalendarAvailability(string storeId, int year, int month)
    {
        return await _storeServiceProvider.GetStoreCalendarAvailabilityAsync(storeId, year, month);
    }
}
