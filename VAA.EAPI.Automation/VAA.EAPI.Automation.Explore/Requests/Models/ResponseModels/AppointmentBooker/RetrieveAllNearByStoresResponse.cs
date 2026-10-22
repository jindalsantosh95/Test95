namespace VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.AppointmentBooker
{
    public class RetrieveAllNearByStoresResponse
    {
        public Data data { get; set; }
    }

    public class Data
    {
        public Retrieveallnearbystores RetrieveAllNearByStores { get; set; }
        public RetrieveStoreDetailsById RetrieveStoreDetailsByID { get; set; }
    }

    // RetrieveStoreDetailsByID returns a Store directly
    public class RetrieveStoreDetailsById : Store
    {
    }

    public class Retrieveallnearbystores
    {
        public Store[] stores { get; set; }
    }

    public class Store
    {
        public string storeId { get; set; }
        public string name { get; set; }
        public string locationLabel { get; set; }
        public float latitude { get; set; }
        public float longitude { get; set; }
        public string drivingTime { get; set; }
        public string distance { get; set; }
        public bool isNew { get; set; }
        public bool isClosest { get; set; }
        public Featuredimage featuredImage { get; set; }       
        public Fulldetails fullDetails { get; set; }
    }

    public class Featuredimage
    {
        public string src { get; set; }
        public string alt { get; set; }
    }

    public class Fulldetails
    {
        public Details details { get; set; }
        public Openinghours openingHours { get; set; }
        public Services services { get; set; }
    }

    public class Details
    {
        public string phone { get; set; }
        public string emailAddress { get; set; }
        public Address address { get; set; }
    }

    public class Address
    {
        public string addressLine1 { get; set; }
        public string addressLine2 { get; set; }
        public string city { get; set; }
        public string county { get; set; }
        public string countryCode { get; set; }
        public string postalCode { get; set; }
    }

    public class Openinghours
    {
        public bool isOpenNow { get; set; }
        public Weeklyhour[] weeklyHours { get; set; }
        public SpecialHour[] specialHours { get; set; }
    }

    public class Weeklyhour
    {
        public string day { get; set; }
        public string hours { get; set; }
    }

    public class SpecialHour
    {
        public string label { get; set; }
        public string date { get; set; }
    }

    public class Services
    {
        public string[] inStoreFacilities { get; set; }
        public string[] accessibilityFeatures { get; set; }
    }
}