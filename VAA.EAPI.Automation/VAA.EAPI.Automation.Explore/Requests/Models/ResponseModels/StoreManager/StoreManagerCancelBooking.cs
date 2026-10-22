namespace VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.StoreManager
{
    public class StoreManagerCancelBookingResponse
    {
        public StoreManagerCancelBookingData? data { get; set; }
    }

    public class StoreManagerCancelBookingData
    {
        public StoreManagerCancelledBooking? storeManagerCancelBooking { get; set; }
    }

    public class StoreManagerCancelledBooking
    {
        public string? id { get; set; }

        public string? storeId { get; set; }

        public string? storeSlotId { get; set; }

        public string? customerRef { get; set; }

        public string? status { get; set; }

        public DateTimeOffset windowStartAt { get; set; }

        public DateTimeOffset windowEndAt { get; set; }

        public int windowCapacity { get; set; }

        public DateTimeOffset createdAt { get; set; }

        public DateTimeOffset updatedAt { get; set; }
    }
}
