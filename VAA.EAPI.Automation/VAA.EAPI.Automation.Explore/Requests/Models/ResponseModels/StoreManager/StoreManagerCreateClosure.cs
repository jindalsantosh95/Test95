namespace VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.StoreManager
{
    public class StoreManagerCreateClosure
    {
        public StoreManagerCreateClosureData? data { get; set; }
    }

    public class StoreManagerCreateClosureData
    {
        public StoreManagerCreateClosureItem? storeManagerCreateClosure { get; set; }
    }

    public class StoreManagerCreateClosureItem
    {
        public string id { get; set; } = string.Empty;
        public string closureDate { get; set; } = string.Empty;
        public string closureType { get; set; } = string.Empty;
        public string reason { get; set; } = string.Empty;
    }
}
