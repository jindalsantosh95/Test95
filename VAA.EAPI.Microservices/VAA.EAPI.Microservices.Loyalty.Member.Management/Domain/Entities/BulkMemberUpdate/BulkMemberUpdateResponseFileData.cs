using System;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BulkMemberUpdate
{
    public class BulkMemberUpdateResponseFileData
    {
        public string MemberNumber { get; set; }
        public string MemberNotes { get; set; }
        public string MemberStatus { get; set; }
        public string InitiatedBy { get; set; }
        public DateTime InitiatedDate { get; set; }
        public string ProcessStatus { get; set; }
        public static explicit operator BulkMemberUpdateResponseFileData(BulkMemberUpdateResponse memberDetail)
        {
            return new()
            {
                MemberNumber = memberDetail.MemberNumber,
                MemberStatus = memberDetail.MemberStatus,
                MemberNotes = memberDetail.MemberNotes,
                InitiatedBy = memberDetail.InitiatedBy,
                InitiatedDate = memberDetail.InitiatedDate,
                ProcessStatus = memberDetail.ProcessStatus
            };
        }
    }
}