using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.StoreManager;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Interfaces.StoreManager;

public interface IStoreManagerService
{
    Task<IReadOnlyCollection<StoreResponse>> GetStores();
    Task<ServiceResult<StoreResponse>> GetStore(string id);
    Task<ServiceResult<StoreResponse>> UpdateStore(string id, StoreRequest request);

    Task<ServiceResult<IReadOnlyCollection<OpeningHoursEntryResponse>>> GetOpeningHours(string storeId);
    Task<ServiceResult<IReadOnlyCollection<OpeningHoursEntryResponse>>> ReplaceOpeningHours(string storeId, OpeningHoursReplaceRequest request);

    Task<ServiceResult<IReadOnlyCollection<SlotRuleResponse>>> GetSlotRules(string storeId);
    Task<ServiceResult<IReadOnlyCollection<SlotRuleResponse>>> ReplaceSlotRules(string storeId, SlotRuleReplaceRequest request);

    Task<ServiceResult<IReadOnlyCollection<WindowResponse>>> GetWindows(string storeId, DateTimeOffset from, DateTimeOffset to);
    Task<ServiceResult<WindowResponse>> GetSlot(string storeSlotId);
    Task<ServiceResult<IReadOnlyCollection<WindowResponse>>> GenerateSlots(string storeId);
    Task<ServiceResult<WindowResponse>> UpdateSlotStatus(string storeId, string slotId, SlotStatusUpdateRequest request);
    Task<ServiceResult<SlotAllocationResponse>> GetSlotAllocations(string storeId, string slotId, DateTimeOffset startAt);
    Task<ServiceResult<SlotAllocationResponse>> UpdateSlotAllocationStatus(string storeId, string slotId, string allocationId, SlotAllocationStatusUpdateRequest request);

    Task<ServiceResult<IReadOnlyCollection<AvailabilityWindowResponse>>> GetAvailability(string storeId, DateOnly date);
    Task<ServiceResult<IReadOnlyCollection<AvailabilityWindowResponse>>> GetAvailability(string storeId, DateOnly from, DateOnly to);

    Task<ServiceResult<IReadOnlyCollection<StoreClosureResponse>>> GetClosures(string storeId, DateOnly from, DateOnly to);
    Task<ServiceResult<StoreClosureResponse>> CreateClosure(string storeId, StoreClosureRequest request);
    Task<ServiceResult<bool>> DeleteClosure(string storeId, string closureId);

    Task<ServiceResult<IReadOnlyCollection<ManageBookingResponse>>> GetBookings(string storeId, DateOnly date, string? status, string? query);
    Task<ServiceResult<ManageBookingResponse>> CancelBooking(string storeId, string bookingId, BookingCancellationRequest request);
}
