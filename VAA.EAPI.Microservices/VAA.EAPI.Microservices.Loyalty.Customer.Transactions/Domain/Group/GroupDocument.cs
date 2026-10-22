using System;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Group
{
    public class GroupDocument
    {
        public string SourceIdentifier { get; set; }
        public string Brand { get; set; }
        public string SchemaVersion { get; set; }
        public string DocType { get; set; }
        public string Id { get; set; }
        public string GroupId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string GroupName { get; set; }
        public string GroupTypeCode { get; set; }
        public string Status { get; set; }
        public string HeadOfGroupCustomerId { get; set; }
        public string SourceSystem { get; set; }
        public List<GroupHistory> GroupHistory { get; set; }
        public string _etag { get; set; }

        public GroupDocument()
        {
            // Required for Serialization
        }

        public GroupDocument(GroupDocumentDTO dto)
        {
            SourceIdentifier = dto.SourceIdentifier;
            Brand = dto.Brand;
            SchemaVersion = dto.SchemaVersion;
            DocType = dto.DocType;
            Id = dto.Id;
            GroupId = dto.GroupId;
            CreatedOn = !string.IsNullOrEmpty(dto.CreatedOn) ? DateTime.Parse(dto.CreatedOn) : null;
            CreatedBy = dto.CreatedBy;
            LastUpdatedBy = dto.LastUpdatedBy;
            LastUpdatedOn = !string.IsNullOrEmpty(dto.LastUpdatedOn) ? DateTime.Parse(dto.LastUpdatedOn) : null;
            GroupName = dto.GroupName;
            GroupTypeCode = dto.GroupTypeCode;
            Status = dto.Status;
            HeadOfGroupCustomerId = dto.HeadOfGroupCustomerId;
            SourceSystem = dto.SourceSystem;
            GroupHistory = new List<GroupHistory>();
            foreach (GroupHistoryDTO historyDto in dto.GroupHistory)
            {
                GroupHistory history = new GroupHistory(historyDto);
                GroupHistory.Add(history);
            }
            _etag = dto._etag;
        }
    }
}
