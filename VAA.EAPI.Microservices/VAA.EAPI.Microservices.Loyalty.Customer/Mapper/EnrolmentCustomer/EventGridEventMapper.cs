using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enums;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Mapper.EnrolmentCustomer
{
    public class EventGridEventMapper
    {
        public static EventGridTopicEvent EventDataMapper(TransactionalEvent eventdata)
        {
            return new EventGridTopicEvent
            {
                Id = eventdata.Id,
                Subject = eventdata.Subject,
                Type = eventdata.EventType,
                Data = eventdata.Data,
                Source = ConfigReader.Get(Environment.SiebelEventGridUrl),
                Key = ConfigReader.Get(Environment.SiebelEventGridTopicKey),
                Specversion = Constants.Application.DataVersion
            };
        }

        public static CloudEvent DeDupDataMapper(CreateCustomerEntities customerDetails, string correlationId)
        {
            return new CloudEvent
            {
                Id = correlationId,
                Subject = Constants.Application.DeDupValidateSubject,
                Data = customerDetails,
                Source = Constants.Application.CustomerEventHandlerTopicName,
                Type = Constants.Application.DeDupValidateSubject,
                Specversion = Constants.Application.DataVersion
            };
        }
        public static CloudEvent VacEnrolmentDataMapper(CreateCustomerEntities customerDetails)
        {
            return new CloudEvent
            {
                Subject = Constants.Application.VacEnrolmentSubject,
                Data = customerDetails,
                Source = Constants.Application.CustomerEventHandlerTopicName,
                Type = Constants.Application.VacEnrolmentEventType,
                Specversion = Constants.Application.DataVersion
            };
        }

        public static CloudEvent B2CEnrolmentDataMapper(CreateCustomerEntities customerDetails, string correlationId)
        {
            List<EmailDetails> emailDetails = [];

            EmailDetails email = new()
            {
                EmailId = customerDetails?.ContactDetails?.EmailDetails.Find(x => x.PreferredFlag == "Y").EmailId
            };

            emailDetails.Add(email);

            return new CloudEvent
            {
                Id = correlationId,
                Subject = Constants.Application.b2cEnrolmentEvent,
                Data = new CreateCustomerEntities()
                {
                    FirstName = customerDetails.FirstName,
                    LastName = customerDetails.LastName,
                    CustomerId = customerDetails.CustomerId,
                    FcNumber = customerDetails.FcNumber,
                    MemberType = MemberTypeEnum.Individual.ToString(),
                    ContactDetails = new()
                    {
                        EmailDetails = emailDetails
                    }
                },
                Source = Constants.Application.CustomerEventHandlerTopicName,
                Type = Constants.Application.b2cEnrolmentEvent,
                Specversion = Constants.Application.DataVersion
            };
        }
    }
}
