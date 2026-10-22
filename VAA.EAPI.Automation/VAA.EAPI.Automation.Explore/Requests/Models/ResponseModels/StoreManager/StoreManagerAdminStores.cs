namespace VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.StoreManager
{
    public class StoreManagerAdminStoresResponse
    {
        public StoreManagerAdminStoresData data { get; set; }
    }

    public class StoreManagerAdminStoresData
    {
        public StoreManagerAdminStore[]? storeManagerAdminStores { get; set; }
    }

    public class StoreManagerAdminStore
    {
        public string id { get; set; }
        public string name { get; set; }
        public string addressLine1 { get; set; }
        public string addressLine2 { get; set; }
        public string cityTown { get; set; }
        public string countyRegion { get; set; }
        public string postalCode { get; set; }
        public string telephone { get; set; }
        public object[] services { get; set; }
    }
}
