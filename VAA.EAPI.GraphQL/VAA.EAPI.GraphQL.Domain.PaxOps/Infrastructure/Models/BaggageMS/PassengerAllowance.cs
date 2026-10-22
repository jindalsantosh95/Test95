namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.BaggageMS
{
    public class PassengerAllowance
    {
        public string GivenNames { get; set; }
        public string Surname { get; set; }
        public string PassengerNiN { get; set; }
        public bool IsInfantPassenger { get; set; }
        public BaggageAdvisory? BaggageAdvisory { get; set; }
        public int NumSelectedBags { get; set; }
        public bool CanChangeBags { get; set; }
        public IEnumerable<BaggageOffer> BaggageOffers { get; set; }
    }
}
