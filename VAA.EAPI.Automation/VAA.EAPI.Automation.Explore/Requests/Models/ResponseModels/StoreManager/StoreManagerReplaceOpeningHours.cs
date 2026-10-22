namespace VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.StoreManager
{
    public class StoreManagerReplaceOpeningHoursResponse
    {
        public StoreManagerReplaceOpeningHoursData data { get; set; }
    }

    public class StoreManagerReplaceOpeningHoursData
    {
        public List<StoreManagerReplaceOpeningHour>? storeManagerReplaceOpeningHours { get; set; }
    }

    public class StoreManagerReplaceOpeningHour
    {
        public int dayOfWeek { get; set; }
        public string opensAt { get; set; }
        public string closesAt { get; set; }
    }
}
