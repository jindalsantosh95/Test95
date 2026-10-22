namespace VAA.EAPI.GraphQL.Domain.Account.Models.Transaction
{
    public class TransactionMutationRequest
    {
        public int Points { get; set; }
        public string OrderId { get; set; }
        public DateTime? ActivityDate { get; set; }
        public string Channel { get; set; }
        public string Comment { get; set; }
        public string MemberNumber { get; set; }
    }
}
