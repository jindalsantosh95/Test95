namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMyTrips
{
    public class Reservation
    {
        public string Type { get; set; }
        public IEnumerable<BookingReference> BookingReferences { get; set; }
        public DateTime BookingCreationDt { get; set; }
        public string TicketExpirationDt { get; set; }
        public bool CheckinEligible { get; set; }
        public string TripType { get; set; }
        public bool International { get; set; }
        public bool StandbyEligible { get; set; }
    }


}
