using System;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction
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
        public string DBLastUpdatedOn { get; set; }

        public string DBLastUpdatedSrc { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public TransactionNotes()
        {

        }
        public TransactionNotes(TransactionNotesDTO transactionNotes)
        {
            NoteId = transactionNotes?.NoteId;
            Note = transactionNotes?.Note;
            NoteType = transactionNotes?.NoteType;
            Private = transactionNotes?.Private;
            CreatedOn = DateTime.Parse(transactionNotes?.CreatedOn);
            CreatedBy = transactionNotes?.CreatedBy;
            LastUpdatedOn = DateTime.Parse(transactionNotes?.LastUpdatedOn);
            LastUpdatedBy = transactionNotes?.LastUpdatedBy;
            DBLastUpdatedOn = transactionNotes?.DBLastUpdatedOn;
            DBLastUpdatedSrc = transactionNotes?.DBLastUpdatedSrc;
            SourceIdentifier = transactionNotes?.SourceIdentifier;
            SourceSystem = transactionNotes?.SourceSystem;

        }
    }
}
