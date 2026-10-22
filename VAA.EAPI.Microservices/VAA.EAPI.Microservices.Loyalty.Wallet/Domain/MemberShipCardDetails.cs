namespace VAA.EAPI.Microservices.Loyalty.Wallet.Domain
{
    public class MemberShipCardDetails
    {
        public string CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public string Active { get; set; }
        public string ExpiryDate { get; set; }
        public string IssueDate { get; set; }
        public string MemberNumber { get; set; }
        public string TierCode { get; set; }
        public string CardExpiryDate { get; set; }
    }
}
