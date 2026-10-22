namespace VAA.EAPI.Microservices.Retail.Trip.Domain.TripDetails
{
    public class TripListResponseItem
    {
        public TripsDetailsRequest TripSearchRequest { get; set; }
        public TripsDetailsResponse TripDetails { get; set; }
    }
}