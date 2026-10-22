namespace VAA.EAPI.GraphQL.Domain.VHols.Models.BookingResponse
{
    public class ContactCentre
    {
        public string? Telephone { get; set; }
        public string? EmergencyTelephone { get; set; }
        public string? TelephoneOfficialEmergency { get; set; }
        public string? TelephoneAbroad { get; set; }
        public string? Email { get; set; }
        public string? WeekdaysHours { get; set; }
        public string? SaturdayHours { get; set; }
        public string? SundayHours { get; set; }
        public string? WeekdaysOpeningHours { get; set; }
        public string? WeekdaysClosingHours { get; set; }
        public string? SaturdayOpeningHours { get; set; }
        public string? SaturdayClosingHours { get; set; }
        public string? SundayOpeningHours { get; set; }
        public string? SundayClosingHours { get; set; }
        public string? Timezone { get; set; }
        public string? LocationId { get; set; }
        public string? ZendeskId { get; set; }
        public bool? HasChatAgent { get; set; }
    }
}

