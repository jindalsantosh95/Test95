using System;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities
{
    public class AwardDocumentData
    {
        public string SchemaVersion { get; set; }
        public string DocType { get; set; }
        public string PartitionKey { get; set; }
        public string CustomerId { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public string AwardNumber { get; set; }
        public string ProductId { get; set; }
        public DateTime StartDate { get; set; }
        public string ExpiryDate { get; set; }
        public string Status { get; set; }
        public string TransactionId { get; set; }
        public string UsedDate { get; set; }
        public string AwardBasis { get; set; }
        public long Quantity { get; set; }
        public long AwardValue { get; set; }
        public string ExpiryDateUpdatedBy { get; set; }
        public string GiftedAwardDate { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public string Brand { get; set; }
        public string GiftedFromCustomerId { get; set; }
        public string GiftedToCustomerId { get; set; }
        public string PreviousStatus { get; set; }
        public string id { get; set; }
        public string AwardId { get; set; }
    }
}
