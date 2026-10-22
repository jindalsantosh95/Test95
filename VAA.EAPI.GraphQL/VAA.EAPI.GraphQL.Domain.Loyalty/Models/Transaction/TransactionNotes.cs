using System;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction
{
    public class TransactionNotes
    {
        public string NoteId { get; set; }
        public string Note { get; set; }
        public string NoteType { get; set; }
        public string Private { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
