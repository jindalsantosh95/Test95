namespace VAA.EAPI.GraphQL.Domain.VHols.Models.BookingResponse
{
    public class GroupedHolidayComponent
    {
        public string Date { get; set; }
        public List<IHolidayComponent> HolidayComponents { get; set; }
    }
}

