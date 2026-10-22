using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Models
{
    public class RetrieveRelatedTxnGqlResponse
    {
        public Data data { get; set; }
    }

    public class Data
    {
        public Loyaltyrelatedtransactions loyaltyRelatedTransactions { get; set; }
    }

    public class Loyaltyrelatedtransactions
    {
        public string customerId { get; set; }
        public TransactionDocuments[] transactionDocuments { get; set; }
    }

    public class TransactionDocuments
    {
        public string transactionType { get; set; }
        public string statusCode { get; set; }
        public string transactionId { get; set; }
        public string productName { get; set; }
        public string partnerId { get; set; }
        public string partnerName { get; set; }
        public string productId { get; set; }
        public string awardId { get; set; }
        public string cancelledTransactionId { get; set; }
        public string transactionSubType { get; set; }
        public FlightDetails flightDetails { get; set; }
    }
    public class FlightDetails
    {
        public string externalReservationNumber { get; set; }
    }

}


