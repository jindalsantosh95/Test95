namespace VAA.EAPI.Automation.Explore.Helpers
{
    public class StoreManagerAvailabilityRangeDbRecord
    {
        public int StoreSlotId { get; set; }

        public string StoreId { get; set; } = string.Empty;

        public DateTimeOffset StartsAtUtc { get; set; }

        public DateTimeOffset EndsAtUtc { get; set; }

        public int Capacity { get; set; }

        public int RemainingCapacity { get; set; }

        public string Status { get; set; } = string.Empty;

        public string? Reason { get; set; }
    }
}
