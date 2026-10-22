namespace VAA.EAPI.Microservices.CRM.Refunds.Models.Refunds
{
    public class RefundRequestForm
    {
        public string RequestId { get; set; } = string.Empty;
        public RefundRequestRequester Requester { get; init; } = new RefundRequestRequester();
        public IReadOnlyCollection<RefundRequestItem> Entries { get; init; } = Array.Empty<RefundRequestItem>();
    }
}
