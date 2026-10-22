namespace VAA.EAPI.Microservices.Retail.Trip.Domain.TripDetails
{
    public class IropProtect
    {
        public bool? Protect {  get; set; }
        public string? TripId { get; set; }
        public string? IropTripId { get; set; }

        public static explicit operator IropProtect(Infrastructure.Proxy.DeltaMyTrips.IropProtect source)
        {
            return new()
            {
                Protect = source?.Protect,
                TripId = source?.TripId.ToString(),
                IropTripId = source?.IropTripId
            };
        }
    }
}