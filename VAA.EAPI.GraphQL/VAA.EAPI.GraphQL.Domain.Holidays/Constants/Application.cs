namespace VAA.EAPI.GraphQL.Domain.Holidays.Constants;

public static class Application
{
    public const string DomainName = "Holidays";
    public const string StoreManagerStoreById = "/stores/{id}";
    public const string StoreManagerOpeningHours = "/stores/{id}/opening-hours";
    public const string StoreManagerSlotRules = "/stores/{storeId}/slot-rules";
    public const string StoreManagerSlots = "/stores/{storeId}/slots";
    public const string StoreManagerSlotStatus = "/stores/{storeId}/slots/{slotId}/status";
    public const string StoreManagerSlotAllocations = "/stores/{storeId}/slots/{slotId}/allocations";
    public const string StoreManagerSlotAllocationStatus = "/stores/{storeId}/slots/{slotId}/allocations/{allocationId}/status";
    public const string StoreManagerAvailability = "/stores/{storeId}/availability";
    public const string StoreManagerAvailabilityRange = "/stores/{storeId}/availability/range";
    public const string StoreManagerClosures = "/stores/{storeId}/closures";
    public const string StoreManagerClosureById = "/stores/{storeId}/closures/{closureId}";
    public const string StoreManagerBookings = "/stores/{storeId}/bookings";
    public const string StoreManagerCancelBooking = "/stores/{storeId}/bookings/{bookingId}/cancel";

    public const string StoreManagerManageAdminStores = "/manage/admin/stores";
    public const string StoreManagerManageAdminStoreById = "/manage/admin/stores/{id}";
}
