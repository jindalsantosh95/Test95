namespace VAA.EAPI.GraphQL.Domain.VHols.Models.BookingResponse
{
    public class Transfer : IHolidayComponent, IBookingStatusAwareComponent
    {
        public string? Name { get; set; }
        public string? BookingStatus { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public string? PickUpPoint { get; set; }
        public string? DropOffPoint { get; set; }
        public string? PickUpPointId { get; set; }
        public string? DropOffPointId { get; set; }
        public string? HashCode { get; set; }
        public string SortByDate { get; set; }
    }
}

