namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    public class TripListResponseItem
    {
        public TripsDetailsRequest TripSearchRequest { get; set; }
        public TripsDetailsResponse TripDetails { get; set; }
    }
}