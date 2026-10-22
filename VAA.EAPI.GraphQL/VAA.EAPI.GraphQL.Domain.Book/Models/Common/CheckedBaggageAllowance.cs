namespace VAA.EAPI.GraphQL.Domain.Book.Models.Common
{
    public class CheckedBaggageAllowance
    {
        public IEnumerable<string> FlightNum { get; set; }
        public IEnumerable<string> BaggageRefId { get; set; }
    }
}
