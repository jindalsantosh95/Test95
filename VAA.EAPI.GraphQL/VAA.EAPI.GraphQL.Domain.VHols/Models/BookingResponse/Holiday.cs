namespace VAA.EAPI.GraphQL.Domain.VHols.Models.BookingResponse
{
    public class Holiday
    {
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public string? PackageType { get; set; }
        public List<Errata> Errata { get; set; }
        //public List<HolidayComponent> HolidayComponents { get; set; }
        public List<GroupedHolidayComponent> HolidayComponentsByDate { get; set; }
        public List<Hotel> Hotels { get; set; }
        public List<Extra> Extras { get; set; }
        public List<Excursion> Excursions { get; set; }
        public List<Tour> Tours { get; set; }
        public List<Flight> Flights { get; set; }
        public List<Car> Cars { get; set; }
        public List<Transfer> Transfers { get; set; }
        public DisneyContent DisneyContent { get; set; }
    }
}

