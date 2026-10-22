using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;
using CloudEvent = VAA.EAPI.Domain.Events.CloudEvent;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Mapper
{
    public class OrderCancelDataMapper
    {
        public static CloudEvent EventMapper(OrderChangeRequestData orderChangeRequestData, string id = "")
        {
            var cloudEvent = new CloudEvent()
            {
                Data = orderChangeRequestData,
                Source = Constants.Application.NDCCancelPnrTopicName,
                Time = DateTime.Now.ToString(Constants.Application.ZuluDateTimeFormat),
                Specversion = Constants.Application.EventSpecversion,
                Subject = Constants.Application.OrderCancelEventSubject,
                DataContentType = Constants.Application.EventDataContentType,
                Type = Constants.Application.OrderCancelEventSubject
            };

            if (!id.IsNullOrWhiteSpace())
                cloudEvent.Id = id;

            return cloudEvent;
        }

        public static Response<string> ResponseMapper(string id)
        {
            return new Response<string>
            {
                Status = Status.Accepted,
                HeaderParameters = new List<ResponseHeader> { new() { Key = Constants.Application.HeaderParameterlocation, Value = ConfigReader.Get(Constants.Environment.OrderCancelResponseUrl).Replace("{correlationid}", id) } },
            };
        }
    }
}
