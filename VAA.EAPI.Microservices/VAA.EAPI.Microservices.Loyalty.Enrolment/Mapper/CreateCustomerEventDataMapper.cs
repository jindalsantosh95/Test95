using System;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Mapper
{
    public class CreateCustomerEventDataMapper
    {
        public static CloudEvent CustomerEventDataMapper(CreateCustomerEventEntities createCustomerEventEntities, EnrolmentEventRequest enrolmentEventRequest)
        {
            return
             new CloudEvent
             {
                 Id = enrolmentEventRequest?.Id,
                 Subject = Constants.Application.CreateCustomerEventSubject,
                 Type = Environment.GetEnvironmentVariable(Constants.Environment.EventType),
                 Data = Json.Serialize(createCustomerEventEntities),
                 Source = Environment.GetEnvironmentVariable(Constants.Environment.ServiceBusTopicName),
                 Specversion = Constants.Application.DataVersion
             };

        }
        public static CloudEvent CustomerDataMapper(string cloudId, string cloudType, object request)
        {
            return
            new CloudEvent
            {
                Id = cloudId,
                Subject = Constants.Application.CreateCustomerEventSubject,
                Type = cloudType,
                Data = request,
                Source = Environment.GetEnvironmentVariable(Constants.Environment.ServiceBusTopicName),
                Specversion = Constants.Application.DataVersion
            };
        }
    }
}
