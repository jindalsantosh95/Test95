using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Application.Command;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Constants;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Mapper
{
    public class AddressLookUpEventDataMapper
    {
        public static CloudEvent EventDataMapper(AddressLookupEventProcessCommand addressLookUpRequest, AddressLookUpRequest request)
        {
            return new CloudEvent
            {
                Id = addressLookUpRequest.Id,
                Subject = Constants.Application.Subject,
                Data = request,
                Type = ConfigReader.Get(Environment.AzureEventTopicType),
                Source = ConfigReader.Get(Environment.ServiceBusTopicName),
                Specversion = Constants.Application.DataVersion
            };
        }
    }
}
