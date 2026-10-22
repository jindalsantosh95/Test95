namespace VAA.EAPI.GraphQL.Domain.VHols.Models.BookingResponse
{
    public class Car : IHolidayComponent, IBookingStatusAwareComponent
    {
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public string? Insurance { get; set; }
        public Depot? PickUpDepot { get; set; }
        public Depot? DropOffDepot { get; set; }
        public List<string>? Images { get; set; }
        public List<string> AvailableVariants { get; set; }
        public List<string> Variant { get; set; }
        public CarContent? CarContent { get; set; }
        public string? Supplier { get; set; }
        public string? BookingStatus { get; set; }
        public string? Id { get; set; }
        public string? HashCode { get; set; }
        public string SortByDate { get; set; }

    }
}

