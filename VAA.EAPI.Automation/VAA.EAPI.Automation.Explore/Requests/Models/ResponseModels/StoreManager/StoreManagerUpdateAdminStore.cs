namespace VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.StoreManager
{
    public class StoreManagerUpdateAdminStoreResponse
    {
        public StoreManagerUpdateAdminStoreData? data { get; set; }
    }

    public class StoreManagerUpdateAdminStoreData
    {
        public StoreManagerUpdatedStore? storeManagerUpdateAdminStore { get; set; }
    }

    public class StoreManagerUpdatedStore
    {
        public string? id { get; set; }

        public string? name { get; set; }

        public string? addressLine1 { get; set; }

        public string? addressLine2 { get; set; }

        public string? cityTown { get; set; }

        public string? countyRegion { get; set; }

        public string? postalCode { get; set; }

        public string? telephone { get; set; }

        public string? email { get; set; }

        public string[]? services { get; set; }

        public bool isClosed { get; set; }
    }
}
