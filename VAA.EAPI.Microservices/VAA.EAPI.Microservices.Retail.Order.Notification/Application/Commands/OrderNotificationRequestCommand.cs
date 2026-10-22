using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Notification.Domain.Entities;

namespace VAA.EAPI.Microservices.Retail.Order.Notification.Application.Commands
{
    public class OrderNotificationRequestCommand : IRequest<Result<Response<string>>>
    {
        public string Channel { get; set; }
        public OrderNotificationRequest NotificationRequest { get; set; }
    }
}
