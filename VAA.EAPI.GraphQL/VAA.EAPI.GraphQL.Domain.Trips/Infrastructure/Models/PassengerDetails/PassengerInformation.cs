namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PassengerDetails
{
    public class PassengerDetailsRequest
    {
        public string bookingId { get; set; }
        public IEnumerable<Passenger> Passengers { get; set; }

    }
}