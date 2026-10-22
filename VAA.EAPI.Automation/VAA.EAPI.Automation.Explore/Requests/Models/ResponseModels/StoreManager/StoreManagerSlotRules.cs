namespace VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.StoreManager
{
    public class StoreManagerSlotRulesResponse
    {
        public StoreManagerSlotRulesData data { get; set; } = null!;
    }

    public class StoreManagerSlotRulesData
    {
        public StoreManagerSlotRule[]? storeManagerSlotRules { get; set; }
    }

    public class StoreManagerSlotRule
    {
        public string? id { get; set; }
        public int dayOfWeek { get; set; }
        public string? startTime { get; set; }
        public string? endTime { get; set; }
        public int slotDurationMinutes { get; set; }
        public int slotCapacity { get; set; }
    }
}
