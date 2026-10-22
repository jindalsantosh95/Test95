using CSharpFunctionalExtensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Application.Command.StoreAppointment;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Interfaces;

public interface IStoreAppointmentHandler
{
    Task<Result<Response<AppointmentBookingResponse>>> SaveAppointmentAsync(AppointmentBookingRequest request);
}
