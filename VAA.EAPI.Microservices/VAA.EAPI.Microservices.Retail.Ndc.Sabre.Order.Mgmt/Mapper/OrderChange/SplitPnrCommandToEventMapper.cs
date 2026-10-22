using System;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.Enums;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Mapper.OrderChange
{
    public static class SplitPnrCommandToEventMapper
    {
        public static CloudEvent CommandToEventMapper(SplitPnrRequest splitPnrRequest)
        {
            var cloudEvent = new CloudEvent()
            {
                Data = splitPnrRequest,
                Type = EnumOperations.GetEnumDescription((OrderEventType)Enum.Parse(typeof(OrderEventType), OrderEventType.SplitPnrInitiate.ToString())),
                Source = ConfigReader.Get(Constants.Environment.ServiceBusTopicName),
                Subject = Constants.Application.SplitPnrEventSubject,
                Time = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt"),
                Specversion = Constants.Application.EventSpecversion
            };

            return cloudEvent;
        }

        public static CloudEvent EventMapper(OrderDocument request, string id)
        {
            var cloudEvent = new CloudEvent()
            {
                Id = id,
                Data = request,
                Type = EnumOperations.GetEnumDescription((OrderEventType)Enum.Parse(typeof(OrderEventType), OrderEventType.SplitPnrInitiate.ToString())),
                Time = DateTime.UtcNow.ToString(Constants.Application.DateTimeFormat)
            };

            return cloudEvent;
        }

        public static SplitPnrRequest TrimCloudEventData(SplitPnrRequest trimCloudEventData)
        {
            return new SplitPnrRequest
            {
                splitPaxId = trimCloudEventData.splitPaxId,
                OrderDocument = new OrderDocument
                {
                    BookingReference = trimCloudEventData.OrderDocument.BookingReference,
                }
            };
        }

        public static OrderDocument TrimOrderDocumentData(OrderDocument request)
        {
            return new OrderDocument
            {
                BookingReference = request.BookingReference,
            };
        }
    }
}
