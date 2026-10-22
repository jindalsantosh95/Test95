namespace VAA.EAPI.GraphQL.Domain.Search.Models.Holiday
{
    public class HolidaySearchResponse
    {
        [UseSorting]
        [UseFiltering]
        public IEnumerable<HolidayCJS> Holidays { get; set; } = Enumerable.Empty<HolidayCJS>();

        public IEnumerable<Facet> Facet { get; set; } = Enumerable.Empty<Facet>();

        public bool HasWidened { get; set; }

        public IEnumerable<DateWidened> DateWidened { get; set; } = Enumerable.Empty<DateWidened>();

        public HolidayDestination? Destination { get; set; }

        public HolidaySearchResponse()
        {

        }
    }
}
