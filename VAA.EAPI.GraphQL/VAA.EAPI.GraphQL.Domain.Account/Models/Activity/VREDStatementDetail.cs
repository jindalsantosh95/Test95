namespace VAA.EAPI.GraphQL.Domain.Account.Models.Activity
{
    public class Detail
    {
        public string TransactionId { get; set; }
        public string PartnerName { get; set; }
        public string PartnerCode { get; set; }
        public string PartnerSubtype { get; set; }
        public bool IsFlight { get; set; }
        public DateTime? ActivityDatetime { get; set; }
        public string ActivitySubtype { get; set; }
        public string ActivityType { get; set; }
        public string Comments { get; set; }
        public string CreatedBy { get; set; }
        public string MembershipNumber { get; set; }
        public string Owner { get; set; }
        public string Points { get; set; }
        public string ProductName { get; set; }
        public string StatementText { get; set; }
        public string TransactionChannelId { get; set; }
        public string TransactionStatus { get; set; }
        public DateTime? TransactionDatetime { get; set; }
    }
}