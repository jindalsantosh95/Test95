namespace VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.AppointmentBooker
{

    public class RetrieveStoreCalendarAvailabilityResponse
    {
        public RetrieveStoreCalendarAvailabilityData data { get; set; }
    }

    public class RetrieveStoreCalendarAvailabilityData
    {
        public RetrieveStoreCalendarAvailability RetrieveStoreCalendarAvailability { get; set; }
    }

    public class RetrieveStoreCalendarAvailability
    {
        public string storeId { get; set; }
        public int year { get; set; }
        public int month { get; set; }
        public Day[] days { get; set; }
    }

    public class Day
    {
        public DateTime date { get; set; }
        public bool hasAvailableSlot { get; set; }
    }
}