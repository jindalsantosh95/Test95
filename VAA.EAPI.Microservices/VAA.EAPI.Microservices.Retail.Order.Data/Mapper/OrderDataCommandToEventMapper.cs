using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.OrderCreate;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Mapper
{
    public class OrderDataCommandToEventMapper
    {
        public static CloudEvent CommandToEventMapper(OrderCreateRequestCommand request)
        {
            return new CloudEvent
            {
                Id = request.Id,
                Data = request.Data,
                Type = request.Type,
                Source = request.Source,
                Subject = request.Subject,
                Time = request.Time,
                Specversion = Constants.Application.EventSpecversion
            };
        }
    }
}
