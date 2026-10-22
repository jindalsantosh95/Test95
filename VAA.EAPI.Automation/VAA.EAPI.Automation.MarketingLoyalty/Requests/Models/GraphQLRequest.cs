namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Models
{
    public class GraphQLRequest
    {
        public string query { get; set; }

        public dynamic variables { get; set; }

    }

    public class RetrieveRelatedTransactionsVariables
    {
        public Relatedtransactionrequest relatedTransactionRequest { get; set; }
    }

    public class Relatedtransactionrequest
    {
        public string customerId { get; set; }
        public string memberNumber { get; set; }
        public string transactionId { get; set; }
    }

    public class CancelRelatedTransactionsVariables
    {
        public Canceltransactionrequest cancelTransactionRequest { get; set; }
    }

    public class Canceltransactionrequest
    {
        public IEnumerable<Canceltransactiondetail> cancelTransactionDetails { get; set; }
        public string cancellationReason { get; set; }
        public string customerId { get; set; }
        public string initiatedBy { get; set; }
        public string initiatedTransactionId { get; set; }
        public string memberNumber { get; set; }
        public string transactionSource { get; set; }
    }

    public class Canceltransactiondetail
    {
        public string partnerId { get; set; }
        public string partnerName { get; set; }
        public string productId { get; set; }
        public string productName { get; set; }
        public string transactionId { get; set; }
        public string transactionStatus { get; set; }
        public string transctionType { get; set; }
    }

}


