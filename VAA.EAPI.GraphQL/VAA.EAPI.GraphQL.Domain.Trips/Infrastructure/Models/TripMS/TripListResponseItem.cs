namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS
{
    public class TripListResponseItem
    {
        public TripsDetailsRequest TripSearchRequest { get; set; }
        public TripsDetailsResponse TripDetails { get; set; }
    }
}
