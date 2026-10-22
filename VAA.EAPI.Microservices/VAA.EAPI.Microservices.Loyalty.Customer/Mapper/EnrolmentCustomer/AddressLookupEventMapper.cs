using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.Enrolment;
using VAA.EAPI.Microservices.Loyalty.Customer.Constants;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Mapper.EnrolmentCustomer
{
    public class AddressLookupEventMapper
    {
        public static AddressLookupEventCommand CustomerEventDataMapper(CloudEvent request)
        {
            return new AddressLookupEventCommand
            {
                Id = request.Id,
                Subject = Constants.Application.CreateCustomerEventSubject,
                Type = ConfigReader.Get(Environment.EventTypeAddressLookup),
                Data = request.Data,
                Source = ConfigReader.Get(Environment.AzureEventTopicEndPoint),
                Specversion = Constants.Application.DataVersion
            };
        }

        public static CloudEvent AddressEventMapper(AddressLookupEventCommand request)
        {
            return new CloudEvent
            {
                Id = request.Id,
                Subject = Constants.Application.CreateCustomerEventSubject,
                Type = ConfigReader.Get(Environment.EventTypeAddressLookup),
                Data = request.Data,
                Source = ConfigReader.Get(Environment.AzureEventTopicEndPoint),
                Specversion = Constants.Application.DataVersion
            };
        }
    }
}
