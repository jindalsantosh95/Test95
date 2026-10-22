namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.TripsCore
{
    public class Seat
    {
        public int Row { get; set; }
        public string Column { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public IEnumerable<CabinClass> ActualCabinClass { get; set; }
    }
}
