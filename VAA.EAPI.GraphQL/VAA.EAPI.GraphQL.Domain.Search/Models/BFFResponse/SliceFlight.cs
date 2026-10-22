using NodaTime;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFResponse
{
    public class SliceFlight
    {
        public IEnumerable<Segment> Segments { get; set; }
        public string Duration { get; set; }
        public Airport Origin => Segments.FirstOrDefault()?.origin;
        public Airport Destination => Segments.LastOrDefault()?.destination;
        public LocalDateTime? Departure => Segments.FirstOrDefault()?.departure;
        public LocalDateTime? Arrival => Segments.LastOrDefault()?.arrival;
        public IEnumerable<Link>? Links { get; set; }
    }
}
