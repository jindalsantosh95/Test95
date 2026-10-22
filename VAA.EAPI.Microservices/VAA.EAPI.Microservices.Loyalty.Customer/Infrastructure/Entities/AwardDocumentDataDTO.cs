using System;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities
{
    public class AwardDocumentDataDTO : CosmosDocument
    {
        public string SchemaVersion { get; set; } = Constants.Application.SchemaVersion;
        public string DocType { get; set; }
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
        public string AwardId { get; set; }

        public AwardDocumentDataDTO()
        {
            //required for serializtion
        }

        public AwardDocumentDataDTO(AwardDocumentData awardDocument)
        {

            if (awardDocument == null) throw new ArgumentNullException(nameof(awardDocument));

            Id = awardDocument.id;
            PartitionKey = awardDocument.PartitionKey;
            DocType = awardDocument.DocType ?? string.Empty;
            CustomerId = awardDocument.CustomerId ?? string.Empty;
            CreatedOn = awardDocument.CreatedOn;
            CreatedBy = awardDocument.CreatedBy;
            LastUpdatedOn = awardDocument.LastUpdatedOn;
            LastUpdatedBy = awardDocument.LastUpdatedBy ?? string.Empty;
            AwardNumber = awardDocument.AwardNumber ?? string.Empty;
            ProductId = awardDocument.ProductId ?? string.Empty;
            StartDate = awardDocument.StartDate;
            ExpiryDate = awardDocument.ExpiryDate ?? string.Empty;
            Status = awardDocument.Status ?? string.Empty;
            TransactionId = awardDocument.TransactionId ?? string.Empty;
            UsedDate = awardDocument.UsedDate ?? string.Empty;
            AwardBasis = awardDocument.AwardBasis ?? string.Empty;
            Quantity = awardDocument.Quantity;
            AwardValue = awardDocument.AwardValue;
            ExpiryDateUpdatedBy = awardDocument.ExpiryDateUpdatedBy ?? string.Empty;
            GiftedAwardDate = awardDocument.GiftedAwardDate ?? string.Empty;
            SourceIdentifier = awardDocument.SourceIdentifier ?? string.Empty;
            SourceSystem = awardDocument.SourceSystem ?? string.Empty;
            Brand = awardDocument.Brand ?? string.Empty;
            GiftedFromCustomerId = awardDocument.GiftedFromCustomerId ?? string.Empty;
            GiftedToCustomerId = awardDocument.GiftedToCustomerId ?? string.Empty;
            PreviousStatus = awardDocument.PreviousStatus ?? string.Empty;
            AwardId = awardDocument.AwardId ?? string.Empty;


        }
    }

}


