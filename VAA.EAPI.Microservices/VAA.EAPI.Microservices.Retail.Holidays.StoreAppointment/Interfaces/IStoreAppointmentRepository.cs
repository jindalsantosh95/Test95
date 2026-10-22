using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Appointment;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Interfaces;

public interface IStoreAppointmentRepository
{
    Task<string?> GetLatestAppointmentReferenceIdAsync(string storeInitials);
    Task<StoreAppointmentSaveResult> AddAsync(Appointment appointment);
}
