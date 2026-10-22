namespace VAA.EAPI.GraphQL.Domain.WebForms.Models.Refunds
{
    public class RefundForm
    {
        public string RequestId { get; set; } = string.Empty;
        public RefundRequester Requester { get; set; } = new RefundRequester();
        public IEnumerable<RefundItem> Entries { get; set; } = new List<RefundItem>();
    }
}
