namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SSRMS
{
    public class Passenger
    {
        public string? PassengerResourceId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public IEnumerable<SsrSegment>? SsrSegments { get; set; }
    }
}
