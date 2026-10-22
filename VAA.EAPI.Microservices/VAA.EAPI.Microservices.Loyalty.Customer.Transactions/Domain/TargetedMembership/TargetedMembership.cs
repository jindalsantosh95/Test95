using System;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Base;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.TargetedMembership
{
    public class TargetedMembership : CustomerTransaction
    {
        public DateTime? AssignmentDate { get; set; }
        public string Comments { get; set; }
        public DateTime? CompletedDate { get; set; }
        public DateTime? ContactedDate { get; set; }
        public string ProfilingCampaignSourceId { get; set; }
        public string StatusCode { get; set; }
        public string TargetedMembershipId { get; set; }

        public TargetedMembership()
        {
            // Required for Serialization
        }
        public TargetedMembership(TargetedMembershipDTO dto)
        {
            Id = dto.Id;
            CreatedBy = dto.CreatedBy;
            CreatedOn = DateTime.Parse(dto.CreatedOn);
            LastUpdatedBy = dto.LastUpdatedBy;
            LastUpdatedOn = DateTime.Parse(dto.LastUpdatedOn);
            CustomerId = dto.CustomerId;

            AssignmentDate = string.IsNullOrEmpty(dto.AssignmentDate) ? null : DateTime.Parse(dto.AssignmentDate);
            Comments = dto.Comments;
            CompletedDate = string.IsNullOrEmpty(dto.CompletedDate) ? null : DateTime.Parse(dto.CompletedDate);
            ContactedDate = string.IsNullOrEmpty(dto.ContactedDate) ? null : DateTime.Parse(dto.ContactedDate);
            ProfilingCampaignSourceId = dto.ProfilingCampaignSourceId;
            StatusCode = dto.StatusCode;
            TargetedMembershipId = dto.TargetedMembershipId;
        }

    }
}