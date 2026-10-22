namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PassengerDetails
{
    public class LoyaltyProgram
    {
        public string Name { get; set; }
        public IEnumerable<Carrier> Owner { get; set; }
    }
}