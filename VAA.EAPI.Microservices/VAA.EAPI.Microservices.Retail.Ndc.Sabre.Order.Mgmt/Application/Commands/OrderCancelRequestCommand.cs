using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Application.Commands
{
    public class OrderCancelRequestCommand : IRequest<Result<Response<string>>>
    {
        public OrderChangeRequestData OrderChangeRequestData { get; set; }

        public static explicit operator OrderCancelRequestCommand(OrderChangeRequestData orderChangeRequestData) => new() { OrderChangeRequestData = orderChangeRequestData};
    }
}
