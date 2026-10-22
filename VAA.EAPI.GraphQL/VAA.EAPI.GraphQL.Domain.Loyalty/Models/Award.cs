using System;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class Award
    {
        public string id { get; set; }
        public string AwardNumber { get; set; }
        public string AwardName { get; set; }
        public string ProductId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string TransactionId { get; set; }
        public string AwardBasis { get; set; }
        public string AwardId { get; set; }
        public string Status { get; set; }
        public string ExpiryDateUpdatedBy { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public DateTime? GiftedAwardDate { get; set; }
        public string GiftedFromCustomerId { get; set; }
        public string GiftedToCustomerId { get; set; }
        public string PreviousStatus { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string CustomerId { get; set; }
    }
}