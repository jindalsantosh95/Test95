namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMessageCentre
{
    public class Passenger
    {
        public string Id { get; set; }
        public TitleType Title { get; set; }
        public string GivenNames { get; set; }
        public string Surname { get; set; }
        public string Suffix { get; set; }
        public Passenger Infant { get; set; }
        public IEnumerable<LoyaltyProgramAccount> LoyaltyProgramAccounts { get; set; }
        public IEnumerable<Affiliation> Affiliations { get; set; }
    }
}
