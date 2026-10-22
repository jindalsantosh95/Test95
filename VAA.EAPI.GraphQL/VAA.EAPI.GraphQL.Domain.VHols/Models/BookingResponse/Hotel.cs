namespace VAA.EAPI.GraphQL.Domain.VHols.Models.BookingResponse
{
    public class Hotel : IHolidayComponent, IBookingStatusAwareComponent
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? LocationCode { get; set; }
        public string? LocationName { get; set; }
        public List<Room> Rooms { get; set; }
        public Address? Address { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public PartyComposition? PartyComposition { get; set; }
        public string? BookingStatus { get; set; }
        public HotelContent? Content { get; set; }
        public string? HashCode { get; set; }
        public string SortByDate { get; set; }

    }
}

