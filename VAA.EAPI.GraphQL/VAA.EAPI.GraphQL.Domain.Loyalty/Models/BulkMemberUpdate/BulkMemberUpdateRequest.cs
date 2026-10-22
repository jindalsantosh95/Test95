using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.BulkMemberUpdate
{
    public class BulkMemberUpdateRequest
    {
        public string? MemberStatus { get; set; }     
        public string? MemberNotes { get; set; } 
        public string BlobFilePath { get; set; }
        public string InitiatedBy { get; set; } 
    }
}
