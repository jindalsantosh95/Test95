using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities
{
    public class TransactionNotesDTO
    {
        public string NoteId { get; set; }
        public string Note { get; set; }
        public string NoteType { get; set; }
        public string Private { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public string DBLastUpdatedOn { get; set; }
        public string DBLastUpdatedSrc { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }

        public TransactionNotesDTO()
        {

        }

        public static explicit operator TransactionNotesDTO(TransactionNotes transactionNotes)
        {
            return new()
            {
                NoteId = string.IsNullOrEmpty(transactionNotes?.NoteId) ? Utility.GetId() : transactionNotes?.NoteId,
                Note = transactionNotes?.Note,
                NoteType = transactionNotes?.NoteType,
                Private = transactionNotes?.Private,
                CreatedOn = string.IsNullOrEmpty(transactionNotes?.CreatedOn.ToString()) ? DateTime.UtcNow.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat) : transactionNotes?.CreatedOn?.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat),
                CreatedBy = transactionNotes?.CreatedBy,
                LastUpdatedOn = DateTime.UtcNow.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat),
                LastUpdatedBy = transactionNotes?.LastUpdatedBy,
                DBLastUpdatedOn = transactionNotes?.DBLastUpdatedOn,
                DBLastUpdatedSrc = transactionNotes?.DBLastUpdatedSrc,
                SourceIdentifier = transactionNotes?.SourceIdentifier,
                SourceSystem = transactionNotes?.SourceSystem
            };
        }
    }
}
