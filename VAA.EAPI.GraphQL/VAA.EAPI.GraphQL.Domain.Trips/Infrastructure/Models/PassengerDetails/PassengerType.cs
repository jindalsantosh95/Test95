namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PassengerDetails
{
    public class PassengerType
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public PassengerAgeType Category { get; set; }
    }
}