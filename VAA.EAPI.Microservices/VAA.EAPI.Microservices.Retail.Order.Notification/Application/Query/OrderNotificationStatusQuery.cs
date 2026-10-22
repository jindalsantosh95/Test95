using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Notification.Domain.Entities;

namespace VAA.EAPI.Microservices.Retail.Order.Notification.Application.Query
{
    ///<summary>
    ///OrderNotificationStatus is not in use as per user story -  296010
    ///</summary>
    public class OrderNotificationStatusQuery : IRequest<Result<Response<OrderNotificationStatusResponse>>>
    {
        public string CorrelationId { get; set; }
        public string Type { get; set; }
    }
}
