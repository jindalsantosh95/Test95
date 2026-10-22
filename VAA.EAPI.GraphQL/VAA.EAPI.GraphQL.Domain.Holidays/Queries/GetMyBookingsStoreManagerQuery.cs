using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.Holidays.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
public class GetMyBookingsStoreManagerQuery
{
    private readonly IStoreManagerServiceProvider _storeServiceProvider;

    public GetMyBookingsStoreManagerQuery(IStoreManagerServiceProvider storeServiceProvider)
    {
        _storeServiceProvider = storeServiceProvider ?? throw new ArgumentNullException(nameof(storeServiceProvider));
    }
    [GraphQLName(GraphQlOperationNames.Queries.StoreManagerMyBookings)]
    [GraphQLDescription("Get bookings for current manager's store on a date.")]
    [Authorize(Roles = new[] { Roles.StoreManager, Roles.StoreAdmin })]
    public async Task<IEnumerable<ManageBookingResponse>> GetMyBookings(string date, string? status = null, string? q = null)
    {
        return await _storeServiceProvider.GetMyStoreBookingsAsync(date, status ?? string.Empty, q ?? string.Empty);
    }
}
