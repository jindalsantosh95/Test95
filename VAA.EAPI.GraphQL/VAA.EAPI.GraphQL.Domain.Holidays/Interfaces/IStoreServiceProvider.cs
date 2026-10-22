using InfraResponse = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.AppointmentBooking.AppointmentBookingResponse;
using VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreLocator;
using VAA.EAPI.GraphQL.Domain.Holidays.Models.AppointmentBooking;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Interfaces
{
    public interface IStoreServiceProvider
    {
        Task<NearbyStoresResponse> GetNearbyStoresAsync(string searchParam);
        Task<StoreRecord> GetStoresByStoreIdAsync(string searchParameter, string storeId);
        Task<StoreSlotsRecord> GetStoreSlotsByDateAsync(DateTime date, string storeId);
        Task<InfraResponse> SaveAppointmentBookingAsync(AppointmentBookingRequest request);
        Task<StoreCalendarAvailabilityRecord> GetStoreCalendarAvailabilityAsync(int year, int month, string storeId);
    }
}
