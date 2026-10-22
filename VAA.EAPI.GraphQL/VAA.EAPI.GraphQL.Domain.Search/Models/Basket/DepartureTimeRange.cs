namespace VAA.EAPI.GraphQL.Domain.Search.Models.Basket
{
    public class DepartureTimeRange
    {
        public string EarliestTime { get; set; }
        public string LatestTime { get; set; }


        public static explicit operator DepartureTimeRange(FlightOffer.DepartureTimeRange source)
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