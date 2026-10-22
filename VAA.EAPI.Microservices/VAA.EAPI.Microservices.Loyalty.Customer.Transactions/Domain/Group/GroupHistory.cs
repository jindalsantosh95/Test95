using System;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Group
{
    public class GroupHistory
    {
        public string HistoryId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExitDate { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public bool HoHManaged { get; set; }
        public string CustomerId { get; set; }

        public GroupHistory()
        {

        }

        public GroupHistory(GroupHistoryDTO dto)
        {
            HistoryId = dto.HistoryId;
            CreatedOn = !string.IsNullOrEmpty(dto.CreatedOn) ? DateTime.Parse(dto.CreatedOn) : null;
            CreatedBy = dto.CreatedBy;
            LastUpdatedOn = !string.IsNullOrEmpty(dto.LastUpdatedOn) ? DateTime.Parse(dto.LastUpdatedOn) : null;
            LastUpdatedBy = dto.LastUpdatedBy;
            StartDate = !string.IsNullOrEmpty(dto.StartDate) ? DateTime.Parse(dto.StartDate) : null;
            ExitDate = !string.IsNullOrEmpty(dto.ExitDate) ? DateTime.Parse(dto.ExitDate) : null;
            SourceIdentifier = dto.SourceIdentifier;
            SourceSystem = dto.SourceSystem;
            HoHManaged = dto.HoHManaged;
            CustomerId = dto.CustomerId;
        }
    }
}
