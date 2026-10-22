using Newtonsoft.Json;

namespace VAA.EAPI.Automation.Explore.Requests.Models.RequestModels
{
    public class AppointmentBooker
    {
        public string query { get; set; }

        public dynamic variables { get; set; }
    }

    public class RetrieveAllNearByStoresVariables
    {
        public string searchParameter { get; set; }
    }

    public class RetrieveStoreDetailsByIDVariables
    {
        public string storeId { get; set; }
        public string origin { get; set; }
    }

    public class RetrieveStoreSlotsByDateVariables
    {
        public string storeId { get; set; }
        public string date { get; set; }
    }

    public class RetrieveStoreCalendarAvailabilityVariables
    {
        public string storeId { get; set; }
        public int year { get; set; }
        public int month { get; set; }
    }

    public class SaveAppointmentBookingVariables
    {
        public Bookingdetails bookingDetails { get; set; }
    }

    public class Bookingdetails
    {
        public string additionalDetails { get; set; }
        public string appointmentReason { get; set; }
        public object appointmentType { get; set; }
        
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string? bookingReference { get; set; }

        public Customer customer { get; set; }
        public DateOnly date { get; set; }
        public DateTimeOffset slotStart { get; set; }
        public DateTimeOffset slotEnd { get; set; }
        public string locationId { get; set; }
        public string visitPurpose { get; set; }
        public string visitPurposeDetails { get; set; }
        public string searchParameter { get; set; }
    }

    public class Customer
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
    }
}