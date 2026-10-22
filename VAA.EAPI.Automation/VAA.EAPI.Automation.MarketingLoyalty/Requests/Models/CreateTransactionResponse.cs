using static VAA.EAPI.Automation.MarketingLoyalty.Constants.Application;

namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Models
{
    public class CreateTransactionResponse
    {
        public string memberNumber { get; set; }
        public string customerId { get; set; }
        public string transactionId { get; set; }
        public string transactionType { get; set; }
        public string transactionStatus { get; set; }
        public string partnerId { get; set; }
        public TransactionProduct transactionProduct { get; set; }
    }
}
