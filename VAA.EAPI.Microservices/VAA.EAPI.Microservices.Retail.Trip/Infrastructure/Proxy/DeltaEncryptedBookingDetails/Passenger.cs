namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaEncryptedBookingDetails
{
    public class Passenger
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FirstNameNum { get; set; }
        public string LastNameNum { get; set; }
        public string PassengerResourceId { get; set; }
        public IEnumerable<Ticket> Tickets { get; set; }
    }
}