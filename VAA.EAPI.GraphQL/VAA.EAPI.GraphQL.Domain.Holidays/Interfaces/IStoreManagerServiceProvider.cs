using VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Interfaces
{
    public interface IStoreManagerServiceProvider
    {
        Task<StoreManagerStoreResponse> GetStoreAsync(string storeId);
        Task<StoreManagerStoreResponse> UpdateStoreAsync(string storeId, StoreRequest request);

        Task<IEnumerable<OpeningHoursEntryResponse>> GetOpeningHoursAsync(string storeId);
        Task<IEnumerable<OpeningHoursEntryResponse>> ReplaceOpeningHoursAsync(string storeId, OpeningHoursReplaceRequest request);

        Task<IEnumerable<SlotRuleResponse>> GetSlotRulesAsync(string storeId);
        Task<IEnumerable<SlotRuleResponse>> ReplaceSlotRulesAsync(string storeId, SlotRuleReplaceRequest request);

        Task<IEnumerable<StoreManagerWindowResponse>> GetSlotsAsync(string storeId, string from, string to);
        Task<StoreManagerWindowResponse> UpdateSlotStatusAsync(string storeId, string slotId, SlotStatusUpdateRequest request);
        Task<ManageSlotAllocationResponse> GetSlotAllocationsAsync(string storeId, string slotId, string startAt);
        Task<ManageSlotAllocationResponse> UpdateSlotAllocationStatusAsync(string storeId, string slotId, string allocationId, StoreManagerSlotAllocationStatusUpdateRequest request);

        Task<IEnumerable<AvailabilityWindowResponse>> GetAvailabilityAsync(string storeId, string date);
        Task<IEnumerable<AvailabilityWindowResponse>> GetAvailabilityRangeAsync(string storeId, string from, string to);

        Task<IEnumerable<StoreClosureResponse>> GetClosuresAsync(string storeId, string from, string to);
        Task<StoreClosureResponse> CreateClosureAsync(string storeId, StoreClosureRequest request);
        Task<bool> DeleteClosureAsync(string storeId, string closureId);

        Task<ManageMeResponse> GetMyStoreUserAsync();
        Task<IEnumerable<ManageBookingResponse>> GetMyStoreBookingsAsync(string date, string status, string query);
        Task<IEnumerable<ManageBookingResponse>> GetStoreBookingsAsync(string storeId, string date, StoreManagerBookingStatus status);
        Task<ManageBookingResponse> CancelBookingAsync(string storeId, string bookingId, BookingCancellationRequest request);

        Task<IEnumerable<StoreManagerStoreResponse>> GetAdminStoresAsync();
        Task<StoreManagerStoreResponse> GetAdminStoreAsync(string storeId);
        Task<StoreManagerStoreResponse> UpdateAdminStoreAsync(string storeId, StoreRequest request);
    }
}
