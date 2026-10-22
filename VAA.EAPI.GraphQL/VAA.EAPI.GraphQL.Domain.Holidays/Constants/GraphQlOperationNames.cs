namespace VAA.EAPI.GraphQL.Domain.Holidays.Constants;

public static class GraphQlOperationNames
{
    public static class Queries
    {
        public const string RetrieveAllNearByStores = "RetrieveAllNearByStores";
        public const string RetrieveStoreDetailsByID = "RetrieveStoreDetailsByID";
        public const string RetrieveStoreSlotsByDate = "RetrieveStoreSlotsByDate";
        public const string RetrieveStoreCalendarAvailability = "RetrieveStoreCalendarAvailability";

        public const string StoreManagerStoreById = "storeManagerStoreById";
        public const string StoreManagerOpeningHours = "storeManagerOpeningHours";
        public const string StoreManagerSlotRules = "storeManagerSlotRules";
        public const string StoreManagerSlots = "storeManagerSlots";
        public const string StoreManagerSlotAllocations = "storeManagerSlotAllocations";
        public const string StoreManagerAvailability = "storeManagerAvailability";
        public const string StoreManagerAvailabilityRange = "storeManagerAvailabilityRange";
        public const string StoreManagerClosures = "storeManagerClosures";
        public const string StoreManagerMe = "storeManagerMe";
        public const string StoreManagerMyBookings = "storeManagerMyBookings";
        public const string StoreManagerBookings = "storeManagerBookings";
        public const string StoreManagerAdminStores = "storeManagerAdminStores";
        public const string StoreManagerAdminStoreById = "storeManagerAdminStoreById";
    }

    public static class Mutations
    {
        public const string StoreManagerUpdateStore = "storeManagerUpdateStore";
        public const string StoreManagerReplaceOpeningHours = "storeManagerReplaceOpeningHours";
        public const string StoreManagerReplaceSlotRules = "storeManagerReplaceSlotRules";
        public const string StoreManagerUpdateSlotStatus = "storeManagerUpdateSlotStatus";
        public const string StoreManagerUpdateSlotAllocationStatus = "storeManagerUpdateSlotAllocationStatus";
        public const string StoreManagerCreateClosure = "storeManagerCreateClosure";
        public const string StoreManagerDeleteClosure = "storeManagerDeleteClosure";
        public const string StoreManagerCancelBooking = "storeManagerCancelBooking";
        public const string StoreManagerUpdateAdminStore = "storeManagerUpdateAdminStore";
        public const string SaveAppointmentBooking = "SaveAppointmentBooking";
    }
}
