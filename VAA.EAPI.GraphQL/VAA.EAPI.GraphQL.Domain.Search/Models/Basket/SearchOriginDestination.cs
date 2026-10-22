namespace VAA.EAPI.GraphQL.Domain.Search.Models.Basket
{
    public class SearchOriginDestination
    {
        public string Origin { get; set; }
        public string Destination { get; set; }
        public string DepartureDate { get; set; }
        public DepartureTimeRange DepartureTimeRange { get; set; }
        public IEnumerable<string> ConnectionAirports { get; set; }


        public static explicit operator SearchOriginDestination(FlightOffer.SearchOriginDestination source)
        {
            if (source == null)
                return null;

            return new()
            {
                DepartureDate = source.DepartureDate,
                Origin = source.Origin,
                Destination = source.Destination,
                DepartureTimeRange = (DepartureTimeRange)source.DepartureTimeRange,
                ConnectionAirports = source.ConnectionAirports

            };
        }
    }
}