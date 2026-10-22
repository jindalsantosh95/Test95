namespace VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.AppointmentBooker
{
    public class RetrieveStoreSlotsByDateResponse
    {
        public RetrieveStoreSlotsByDateData data { get; set; }
    }

    public class RetrieveStoreSlotsByDateData
    {
        public Retrievestoreslotsbydate RetrieveStoreSlotsByDate { get; set; }
    }

    public class Retrievestoreslotsbydate
    {
        public string storeId { get; set; }
        public Slot[] slots { get; set; }
    }

    public class Slot
    {
        public DateTimeOffset startTime { get; set; }
        public DateTimeOffset endTime { get; set; }
        public bool isAvailable { get; set; }
    }

}