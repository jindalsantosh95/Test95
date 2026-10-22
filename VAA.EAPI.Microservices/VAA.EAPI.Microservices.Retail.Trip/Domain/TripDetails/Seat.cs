namespace VAA.EAPI.Microservices.Retail.Trip.Domain.TripDetails
{
    public class Seat
    {
        public int Row { get; set; }
        public string? Column { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }
        public IEnumerable<CabinClass>? ActualCabinClass { get; set; }

        public static explicit operator Seat(Infrastructure.Proxy.DeltaMyTrips.Seat source)
        {
            return new Seat
            {
                Column = source.Column,
                Name = source.Number,
                Row = source.SeatRow.Number,
                Type = source.Column,
                ActualCabinClass = source.ActualCabinClass?.Select(c => new CabinClass() { Code = c.Code, ProductCode = c.ProductCode })
            };
        }
    }
}
