using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Application.Command.StoreAppointment;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Store;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Interfaces;

public interface IStoreAppointmentEmailPublisher
{
    Task PublishAsync(string appointmentId, AppointmentBookingRequest request, StoreResult? store);
}
