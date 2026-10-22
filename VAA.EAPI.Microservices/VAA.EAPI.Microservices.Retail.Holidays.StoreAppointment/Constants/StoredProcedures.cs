namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Constants;

public static class StoredProcedures
{
    public const string StoreMasterGetAll = "dbo.usp_StoreMaster_GetAll";
    public const string StoreMasterGetById = "dbo.usp_StoreMaster_GetById";
    public const string StoreSlotsGetByDate = "dbo.usp_StoreSlots_GetByDate";
    public const string StoreCalendarAvailabilityGetByMonth = "dbo.usp_StoreCalendarAvailability_GetByMonth";
    public const string StoreAppointmentGetLatestReferenceId = "dbo.usp_StoreAppointment_GetLatestReferenceId";
    public const string StoreAppointmentSave = "dbo.usp_StoreAppointment_Save";

    // Store Manager
    public const string StoreManagerGetStores = "dbo.usp_StoreManager_GetStores";
    public const string StoreManagerReplaceOpeningHours = "dbo.usp_StoreManager_ReplaceOpeningHours";
    public const string StoreManagerValidateOpeningHoursBookingImpact = "dbo.usp_StoreManager_ValidateOpeningHoursBookingImpact";
    public const string StoreManagerGetSlots = "dbo.usp_StoreManager_GetSlots";
    public const string StoreManagerUpdateSlotStatus = "dbo.usp_StoreManager_UpdateSlotStatus";
    public const string StoreManagerGetSlotAllocations = "dbo.usp_StoreManager_GetSlotAllocations";
    public const string StoreManagerUpdateSlotAllocationStatus = "dbo.usp_StoreManager_UpdateSlotAllocationStatus";
    public const string StoreManagerCreateBooking = "dbo.usp_StoreManager_CreateBooking";
    public const string StoreManagerCancelBooking = "dbo.usp_StoreManager_CancelBooking";

    public const string StoreManagerGetSlotRules = "dbo.usp_StoreManager_GetSlotRules";
    public const string StoreManagerReplaceSlotRules = "dbo.usp_StoreManager_ReplaceSlotRules";
    public const string StoreManagerValidateSlotRulesBookingImpact = "dbo.usp_StoreManager_ValidateSlotRulesBookingImpact";
    public const string StoreManagerGetSlotById = "dbo.usp_StoreManager_GetSlotById";
    public const string StoreManagerGenerateSlots = "dbo.usp_StoreManager_GenerateSlots";
    public const string StoreManagerGetAvailability = "dbo.usp_StoreManager_GetAvailability";
    public const string StoreManagerGetBookingById = "dbo.usp_StoreManager_GetBookingById";
    public const string StoreManagerGetBookings = "dbo.usp_StoreManager_GetBookings";

    public const string StoreManagerGetStoreById = "dbo.usp_StoreManager_GetStoreById";
    public const string StoreManagerCreateStore = "dbo.usp_StoreManager_CreateStore";
    public const string StoreManagerUpdateStore = "dbo.usp_StoreManager_UpdateStore";
    public const string StoreManagerValidateStoreClosedBookingImpact = "dbo.usp_StoreManager_ValidateStoreClosedBookingImpact";
    public const string StoreManagerDeleteStore = "dbo.usp_StoreManager_DeleteStore";
    public const string StoreManagerGetOpeningHours = "dbo.usp_StoreManager_GetOpeningHours";
    public const string StoreManagerGetClosures = "dbo.usp_StoreManager_GetClosures";
    public const string StoreManagerCreateClosure = "dbo.usp_StoreManager_CreateClosure";
    public const string StoreManagerDeleteClosure = "dbo.usp_StoreManager_DeleteClosure";
}
