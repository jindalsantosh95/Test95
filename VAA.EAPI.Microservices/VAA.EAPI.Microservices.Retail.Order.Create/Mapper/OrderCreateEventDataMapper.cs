using CSharpFunctionalExtensions;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Create.Application.Command;
using VAA.EAPI.Microservices.Retail.Order.Create.Constants;
using VAA.EAPI.Microservices.Retail.Order.Create.Domain;

namespace VAA.EAPI.Microservices.Retail.Order.Create.Mapper
{
    public class OrderCreateEventDataMapper
    {
        public static CloudEvent Mapper(OrderCreateRequest request)
        {
            return new CloudEvent()
            {
                Data = request,
                Type = ConfigReader.Get(Environment.OrderCreateEventType),
                Subject = ConfigReader.Get(Environment.OrderCreateEventSubject),
                Source = ConfigReader.Get(Environment.ServiceBusTopicName),
                Specversion = Constants.Application.SpecVersion
            };
        }

        public static OrderCreateSubscriberCommand RequestMapper(CloudEvent cloudEvent)
        {
            return new OrderCreateSubscriberCommand()
            {
                Id = cloudEvent.Id,
                Data = cloudEvent.Data,
                Type = cloudEvent.Type
            };
        }

        public static Result<Response<OrderResponse>> ResponseMapper(OrderResponse orderResponse)
        {
            Response<OrderResponse> response = new()
            {
                Value = orderResponse,
                Status = Status.OK
            };
            return response;

        }
    }
}
