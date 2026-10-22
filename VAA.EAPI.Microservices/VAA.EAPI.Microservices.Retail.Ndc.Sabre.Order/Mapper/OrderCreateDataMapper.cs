using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Domain;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mapper
{
    public class OrderCreateDataMapper
    {
        public static CloudEvent EventMapper(OrderCreateRequest request)
        {
            return new CloudEvent()
            {
                Data = request,
                Type = Constants.Application.OrderCreateEventType,
                Source = ConfigReader.Get(Constants.Environment.ServiceBusTopicName),
                Subject = Constants.Application.OrderCreateEventSubject,
                Time = DateTime.UtcNow.ToString(Constants.Application.DateTimeFormat)
            };
        }

        public static Response<string> ResponseMapper(string id)
        {
            return new Response<string>
            {
                Status = Status.Accepted,
                HeaderParameters = new List<ResponseHeader> { new() { Key = Constants.Application.HeaderParameterlocation, Value = ConfigReader.Get(Constants.Environment.OrderCreateResponseUrl).Replace("{correlationid}", id) } },
            };
        }

        public static CloudEvent EventMapper(OrderDocument request, string correlationId)
        {
            return new CloudEvent()
            {
                Id = correlationId,
                Data = request,
                Type = Constants.Application.OrderCreateEventSubject,
                Time = DateTime.UtcNow.ToString(Constants.Application.DateTimeFormat)
            };
        }
    }
}
