namespace VAA.EAPI.Automation.Explore.Helpers
{
    public class StoreManagerClosure
    {
        public string ClosureId { get; set; } = string.Empty;
        public string StoreId { get; set; } = string.Empty;
        public string ClosureDate { get; set; } = string.Empty;
        public string ClosureType { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime UpdatedUtc { get; set; }
    }
}
