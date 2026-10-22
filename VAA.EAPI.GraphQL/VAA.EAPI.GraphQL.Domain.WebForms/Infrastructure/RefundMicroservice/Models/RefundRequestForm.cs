using VAA.EAPI.GraphQL.Domain.WebForms.Models.Refunds;

namespace VAA.EAPI.GraphQL.Domain.WebForms.Infrastructure.RefundMicroservice.Models
{
    public class RefundRequestForm
    {
        public string RequestId { get; set; } = string.Empty;
        public RefundRequestRequester Requester { get; set; } = new RefundRequestRequester();
        public IReadOnlyCollection<RefundRequestItem> Entries { get; set; } = Array.Empty<RefundRequestItem>();


        // Map the GraphQL domain model to the Refund Microservice request model
        public static explicit operator RefundRequestForm(RefundForm form)
        {
            return new RefundRequestForm
            {
                RequestId = form.RequestId,
                Requester = (RefundRequestRequester)form.Requester,
                Entries = form.Entries.Select(entry => (RefundRequestItem)entry).ToList()
            };
        }
    }
}
