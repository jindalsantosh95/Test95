namespace VAA.EAPI.Automation.Explore.Requests.Models.ResponseModels.AppointmentBooker
{

    public class SaveAppointmentBookingResponse
    {
        public SaveAppointmentBookingData data { get; set; }
    }

    public class SaveAppointmentBookingData
    {
        public SaveAppointmentBooking SaveAppointmentBooking { get; set; }
    }

    public class SaveAppointmentBooking
    {
        public string appointmentReferenceId { get; set; }
        public string date { get; set; }
        public string store { get; set; }
        public Slotdetails slotDetails { get; set; }
    }

    public class Slotdetails
    {
        public DateTimeOffset startTime { get; set; }
        public DateTimeOffset endTime { get; set; }
        public bool isAvailable { get; set; }
    }
}
