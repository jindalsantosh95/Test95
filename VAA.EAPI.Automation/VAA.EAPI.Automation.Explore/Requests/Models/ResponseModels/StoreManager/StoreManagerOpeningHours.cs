namespace VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.StoreManager
{
    public class StoreManagerOpeningHoursResponse
    {
        public StoreManagerOpeningHoursData data { get; set; }
    }

    public class StoreManagerOpeningHoursData
    {
        public StoreManagerOpeningHour[]? storeManagerOpeningHours { get; set; }
    }

    public class StoreManagerOpeningHour
    {
        public int dayOfWeek { get; set; }
        public string opensAt { get; set; }
        public string closesAt { get; set; }
    }
}
