using System;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Awards
{
    public class AwardAuditDTO
    {
        public string Id { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CustomerId { get; set; }
        public string AwardUpdatedBy { get; set; }
        public DateTime? AwardUpdatedOn { get; set; }
        public string SourceIdentifier { get; set; }
        public string AwardNumber { get; set; }
        public string AwardName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; }
        public DateTime? UsedDate { get; set; }
        public string AwardId { get; set; }
        public string AwardProductId { get; set; }
        public DateTime? GiftedAwardDate { get; set; }
        public string GiftedFromCustomerId { get; set; }
        public string GiftedToCustomerId { get; set; }
        public string PreviousStatus { get; set; }
        public string GiftedFromMemberNumber { get; set; }
        public string GiftedToMemberNumber { get; set; }

        public static explicit operator AwardAuditDTO(Award award)
        {
            return new()
            {
                Id = award.Id,
                CreatedBy = award.CreatedBy,
                AwardUpdatedBy = award.LastUpdatedBy ?? award.CreatedBy,
                SourceIdentifier = award.SourceIdentifier ?? string.Empty,
                CustomerId = award.CustomerId,
                StartDate = award.StartDate != null ? Convert.ToDateTime(Convert.ToDateTime(award.StartDate).ToString(Constants.Application.ISODateTimeFormat)) : null,
                AwardUpdatedOn = award.LastUpdatedOn,
                AwardNumber = award.AwardNumber,
                EndDate = award.ExpiryDate,
                Status = award.Status,
                UsedDate = award.UsedDate,
                AwardId = award.AwardId,
                AwardProductId = award.ProductId,
                GiftedAwardDate = award.GiftedAwardDate,
                GiftedFromCustomerId = award.GiftedFromCustomerId,
                GiftedToCustomerId = award.GiftedToCustomerId,
                PreviousStatus = award.PreviousStatus,
                GiftedFromMemberNumber = award.GiftedFromCustomerId,
                GiftedToMemberNumber = award.GiftedToCustomerId
            };
        }
    }
}
