using System;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Base;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities
{
    public class AwardAudit : LoyaltyAudit
    {
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
    }
}