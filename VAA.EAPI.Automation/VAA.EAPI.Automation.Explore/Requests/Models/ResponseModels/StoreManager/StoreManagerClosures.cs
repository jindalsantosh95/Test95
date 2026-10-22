namespace VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.StoreManager
{
    public class StoreManagerClosuresResponse
    {
        public StoreManagerClosuresData data { get; set; } = null!;
    }

    public class StoreManagerClosuresData
    {
        public List<StoreManagerClosure>? storeManagerClosures { get; set; }
    }

    public class StoreManagerClosure
    {
        public string id { get; set; } = string.Empty;
        public string closureDate { get; set; } = string.Empty;
        public string closureType { get; set; } = string.Empty;
        public string reason { get; set; } = string.Empty;
    }
}
