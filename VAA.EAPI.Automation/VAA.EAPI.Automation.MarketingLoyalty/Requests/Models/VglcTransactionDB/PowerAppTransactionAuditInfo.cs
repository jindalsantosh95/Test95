namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.VglcTransactionDB
{
    public class PowerAppTransactionAuditInfo
    {
        public int AuditId { get; set; }
        public string MembershipNumber { get; set; }
        public string TransactionID { get; set; }
        public string TransactionType { get; set; }
        public string ActivityDate { get; set; }
        public string Status { get; set; }
        public string TransactionProcessedDate { get; set; }
        public string TransactionSource { get; set; }
        public string ServiceRequest { get; set; }
        public object? SiebelRequest { get; set; }
        public object? SiebelResponse { get; set; }
        public string VredRequest { get; set; }
        public string VredResponse { get; set; }
        public DateTime? Created { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedByEmailId { get; set; }
        public string CreatedByUserName { get; set; }

    }
}
