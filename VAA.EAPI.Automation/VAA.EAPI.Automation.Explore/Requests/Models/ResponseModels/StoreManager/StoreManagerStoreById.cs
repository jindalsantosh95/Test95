namespace VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.StoreManager
{
    public class StoreManagerStoreByIdResponse
    {
        public StoreManagerStoreByIdData data { get; set; }
    }

    public class StoreManagerStoreByIdData
    {
        public StoreManagerStoreById storeManagerStoreById { get; set; }
    }

    public class StoreManagerStoreById
    {
        public string id { get; set; }
        public string name { get; set; }
        public string label { get; set; }
        public string addressLine1 { get; set; }
        public string addressLine2 { get; set; }
        public string cityTown { get; set; }
        public string countyRegion { get; set; }
        public string postalCode { get; set; }
        public string telephone { get; set; }
        public string[] services { get; set; }
        public bool isClosed { get; set; }
    }
}
