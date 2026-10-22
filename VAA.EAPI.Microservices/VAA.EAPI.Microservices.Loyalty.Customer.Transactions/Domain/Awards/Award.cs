using System;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Base;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Awards
{
    public class Award : CustomerTransaction
    {
        public string AwardNumber { get; set; }
        public string AwardName { get; set; }
        public string ProductId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int AwardValue { get; set; } = 0;
        public string TransactionId { get; set; }
        public DateTime? UsedDate { get; set; }
        public string AwardBasis { get; set; }
        public string AwardId { get; set; }
        public string Status { get; set; }
        public int Quantity { get; set; } = 0;
        public string ExpiryDateUpdatedBy { get; set; }
        public DateTime? GiftedAwardDate { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public string GiftedFromCustomerId { get; set; }
        public string GiftedToCustomerId { get; set; }
        public string PreviousStatus { get; set; }

        public Award()
        {
            // Required for Serialization
        }
        public Award(AwardDTO dto)
        {
            Id = dto.Id;
            CustomerId = dto.CustomerId;
            CreatedOn = Common.Helpers.DateTimeHelper.ParseDateFormat(dto.CreatedOn) ?? DateTime.Parse(dto.CreatedOn);
            CreatedBy = dto.CreatedBy;
            LastUpdatedOn = DateTime.Parse(dto.LastUpdatedOn);
            LastUpdatedBy = dto.LastUpdatedBy;
            AwardId = dto.AwardId;
            AwardNumber = dto.AwardNumber;
            ProductId = dto.ProductId;
            StartDate = string.IsNullOrEmpty(dto.StartDate) ? null : DateTime.Parse(dto.StartDate);
            ExpiryDate = string.IsNullOrEmpty(dto.ExpiryDate) ? null : DateTime.Parse(dto.ExpiryDate);
            Status = dto.Status;
            TransactionId = dto.TransactionId;
            UsedDate = string.IsNullOrEmpty(dto.UsedDate) ? null : DateTime.Parse(dto.UsedDate);
            AwardBasis = dto.AwardBasis;
            Quantity = !string.IsNullOrEmpty(dto.Quantity) ? (int)Convert.ToDouble(dto.Quantity) : 0;
            AwardValue = !string.IsNullOrEmpty(dto.AwardValue) ? (int)Convert.ToDouble(dto.AwardValue) : 0;
            ExpiryDateUpdatedBy = dto.ExpiryDateUpdatedBy;
            GiftedAwardDate = string.IsNullOrEmpty(dto.GiftedAwardDate) ? null : DateTime.Parse(dto.GiftedAwardDate);
            SourceIdentifier = dto.SourceIdentifier;
            SourceSystem = dto.SourceSystem;
            GiftedFromCustomerId = dto.GiftedFromCustomerId;
            GiftedToCustomerId = dto.GiftedToCustomerId;
            PreviousStatus = dto.PreviousStatus;
        }

    }
}


