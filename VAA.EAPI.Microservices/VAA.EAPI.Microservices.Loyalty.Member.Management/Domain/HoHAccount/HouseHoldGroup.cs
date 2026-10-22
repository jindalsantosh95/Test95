using System;
using System.Collections.Generic;
namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.HoHAccount
{
    public class HouseHoldGroup
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
    }
}
