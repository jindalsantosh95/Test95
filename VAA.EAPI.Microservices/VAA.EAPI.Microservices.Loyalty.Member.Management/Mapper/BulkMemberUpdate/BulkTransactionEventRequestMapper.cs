using System;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Customer;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.MemberAccount;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.BulkMemberUpdate
{
    public class BulkTransactionEventRequestMapper
    {
        public static CloudEvent EventDataMapperNotes(Customer createUpdateEventRequest, string notes)
        {
            return new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Subject = Constants.Application.CustomerTransactioneventmessagesSubject,
                Time = DateTime.UtcNow.ToString(Constants.Application.EventTimeFormat),
                Data = new CustomerNotesEventRequest
                {
                    ContactNote = new ContactNote
                    {
                        Group = Constants.Application.ContactNoteGroup,
                        Type = Constants.Application.ContactNoteGroup,
                        Text = notes,
                        Id = createUpdateEventRequest.CustomerId,
                        CreatedBy = createUpdateEventRequest.LastUpdatedBy,
                        LastUpdatedBy = createUpdateEventRequest.LastUpdatedBy
                    },
                    CustomerId = createUpdateEventRequest.CustomerId
                },
                Source = Constants.Application.CustomerTransactionEventmessagesTopicName,
                Type = Constants.Application.CustomerTransactioneventmessagesType,
                Specversion = Constants.Application.EventSpecversion
            };
        }

        public static CloudEvent EventDataMapperCustomerTransStatus(Domain.FlyingClubMember.FlyingClubMember flyingClubMemberDoc)
        {
            return new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Subject = Constants.Application.CustomerTransactionStatuseventmessagesSubject,
                Time = DateTime.UtcNow.ToString(Constants.Application.EventTimeFormat),
                Data = new CustomerTransactionStatusEventRequest
                {
                    FlyingClubMemberDocument = new FlyingClubMemberDocument
                    {
                        Id = flyingClubMemberDoc.Id,
                        CustomerId = flyingClubMemberDoc.CustomerId,
                        LastUpdatedBy = flyingClubMemberDoc.LastUpdatedBy,
                        LastUpdatedOn = DateTime.UtcNow.ToString(Constants.Application.EventTimeFormat),
                        Status = flyingClubMemberDoc.Status
                    },
                    CustomerId = flyingClubMemberDoc?.CustomerId
                },
                Source = Constants.Application.CustomerTransactionEventmessagesTopicName,
                Type = Constants.Application.CustomerTransactionStatuseventmessagesSubject,
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}
