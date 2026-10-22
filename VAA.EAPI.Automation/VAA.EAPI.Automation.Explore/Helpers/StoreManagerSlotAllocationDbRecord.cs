namespace VAA.EAPI.Automation.Explore.Helpers
{
    public class StoreManagerSlotAllocationDbRecord
    {
        public int StoreSlotId { get; set; }

        public string StoreId { get; set; } = string.Empty;

        public DateTimeOffset StartsAtUtc { get; set; }

        public DateTimeOffset EndsAtUtc { get; set; }

        public int Capacity { get; set; }

        public int BookedCount { get; set; }

        public int RemainingCapacity { get; set; }

        public string AllocationId { get; set; } = string.Empty;

        public int AllocationIndex { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? Reason { get; set; }

        public string? BookingId { get; set; }

        public string? CustomerRef { get; set; }
    }
}
