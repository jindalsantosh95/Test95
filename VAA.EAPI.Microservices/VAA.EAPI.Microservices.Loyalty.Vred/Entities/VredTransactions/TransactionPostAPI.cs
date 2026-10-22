namespace VAA.EAPI.Microservices.Loyalty.Vred.Entities.VredTransactions
{
    public class TransactionPostAPI
    {
        public string requestId { get; set; }
        public string partnerCode { get; set; }
        public string partnerTransactionId { get; set; }
        public string partnerUserIdDomain { get; set; }
        public string partnerUserId { get; set; }
        public string dateTime { get; set; }
        public string type { get; set; }
        public int points { get; set; }
        public string activityDateTime { get; set; }
        public string description { get; set; }
        public string callbackUrl { get; set; }
        public string details { get; set; }
        public string transactionPartnerCode { get; set; }
        public string lockedPointsId { get; set; }
    }
}