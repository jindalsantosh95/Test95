using VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreLocator;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Interfaces;

public interface IStoreLocatorServiceProvider
{
    Task<NearbyStoresResponse> GetNearbyStoresAsync(string? searchParameter);
    Task<StoreResponse> GetStoreDetailsByIdAsync(string storeId, string origin);
    Task<StoreSlotsResponse> GetStoreSlotsByDateAsync(string storeId, DateTime date);
    Task<StoreCalendarAvailabilityResponse> GetStoreCalendarAvailabilityAsync(string storeId, int year, int month);
}
