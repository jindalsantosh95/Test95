
namespace VAA.EAPI.GraphQL.Domain.Account.Models.Transaction
{
    public class CancelTransactionMutation
    {
        public string TransactionId { get; set; }
        public string MemberNumber { get; set; }
        public DateTime? ActivityDate { get; set; }
        public string CancellationReason { get; set; }
        public string Channel { get; set; }

    }
}
