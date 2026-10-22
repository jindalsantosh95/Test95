namespace VAA.EAPI.GraphQL.Domain.Book.Models.Common
{
    public class PenaltyRef
    {
        public IEnumerable<string> ChangePenaltyRefIds { get; set; }
        public IEnumerable<string> CancellationPenaltyRefIds { get; set; }
    }
}
