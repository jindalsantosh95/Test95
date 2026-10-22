namespace VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer
{
    public class Parties
    {
        public IEnumerable<Participant> Participants { get; set; }
        public CorporateAccount CorporateAccount { get; set; }
    }
}
