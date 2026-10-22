namespace VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer
{
    public class SearchOriginDestination
    {
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string DepartureDate { get; set; }
        public DepartureTimeRange? DepartureTimeRange { get; set; }
        public IEnumerable<string>? ConnectionAirports { get; set; }

        public static explicit operator SearchOriginDestination(Basket.SearchOriginDestination source)
        {
            if (source == null)
                return null;

            return new()
            {
                Origin = source.Origin,
                Destination = source.Destination,
                DepartureDate = source.DepartureDate,
                DepartureTimeRange = (DepartureTimeRange)source.DepartureTimeRange,
                ConnectionAirports = source.ConnectionAirports,
            };
        }
    }
}