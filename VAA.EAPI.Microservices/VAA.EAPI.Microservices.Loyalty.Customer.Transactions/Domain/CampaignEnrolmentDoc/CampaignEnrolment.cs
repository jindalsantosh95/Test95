using System;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Base;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.CampaignEnrolmentDoc
{
    public class CampaignEnrolment : CustomerTransaction
    {
        public string MemberNumber { get; set; }
        public string Comments { get; set; }
        public string Eligible { get; set; }
        public DateTime? OptInDate { get; set; }
        public DateTime? EnrollmentExpiryDate { get; set; }
        public string EnrollmentId { get; set; }
        public string PromotionId { get; set; }
        public CampaignEnrolment()
        {
            // Required for Serialization
        }
        public CampaignEnrolment(CampaignEnrolmentDTO dto)
        {
            Id = dto.Id;
            CreatedBy = dto.CreatedBy;
            CreatedOn = DateTime.Parse(dto.CreatedOn);
            LastUpdatedBy = dto.LastUpdatedBy;
            LastUpdatedOn = DateTime.Parse(dto.LastUpdatedOn);
            CustomerId = dto.CustomerId;

            MemberNumber = dto.MemberNumber;
            OptInDate = string.IsNullOrEmpty(dto.OptInDate) ? null : DateTime.Parse(dto.OptInDate);
            EnrollmentExpiryDate = string.IsNullOrEmpty(dto.EnrollmentExpiryDate) ? null : DateTime.Parse(dto.EnrollmentExpiryDate);
            Eligible = dto.Eligible;
            EnrollmentId = dto.EnrollmentId;
            PromotionId = dto.PromotionId;
        }

    }
}
