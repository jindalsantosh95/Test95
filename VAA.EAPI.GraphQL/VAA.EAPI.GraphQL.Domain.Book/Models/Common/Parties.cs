namespace VAA.EAPI.GraphQL.Domain.Book.Models.Common
{
    public class Parties
    {
        public IEnumerable<Participant> Participants { get; set; }
        public CorporateAccount CorporateAccount { get; set; }
    }

}