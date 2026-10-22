namespace VAA.EAPI.Automation.Explore.Requests.Models.RequestModels
{
    public class StoreManager
    {
        public string query { get; set; }
        public dynamic variables { get; set; }
    }

    public class StoreManagerAdminStoreByIdVariables
    {
        public required string id { get; set; }
    }

    public class StoreManagerOpeningHoursVariables
    {
        public required string storeId { get; set; }
    }

    public class StoreManagerStoreByIdVariables
    {
        public required string id { get; set; }
    }

    public class StoreManagerReplaceOpeningHoursVariables
    {
        public required string storeId { get; set; }
        public required StoreManagerOpeningHoursReplaceRequestInput request { get; set; }
    }

    public class StoreManagerOpeningHoursReplaceRequestInput
    {
        public required List<StoreManagerOpeningHourReplace> openingHours { get; set; }
    }

    public class StoreManagerOpeningHourReplace
    {
        public int dayOfWeek { get; set; }
        public required string opensAt { get; set; }
        public required string closesAt { get; set; }
        public bool closeAfterOpen { get; set; } // only needed in request
    }

    public class StoreManagerSlotRulesVariables
    {
        public required string storeId { get; set; }
    }

    public class StoreManagerReplaceSlotRulesVariables
    {
        public required string storeId { get; set; }
        public required SlotRuleReplaceRequestInput request { get; set; }
    }

    public class SlotRuleReplaceRequestInput
    {
        public required List<SlotRuleReplace> rules { get; set; }
    }

    public class SlotRuleReplace
    {
        public int dayOfWeek { get; set; }
        public required string startTime { get; set; }
        public required string endTime { get; set; }
        public int slotDurationMinutes { get; set; }
        public int slotCapacity { get; set; }
    }

    public class StoreManagerClosuresVariables
    {
        public required string storeId { get; set; }
        public required string from { get; set; }
        public required string to { get; set; }
    }

    public class StoreClosureRequestInput
    {
        public string closureDate { get; set; } = string.Empty;
        public string closureType { get; set; } = string.Empty;
        public string reason { get; set; } = string.Empty;
    }

    public class DeleteClosureRequestInput
    {
        public string storeId { get; set; } = string.Empty;
        public string closureId { get; set; } = string.Empty;
    }

    public class StoreManagerCancelBookingVariables
    {
        public required string storeId { get; set; }

        public required string bookingId { get; set; }

        public required BookingCancellationRequestInput request { get; set; }
    }

    public class BookingCancellationRequestInput
    {
        public required string reason { get; set; }

        public bool customerContactConfirmed { get; set; }
    }

    public class StoreManagerSlotsVariables
    {
        public required string storeId { get; set; }

        public required string from { get; set; }

        public required string to { get; set; }
    }

    public class StoreManagerSlotAllocationsVariables
    {
        public required string storeId { get; set; }

        public required string slotId { get; set; }

        public required string startAt { get; set; }
    }

    public class StoreManagerAvailabilityRangeVariables
    {
        public required string storeId { get; set; }

        public required string from { get; set; }

        public required string to { get; set; }
    }

    public class StoreManagerUpdateSlotAllocationStatusVariables
    {
        public required string storeId { get; set; }

        public required string slotId { get; set; }

        public required string allocationId { get; set; }

        public required StoreManagerSlotAllocationStatusUpdateRequestInput request { get; set; }
    }

    public class StoreManagerSlotAllocationStatusUpdateRequestInput
    {
        public required string status { get; set; }

        public string? reason { get; set; }
    }

    public class StoreManagerUpdateAdminStoreVariables
    {
        public required string id { get; set; }

        public required StoreRequestInput request { get; set; }
    }

    public class StoreRequestInput
    {
        public required string name { get; set; }

        public required string addressLine1 { get; set; }

        public required string addressLine2 { get; set; }

        public required string cityTown { get; set; }

        public required string countyRegion { get; set; }

        public required string postalCode { get; set; }

        public required string telephone { get; set; }

        public required string email { get; set; }

        public required List<string> services { get; set; }

        public bool isClosed { get; set; }
    }

    public class CancelledBookingsVariables
    {
        public required string storeId { get; set; }

        public required string date { get; set; }

        public required string status { get; set; }
    }
}
