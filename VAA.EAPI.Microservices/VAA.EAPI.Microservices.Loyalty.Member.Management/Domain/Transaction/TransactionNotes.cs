using System;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Transaction
{
    public class TransactionNotes
    {
        public string NoteId { get; set; }
        public string Note { get; set; }
        public string NoteType { get; set; }
        public string Private { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public string DBLastUpdatedOn { get; set; }
        public string DBLastUpdatedSrc { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
    }
}
