using VAA.EAPI.GraphQL.Domain.Holidays.Models.AppointmentBooking;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Interfaces;

public interface IAppointmentBookingServiceProvider
{
    Task<AppointmentBookingResponse> SaveAppointmentBookingAsync(AppointmentBookingRequest request);
}
