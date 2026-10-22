namespace VAA.EAPI.GraphQL.Domain.Search.Models.Brand
{
    public class Fees
    {
        public IEnumerable<ChangePolicy>? Change { get; set; }
        public IEnumerable<CancelPolicy>? Cancellation { get; set; }
    }
}