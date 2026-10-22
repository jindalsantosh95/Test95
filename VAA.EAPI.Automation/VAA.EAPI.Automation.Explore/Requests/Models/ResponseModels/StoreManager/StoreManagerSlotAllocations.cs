namespace VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.StoreManager
{
    public class StoreManagerSlotAllocationsResponse
    {
        public StoreManagerSlotAllocationsData? data { get; set; }
    }

    public class StoreManagerUpdateSlotAllocationStatusResponse
    {
        public StoreManagerUpdateSlotAllocationStatusData? data { get; set; }
    }

    public class StoreManagerSlotAllocationsData
    {
        public StoreManagerSlotAllocationWindow? storeManagerSlotAllocations { get; set; }
    }

    public class StoreManagerUpdateSlotAllocationStatusData
    {
        public StoreManagerSlotAllocationWindow? storeManagerUpdateSlotAllocationStatus { get; set; }
    }

    public class StoreManagerSlotAllocationWindow
    {
        public string? id { get; set; }

        public string? windowId { get; set; }

        public string? slotId { get; set; }

        public string? storeId { get; set; }

        public DateTimeOffset startAt { get; set; }

        public DateTimeOffset endAt { get; set; }

        public int capacity { get; set; }

        public int bookedCount { get; set; }

        public int remainingCapacity { get; set; }

        public StoreManagerSlotAllocation[]? allocations { get; set; }
    }

    public class StoreManagerSlotAllocation
    {
        public string? id { get; set; }

        public string? windowId { get; set; }

        public string? slotId { get; set; }

        public string? storeId { get; set; }

        public int allocationIndex { get; set; }

        public string? status { get; set; }

        public string? reason { get; set; }

        public string? bookingId { get; set; }

        public string? customerRef { get; set; }
    }
}
