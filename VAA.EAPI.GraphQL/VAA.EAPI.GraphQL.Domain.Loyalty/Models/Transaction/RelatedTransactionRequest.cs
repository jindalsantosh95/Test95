
namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction
{
    public class RelatedTransactionRequest
    {
        public string CustomerId { get; set; }
        public string MemberNumber { get; set; }
        public string TransactionId { get; set; }
    }
}
