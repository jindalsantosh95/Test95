namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMyTrips
{ 
    public class Passenger
    {
        public string? PassengerResourceId { get; set; }
        public bool GroupLeader { get; set; }
        public string? GivenNames { get; set; }
        public string? Surname { get; set; }
        public string? FirstNameNum { get; set; }
        public string? LastNameNum { get; set; }
        public string? CustomerId { get; set; }
        public Passenger? Infant { get; set; }
        public PassengerType? PassengerType { get; set; }
        public IEnumerable<Ticket>? Tickets { get; set; }
        public IEnumerable<PassengerTrip>? PassengerTrips { get; set; }
        public IEnumerable<LoyaltyProgramAccount>? LoyaltyProgramAccounts { get; set; }
    }
}