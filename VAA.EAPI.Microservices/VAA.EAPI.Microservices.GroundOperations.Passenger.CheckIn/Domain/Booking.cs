namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Domain
{
    public class Booking
    {
        public string Status { get; set; }
        public string BookingId { get; set; }
        public Pos Pos { get; set; }
        public IEnumerable<Product> Product { get; set; }
        public IEnumerable<Passenger> Passengers { get; set; }
    }
}