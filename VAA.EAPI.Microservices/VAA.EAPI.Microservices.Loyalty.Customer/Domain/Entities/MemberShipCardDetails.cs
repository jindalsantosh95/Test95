using System;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities
{
    public class MemberShipCardDetails
    {
        public string Active { get; set; }
        public DateTime? CardExpiryDate { get; set; }
        public string CardNumber { get; set; }
        public string CardReplacementReason { get; set; }
        public string CardStatusCode { get; set; }
        public string CardType { get; set; }
        public string Description { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public DateTime? IssueDate { get; set; }
        public string MemberNumber { get; set; }
        public string NameOnCard { get; set; }
        public DateTime? RequestedDate { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public DateTime? StartDate { get; set; }
        public string TierCode { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public string CardId { get; set; }
    }
}
