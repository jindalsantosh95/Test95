using System;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Customer;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.MemberAccount;


namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.BulkMemberUpdate
{
    public class UpdateBulkEventDataMapper
    {
        public static CloudEvent EventDataMapperCustomer(Customer customerDocument)
        {
            return new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Subject = Constants.Application.UpdateCustomerEventSubjectinCustomer,
                Time = DateTime.UtcNow.ToString(Constants.Application.EventTimeFormat),
                Data = new CustomerStatusEventRequest
                {
                    CustomerId = customerDocument.CustomerId,
                    LastUpdatedBy = customerDocument.LastUpdatedBy,
                    LastUpdatedOn = customerDocument.LastUpdatedOn,
                    Status = customerDocument.Status
                },
                Source = Constants.Application.CustomerEventmessagesTopicName,
                Type = Constants.Application.UpdateCustomerEventSubjectinCustomer,
                Specversion = Constants.Application.EventSpecversion
            };
        }
        public static CloudEvent EventDataMapperVRED(Customer createUpdateEventRequest, string correlationId)
        {
            return new CloudEvent
            {
                Id = correlationId,
                Subject = Constants.Application.UpdateCustomerEventSubjectinVRED,
                Time = DateTime.UtcNow.ToString(Constants.Application.EventTimeFormat),
                Data = new VREDEventRequest
                {
                    CustomerId = createUpdateEventRequest.CustomerId,
                    MemberNumber = createUpdateEventRequest.LoyaltyMemberNumber,
                    MemberStatus = createUpdateEventRequest.Status,
                    FirstName = createUpdateEventRequest.FirstName,
                    LastName = createUpdateEventRequest.LastName,
                    Channel = Constants.Application.ChannelValue,
                    CreateUpdateTimestamp = DateTime.UtcNow.ToString(Constants.Application.EventTimeFormat),
                    EmailId = createUpdateEventRequest.EmailId
                },
                Source = Constants.Application.ManageAccountTopicName,
                Type = Constants.Application.UpdateCustomerEventSubjectinVRED,
                Specversion = Constants.Application.EventSpecversion
            };
        }
        public static CloudEvent EventDataMapperB2C(Customer createUpdateEventRequest)
        {
            return new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Subject = Constants.Application.UpdateCustomerEventSubjectinB2C,
                Time = DateTime.UtcNow.ToString(Constants.Application.EventTimeFormat),
                Data = new B2CEventRequest
                {
                    MemberNumber = createUpdateEventRequest.LoyaltyMemberNumber,
                    FirstName = createUpdateEventRequest.FirstName,
                    LastName = createUpdateEventRequest.LastName,
                    CurrentEmailId = createUpdateEventRequest.EmailId,
                    NewEmailId = createUpdateEventRequest.EmailId,
                    AccountStatus = createUpdateEventRequest.Status
                },
                Source = Constants.Application.ManageAccountTopicName,
                Type = Constants.Application.UpdateCustomerEventSubjectinB2C,
                Specversion = Constants.Application.EventSpecversion
            };
        }

    }
}
