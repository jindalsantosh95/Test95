namespace VAA.EAPI.GraphQL.Domain.VHols.Models.BookingResponse
{
    public class Excursion : IHolidayComponent, IBookingStatusAwareComponent
    {
        public string? BookingStatus { get; set; }
        public string? Name { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public string? HashCode { get; set; }
        public string? PickUpLocation { get; set; }
        public string? DropOffLocation { get; set; }
        public string SortByDate { get; set; }


    }
}

