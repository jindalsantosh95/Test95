using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Awards;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities
{
    public class AwardDTO : CosmosDocument
    {
        public string SchemaVersion = Constants.Application.AwardDocSchemaVersion;
        public string Brand = Constants.Application.AwardBrand;
        public string DocType = ConfigReader.Get(EnvironmentSettings.AwardDocType);
        public string CustomerId = string.Empty;
        public string CreatedOn = DateTime.UtcNow.Date.ToString(Constants.Application.ISODateTimeFormat);
        public string CreatedBy = string.Empty;
        public string LastUpdatedOn = DateTime.UtcNow.Date.ToString(Constants.Application.ISODateTimeFormat);
        public string LastUpdatedBy = string.Empty;
        public string AwardId = string.Empty;
        public string AwardNumber = string.Empty;
        public string ProductId = string.Empty;
        public string StartDate = DateTime.UtcNow.Date.ToString(Constants.Application.ISODateTimeFormat);
        public string ExpiryDate = string.Empty;
        public string Status = string.Empty;
        public string TransactionId = string.Empty;
        public string UsedDate = string.Empty;
        public string AwardBasis = string.Empty;
        public string Quantity = "0";
        public string AwardValue = "0";
        public string ExpiryDateUpdatedBy = string.Empty;
        public string GiftedAwardDate = string.Empty;
        public string SourceIdentifier = string.Empty;
        public string SourceSystem = string.Empty;
        public string GiftedFromCustomerId = string.Empty;
        public string GiftedToCustomerId = string.Empty;
        public string PreviousStatus = string.Empty;

        public AwardDTO()
        {
            // Required for serialisation
        }
        public AwardDTO(Award award)
        {
            Id = award.Id;
            PartitionKey = award.CustomerId;
            CustomerId = award.CustomerId;
            CreatedOn = award.CreatedOn == default ? DateTime.UtcNow.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat) : award.CreatedOn.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat);
            CreatedBy = award.CreatedBy;
            LastUpdatedOn = award.LastUpdatedOn == default ? DateTime.UtcNow.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat) : award.LastUpdatedOn.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat);
            LastUpdatedBy = award.LastUpdatedBy;

            AwardId = string.IsNullOrEmpty(award.AwardId) ? Utility.GetId() : award.AwardId;
            AwardNumber = award.AwardNumber ?? string.Empty;
            AwardBasis = award.AwardBasis ?? string.Empty;
            ProductId = award.ProductId;
            StartDate = award.StartDate?.ToString(Constants.Application.ISODateTimeFormat) ?? string.Empty;
            ExpiryDate = award.ExpiryDate?.ToString(Constants.Application.ISODateTimeFormat) ?? string.Empty;
            Status = award.Status ?? string.Empty;
            TransactionId = award.TransactionId ?? string.Empty;
            UsedDate = award.UsedDate?.ToString(Constants.Application.ISODateTimeFormat) ?? string.Empty;
            Quantity = award.Quantity.ToString();
            AwardValue = award.AwardValue.ToString();
            ExpiryDateUpdatedBy = award.ExpiryDateUpdatedBy ?? string.Empty;
            GiftedAwardDate = award.GiftedAwardDate?.ToString(Constants.Application.ISODateTimeFormat) ?? string.Empty;
            GiftedFromCustomerId = award.GiftedFromCustomerId ?? string.Empty;
            GiftedToCustomerId = award.GiftedToCustomerId ?? string.Empty;
            PreviousStatus = award.PreviousStatus ?? string.Empty;
        }

        public Award Award()
        {
            return new Award(this);
        }
    }
}
