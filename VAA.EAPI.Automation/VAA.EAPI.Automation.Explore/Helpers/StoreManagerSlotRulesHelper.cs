namespace VAA.EAPI.Automation.Explore.Helpers
{
    public class StoreManagerSlotRulesHelper
    {
        public string SlotRuleId { get; set; } = string.Empty;
        public string StoreId { get; set; } = string.Empty;
        public int DayOfWeek { get; set; }
        public string StartTime { get; set; } = string.Empty;
        public string EndTime { get; set; } = string.Empty;
        public int SlotDurationMinutes { get; set; }
        public int SlotCapacity { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime UpdatedUtc { get; set; }
        public string DayName { get; set; } = string.Empty;
    }
}
