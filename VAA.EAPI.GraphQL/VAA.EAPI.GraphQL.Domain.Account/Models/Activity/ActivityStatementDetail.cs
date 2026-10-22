namespace VAA.EAPI.GraphQL.Domain.Account.Models.Activity
{
    public class ActivityStatementDetail
    {
        public string TransactionId { get; set; }
        public string TransactionStatus { get; set; }
        public string TransactionType { get; set; }
        public string TransactionSubType { get; set; }
        public DateTime TransactionProcessedDate { get; set; }
        public DateTime ActivityDate { get; set; }
        public string TotalPoints { get; set; }
        public string ProductName { get; set; }
        public string PartnerName { get; set; }
        public string PartnerCode { get; set; }
        public string PartnerType { get; set; }
        public string Comments { get; set; }
        public string StatementDescription { get; set; }
        public string PartnerSubType { get; set; }
        public Point Point { get; set; }
    }
}
