namespace VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer
{
    public class PenaltyRef
    {
        public IEnumerable<string> ChangePenaltyRefIds { get; set; }
        public IEnumerable<string> CancellationPenaltyRefIds { get; set; }
    }
}
