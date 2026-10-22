namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction
{
    public class VredCancelTransactionRequest
    {
        public string TransactionId { get; set; }
        public string CancellationReason { get; set; }
        public string MemberNumber { get; set; }
        public string ActivityDate { get; set; }
        public string TransactionChannel { get; set; }
        public string CreatedBy { get; set; }
        public string UpdatedBy { get; set; }
        public string CompensatoryTransactionId { get; set; }
    }
}
