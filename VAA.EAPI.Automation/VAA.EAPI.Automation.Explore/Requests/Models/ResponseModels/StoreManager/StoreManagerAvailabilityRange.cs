namespace VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.StoreManager
{
    public class StoreManagerAvailabilityRangeResponse
    {
        public StoreManagerAvailabilityRangeData? data { get; set; }
    }

    public class StoreManagerAvailabilityRangeData
    {
        public StoreManagerAvailabilityRangeItem[]? storeManagerAvailabilityRange { get; set; }
    }

    public class StoreManagerAvailabilityRangeItem
    {
        public string? id { get; set; }

        public string? storeId { get; set; }

        public DateTimeOffset startAt { get; set; }

        public DateTimeOffset endAt { get; set; }

        public int capacity { get; set; }

        public int remainingCapacity { get; set; }
    }
}
