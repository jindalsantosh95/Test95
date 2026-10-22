namespace VAA.EAPI.GraphQL.Domain.VHols.Models.BookingResponse
{
    public class Extra : IHolidayComponent, IBookingStatusAwareComponent
    {
        public string? BookingStatus { get; set; }
        public string? Name { get; set; }
        public string? ElementGroup { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public string? Type { get; set; }
        public string? HashCode { get; set; }
        public string? TbxId { get; set; }
        public ExtraContent? Content { get; set; }
        public string SortByDate { get; set; }


    }
}

