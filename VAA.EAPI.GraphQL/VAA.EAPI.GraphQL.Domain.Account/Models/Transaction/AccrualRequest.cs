
namespace VAA.EAPI.GraphQL.Domain.Account.Models.Transaction
{
    public class AccrualRequest
    {
        public string CustomerId { get; set; }
        public string MemberNumber { get; set; }
        public string PartnerCode { get; set; }
        public string ProductName { get; set; }
        public string ActivityDate { get; set; }
        public string Comments { get; set; }
        public int Points { get; set; }
        public int TierPoints { get; set; }
        public string TransactionSource { get; set; }
        public string CreatedByEmailId { get; set; }
        public string CreatedByUserName { get; set; }
    }
}
