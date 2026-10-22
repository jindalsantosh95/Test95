using CSharpFunctionalExtensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Store;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Interfaces.Store;

public interface IStoreHandler
{
    Task<Result<Response<NearbyStoreResult>>> GetNearByStoresAsync(StoreSearchRequest request);
    Task<Result<Response<StoreResult>>> GetStoreByIdAsync(StoreSearchRequest request);
    Task<Result<Response<StoreSlotsResult>>> GetStoreSlotsByDateAsync(StoreSearchRequest request);
    Task<Result<Response<StoreCalendarAvailabilityResult>>> GetStoreCalendarAvailabilityAsync(StoreSearchRequest request);
}
