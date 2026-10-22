namespace VAA.EAPI.Automation.Explore.Helpers
{
    public class CancelledBookingDbRecord
    {
        public string BookingId { get; set; } = string.Empty;

        public string StoreId { get; set; } = string.Empty;

        public int? StoreSlotId { get; set; }

        public string CustomerRef { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTimeOffset StartsAtUtc { get; set; }

        public DateTimeOffset EndsAtUtc { get; set; }

        public int Capacity { get; set; }

        public DateTimeOffset CreatedUtc { get; set; }

        public DateTimeOffset UpdatedUtc { get; set; }

        public string? Reason { get; set; }
    }
}
