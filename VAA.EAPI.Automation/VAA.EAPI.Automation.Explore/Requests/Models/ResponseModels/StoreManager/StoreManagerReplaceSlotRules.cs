namespace VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.StoreManager
{
    public class StoreManagerReplaceSlotRulesResponse
    {
        public StoreManagerReplaceSlotRulesData? data { get; set; }
    }

    public class StoreManagerReplaceSlotRulesData
    {
        public List<StoreManagerReplaceSlotRule>? storeManagerReplaceSlotRules { get; set; }
    }

    public class StoreManagerReplaceSlotRule
    {
        public string id { get; set; } = string.Empty;
        public int dayOfWeek { get; set; }
        public string startTime { get; set; } = string.Empty;
        public string endTime { get; set; } = string.Empty;
        public int slotDurationMinutes { get; set; }
        public int slotCapacity { get; set; }
    }
}
