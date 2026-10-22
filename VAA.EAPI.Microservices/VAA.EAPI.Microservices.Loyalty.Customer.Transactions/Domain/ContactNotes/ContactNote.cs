using System;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Base;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.ContactNotes
{
    public class ContactNote : CustomerTransaction
    {
        public string Group { get; set; }
        public string Type { get; set; }
        public string Text { get; set; }

        public ContactNote()
        {
            // Required for Serialization
        }
        public ContactNote(ContactNoteDTO dto)
        {
            Id = dto.Id;
            CreatedBy = dto.CreatedBy;
            CreatedOn = DateTime.Parse(dto.CreatedOn);
            LastUpdatedBy = dto.LastUpdatedBy;
            LastUpdatedOn = DateTime.Parse(dto.LastUpdatedOn);
            Group = dto.NoteTypeGroup;
            Type = dto.NoteType;
            Text = dto.Description;
            CustomerId = dto.CustomerId;
        }

    }
}