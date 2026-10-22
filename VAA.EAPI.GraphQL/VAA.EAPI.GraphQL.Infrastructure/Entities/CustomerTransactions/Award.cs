using System;

namespace VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions
{
    public class Award
    {
        public string AwardNumber { get; set; }
        public string ProductId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public int AwardValue { get; set; } = 0;
        public string TransactionId { get; set; }
        public DateTime UsedDate { get; set; }
        public string AwardBasis { get; set; }
        public string AwardId { get; set; }
        public string Status { get; set; }
        public int Quantity { get; set; } = 0;
        public string ExpiryDateUpdatedBy { get; set; }
        public DateTime GiftedAwardDate { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public string GiftedFromCustomerId { get; set; }
        public string GiftedToCustomerId { get; set; }
        public string PreviousStatus { get; set; }
    }
}
