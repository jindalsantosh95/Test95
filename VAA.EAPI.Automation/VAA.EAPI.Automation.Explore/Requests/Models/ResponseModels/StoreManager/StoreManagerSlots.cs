namespace VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.StoreManager
{
    public class StoreManagerSlotsResponse
    {
        public StoreManagerSlotsData? data { get; set; }
    }

    public class StoreManagerSlotsData
    {
        public StoreManagerSlot[]? storeManagerSlots { get; set; }
    }

    public class StoreManagerSlot
    {
        public string? id { get; set; }

        public string? storeId { get; set; }

        public DateTimeOffset startAt { get; set; }

        public DateTimeOffset endAt { get; set; }

        public int capacity { get; set; }

        public int remainingCapacity { get; set; }

        public string? status { get; set; }

        public string? reason { get; set; }
    }
}
