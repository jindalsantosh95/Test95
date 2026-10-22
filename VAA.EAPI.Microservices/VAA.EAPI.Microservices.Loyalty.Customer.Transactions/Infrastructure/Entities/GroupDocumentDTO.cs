using System.Collections.Generic;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Group;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities
{
    public class GroupDocumentDTO : CosmosDocument
    {
        public string SourceIdentifier { get; set; }
        public string Brand { get; set; }
        public string SchemaVersion { get; set; }
        public string DocType { get; set; }
        public string GroupId { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedBy { get; set; }
        public string LastUpdatedOn { get; set; }
        public string GroupName { get; set; }
        public string GroupTypeCode { get; set; }
        public string Status { get; set; }
        public string HeadOfGroupCustomerId { get; set; }
        public string SourceSystem { get; set; }
        public List<GroupHistoryDTO> GroupHistory { get; set; }
        public string _etag { get; set; }

        public GroupDocumentDTO()
        {

        }

        public GroupDocumentDTO(GroupDocument groupDoc)
        {
            SourceIdentifier = groupDoc.SourceIdentifier ?? string.Empty;
            Brand = groupDoc.Brand ?? string.Empty;
            SchemaVersion = groupDoc.SchemaVersion ?? string.Empty;
            DocType = groupDoc.DocType ?? string.Empty;
            Id = groupDoc.Id;
            PartitionKey = groupDoc.Id;
            GroupId = groupDoc.GroupId;
            CreatedOn = groupDoc.CreatedOn?.ToString(Constants.Application.ISODateTimeFormat);
            CreatedBy = groupDoc.CreatedBy ?? string.Empty;
            LastUpdatedBy = groupDoc.LastUpdatedBy ?? string.Empty;
            LastUpdatedOn = groupDoc.LastUpdatedOn?.ToString(Constants.Application.ISODateTimeFormat);
            GroupName = groupDoc.GroupName ?? string.Empty;
            GroupTypeCode = groupDoc.GroupTypeCode ?? string.Empty;
            Status = groupDoc.Status ?? string.Empty;
            HeadOfGroupCustomerId = groupDoc.HeadOfGroupCustomerId ?? string.Empty;
            SourceSystem = groupDoc.SourceSystem ?? string.Empty;
            GroupHistory = new List<GroupHistoryDTO>();
            foreach (GroupHistory history in groupDoc.GroupHistory)
            {
                GroupHistory.Add(new GroupHistoryDTO(history));
            }
            _etag = groupDoc._etag;
        }
    }
}
