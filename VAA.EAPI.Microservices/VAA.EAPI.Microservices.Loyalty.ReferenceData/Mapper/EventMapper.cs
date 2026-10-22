using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Domain.EventData;

namespace VAA.EAPI.Microservices.Loyalty.ReferenceData.Mapper
{
    public class EventMapper
    {
        public static CloudEvent MapProductsToCloudEvent(ProductsEventData productData)
        {
            return new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Data = productData,
                Type = EnumOperations.GetEnumDescription((EventCommandType)Enum.Parse(typeof(EventCommandType), EventCommandType.LoyaltyProductReferenceData.ToString())),
                Source = ConfigReader.Get(Constants.Environment.ServiceBusTopicName),
                Subject = EnumOperations.GetEnumDescription((EventCommandType)Enum.Parse(typeof(EventCommandType), EventCommandType.LoyaltyProductReferenceData.ToString())),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.SpecVersion
            };
        }

        public static CloudEvent MapPartnersToCloudEvent(PartnersEventData partnerData)
        {
            return new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Data = partnerData,
                Type = EnumOperations.GetEnumDescription((EventCommandType)Enum.Parse(typeof(EventCommandType), EventCommandType.LoyaltyPartnerReferenceData.ToString())),
                Source = ConfigReader.Get(Constants.Environment.ServiceBusTopicName),
                Subject = EnumOperations.GetEnumDescription((EventCommandType)Enum.Parse(typeof(EventCommandType), EventCommandType.LoyaltyPartnerReferenceData.ToString())),
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.SpecVersion
            };
        }
    }
}
