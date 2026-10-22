namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMyTrips
{
    public class Leg
    {
        public int LegId { get; set; }
        public int Ordinal { get; set; }
        public IEnumerable<Carrier>? Carrier { get; set; }
        public string? FlightNum { get; set; }
        public TransportOrigin? TransportOrigin { get; set; }
        public TransportDestination? TransportDestination { get; set; }
        public TransportEquipment? TransportEquipment { get; set; }
        public string? OnAirDuration { get; set; }
        public string? Status { get; set; }
        public CabinClass? CabinClass { get; set; }
        public string? SeatSelectionType { get; set; }
        public bool SameDayArrival { get; set; }
        public string? MealServiceCode { get; set; }
        public int DistanceMiles { get; set; }
        public bool International { get; set; }
        public string? OperationalSuffix { get; set; }   
        public string? OnGroundDuration { get; set; }
    }


}
