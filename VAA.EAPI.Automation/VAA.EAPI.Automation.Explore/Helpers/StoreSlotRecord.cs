namespace VAA.EAPI.Automation.Explore.Helpers
{
    public class StoreSlotRecord
    {
        public string StoreId { get; set; } = string.Empty;
        public string DayCode { get; set; } = string.Empty;
        public TimeSpan SlotStartTime { get; set; }
        public TimeSpan SlotEndTime { get; set; }
        public int Capacity { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
