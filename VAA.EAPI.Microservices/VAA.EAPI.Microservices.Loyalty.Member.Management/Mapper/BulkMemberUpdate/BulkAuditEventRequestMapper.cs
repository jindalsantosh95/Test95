using System;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Customer;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.MemberAccount;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.BulkMemberUpdate
{
    public class BulkAuditEventRequestMapper
    {
        public static CloudEvent EventDataMapperAudit(Customer createUpdateEventRequest)
        {
            return new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Subject = Constants.Application.UpdateCustomerEventSubjectinAudit,
                Time = DateTime.UtcNow.ToString(Constants.Application.EventTimeFormat),
                Data = new AuditEventRequest
                {
                    CustomerProfileAuditDocument = new CustomerProfileAuditDocument
                    {
                        FirstName = createUpdateEventRequest.FirstName,
                        LastName = createUpdateEventRequest.LastName,
                        Status = createUpdateEventRequest.Status,
                        SourceIdentifier = null,
                        CustomerId = createUpdateEventRequest.CustomerId,
                        CustomerProfileUpdatedBy = createUpdateEventRequest.LastUpdatedBy,
                        CustomerProfileUpdatedOn = createUpdateEventRequest.LastUpdatedOn
                    }
                },
                Source = Constants.Application.AuditEventmessagesTopicName,
                Type = Constants.Application.UpdateCustomerEventSubjectinAudit,
                Specversion = Constants.Application.EventSpecversion
            };

        }
    }
}
