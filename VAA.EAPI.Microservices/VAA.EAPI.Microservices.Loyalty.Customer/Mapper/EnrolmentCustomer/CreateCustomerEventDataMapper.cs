using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Mapper.EnrolmentCustomer
{
    public class CreateCustomerEventDataMapper
    {
        public static CloudEvent CustomerEventDataMapper(CreateCustomerEventResponse createCustomerEventResponse, CreateCustomerEntities createCustomerEventRequest, string Id)
        {
            return new CloudEvent
            {
                Id = Id,
                Subject = Constants.Application.CreateCustomerEventSubject,
                Type = ConfigReader.Get(Environment.EventTypeCustomerSync),
                Data = createCustomerEventResponse,
                Source = ConfigReader.Get(Environment.ServiceBusTopicName),
                Specversion = Constants.Application.DataVersion
            };

        }

        public static CloudEvent VaCustomerEventDataMapper(CreateCustomerEventResponse createCustomerEventResponse, string Id)
        {
            return new CloudEvent
            {
                Id = Id,
                Subject = Constants.Application.CreateCustomerEventSubject,
                Type = Constants.Application.VaUpgradeFlyingClubSubject,
                Data = createCustomerEventResponse,
                Source = ConfigReader.Get(Environment.ServiceBusTopicName),
                Specversion = Constants.Application.DataVersion
            };

        }
    }
}
