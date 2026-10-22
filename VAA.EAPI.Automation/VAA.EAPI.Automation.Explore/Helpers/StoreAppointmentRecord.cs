namespace VAA.EAPI.Automation.Explore.Helpers
{
    public class StoreAppointmentRecord
    {
        public string Id { get; set; } = string.Empty;

        public string AppointmentType { get; set; } = string.Empty;

        public string AppointmentReason { get; set; } = string.Empty;

        public string BookingReference { get; set; } = string.Empty;

        public DateOnly Date { get; set; }

        public TimeSpan SlotStart { get; set; }

        public TimeSpan SlotEnd { get; set; }

        public string LocationId { get; set; } = string.Empty;

        public string VisitPurpose { get; set; } = string.Empty;

        public string VisitPurposeDetails { get; set; } = string.Empty;

        public string AdditionalDetails { get; set; } = string.Empty;

        public string? AllocationId { get; set; }

        public string AppointmentStatus { get; set; } = string.Empty;
    }

    public class StoreSlotBookingState
    {
        public int Capacity { get; set; }
        public int BookedCount { get; set; }
        public bool HasBlockedAppointment { get; set; }
    }
}
