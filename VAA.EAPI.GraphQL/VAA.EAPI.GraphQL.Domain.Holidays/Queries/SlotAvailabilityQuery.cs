using HotChocolate;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.Holidays.Interfaces;
using VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreLocator;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
public class SlotAvailabilityQuery
{
    private readonly IStoreLocatorServiceProvider _storeServiceProvider;

    public SlotAvailabilityQuery(IStoreLocatorServiceProvider storeServiceProvider)
    {
        _storeServiceProvider = storeServiceProvider ?? throw new ArgumentNullException(nameof(storeServiceProvider));
    }

    [GraphQLName(GraphQlOperationNames.Queries.RetrieveStoreSlotsByDate)]
    [GraphQLDescription("Retrieve store slots for a specific date.")]
    public async Task<StoreSlotsResponse> GetStoreSlotsByDate(string storeId, DateTime date)
    {
        return await _storeServiceProvider.GetStoreSlotsByDateAsync(storeId, date);
    }
}
