using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.Holidays.Constants;
using VAA.EAPI.GraphQL.Domain.Holidays.Interfaces;
using VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Queries;

[ExtendObjectType(OperationTypeNames.Query)]
public sealed class GetBookingsStoreManagerQuery
{
    private readonly IStoreManagerServiceProvider _storeServiceProvider;

    public GetBookingsStoreManagerQuery(IStoreManagerServiceProvider storeServiceProvider) =>
        _storeServiceProvider = storeServiceProvider ?? throw new ArgumentNullException(nameof(storeServiceProvider));

    [GraphQLName(GraphQlOperationNames.Queries.StoreManagerBookings)]
    [GraphQLDescription("Gets bookings for a selected store and UK-local calendar date, filtered by status.")]
    [Authorize(Roles = new[] { Roles.StoreManager, Roles.StoreAdmin })]
    public Task<IEnumerable<ManageBookingResponse>> GetBookings(
        string storeId,
        string date,
        StoreManagerBookingStatus status) =>
        _storeServiceProvider.GetStoreBookingsAsync(storeId, date, status);
}
