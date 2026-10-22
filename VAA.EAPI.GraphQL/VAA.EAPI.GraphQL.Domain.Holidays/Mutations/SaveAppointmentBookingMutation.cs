using System.Net;
using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using VAA.EAPI.GraphQL.Domain.Holidays.Interfaces;
using VAA.EAPI.GraphQL.Domain.Holidays.Models.AppointmentBooking;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Mutations;

[ExtendObjectType(OperationTypeNames.Mutation)]
public class SaveAppointmentBookingMutation
{
    private readonly IAppointmentBookingServiceProvider _storeServiceProvider;

    public SaveAppointmentBookingMutation(IAppointmentBookingServiceProvider storeServiceProvider)
    {
        _storeServiceProvider = storeServiceProvider ?? throw new ArgumentNullException(nameof(storeServiceProvider));
    }

    [GraphQLName(GraphQlOperationNames.Mutations.SaveAppointmentBooking)]
    [GraphQLDescription("Saves a Holidays appointment booking draft and returns confirmation details, including slotDetails with UK offset-aware ISO-8601 timestamps (+00:00/+01:00).")]
    public async Task<AppointmentBookingResponse> SaveAppointmentBooking(AppointmentBookingRequest bookingDetails)
    {
        ArgumentNullException.ThrowIfNull(bookingDetails);
        return await _storeServiceProvider.SaveAppointmentBookingAsync(bookingDetails);
    }
}
