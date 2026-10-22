namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.BulkMemberUpdate
{
    public class BulkMemberUpdateRequest
    {
        public string MemberStatus { get; set; }
        public string MemberNotes { get; set; }
        public string BlobFilePath { get; set; }
        public string InitiatedBy { get; set; }
    }
}