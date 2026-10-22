using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using VAA.EAPI.GraphQL.Domain.Holidays.Constants;
using VAA.EAPI.GraphQL.Domain.Holidays.Interfaces;
using VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Mutations;

[ExtendObjectType(OperationTypeNames.Mutation)]
public sealed class CancelBookingMutation
{
    private readonly IStoreManagerServiceProvider _storeServiceProvider;

    public CancelBookingMutation(IStoreManagerServiceProvider storeServiceProvider) =>
        _storeServiceProvider = storeServiceProvider ?? throw new ArgumentNullException(nameof(storeServiceProvider));

    [GraphQLName(GraphQlOperationNames.Mutations.StoreManagerCancelBooking)]
    [GraphQLDescription("Cancels a booked store appointment after customer contact is confirmed.")]
    [Authorize(Roles = new[] { Roles.StoreManager, Roles.StoreAdmin })]
    public Task<ManageBookingResponse> CancelBooking(string storeId, string bookingId, BookingCancellationRequest request) =>
        _storeServiceProvider.CancelBookingAsync(storeId, bookingId, request);
}
