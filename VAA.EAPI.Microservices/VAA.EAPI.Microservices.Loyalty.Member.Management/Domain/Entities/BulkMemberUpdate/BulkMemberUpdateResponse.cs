using Microsoft.Azure.Cosmos.Table;
using System;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BulkMemberUpdate
{
    public class BulkMemberUpdateResponse : TableEntity
    {
        public string MemberNumber { get; set; }
        public string MemberNotes { get; set; }
        public string MemberStatus { get; set; }
        public string InitiatedBy { get; set; }
        public DateTime InitiatedDate { get; set; }
        public string ProcessStatus { get; set; }
    }
}