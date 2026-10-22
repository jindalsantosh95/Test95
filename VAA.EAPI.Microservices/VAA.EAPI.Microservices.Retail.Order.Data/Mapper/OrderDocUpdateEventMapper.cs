using System;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.Order;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Mapper
{
    public class OrderDocUpdateEventMapper
    {
        public static CloudEvent EventMapper(OrderDocument orderDocument, string correlationId)
        {
            return new CloudEvent()
            {
                Id = correlationId,
                Data = orderDocument,
                Source = Constants.Environment.NDCOrderStoreEventHandlerTopicName,
                Time = DateTime.Now.ToString(Constants.Application.ISODateTimeFormat),
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}
