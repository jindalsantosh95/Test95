namespace VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer
{
    public class DepartureTimeRange
    {
        public string EarliestTime { get; set; }
        public string LatestTime { get; set; }

        public static explicit operator DepartureTimeRange(Basket.DepartureTimeRange source)
        {
            if (source == null)
                return null;

            return new()
            {
                EarliestTime = source.EarliestTime,
                LatestTime = source.LatestTime,
            };
        }
    }
}
