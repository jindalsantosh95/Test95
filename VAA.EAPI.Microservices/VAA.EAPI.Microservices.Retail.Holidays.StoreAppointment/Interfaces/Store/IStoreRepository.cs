using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Store;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Interfaces.Store;

public interface IStoreRepository
{
    Task<IReadOnlyCollection<StoreResult>> GetStoresAsync();
    Task<StoreResult?> GetStoreByIdAsync(string storeId);
    Task<IReadOnlyCollection<StoreSlot>> GetStoreSlotsByDateAsync(string storeId, DateTime selectedDate);
    Task<IReadOnlyCollection<StoreDayAvailability>> GetStoreCalendarAvailabilityAsync(string storeId, int year, int month);
}