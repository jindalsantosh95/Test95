using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Retail.Order.Notification.Domain.Entities;
using VAA.EAPI.Microservices.Retail.Order.Notification.Domain.Enums;

namespace VAA.EAPI.Microservices.Retail.Order.Notification.Application.Commands
{

    public class OrderNotificationEventProcessCommand : CloudEvent, IRequest<Result<Unit>>
    {

    }
    public class OrderNotificationEventSubscribeCommandHandler : IRequestHandler<OrderNotificationEventProcessCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderNotificationEventSubscribeCommandHandler> _logger;
        public OrderNotificationEventSubscribeCommandHandler(IMediator mediator, ILogger<OrderNotificationEventSubscribeCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Unit>> Handle(OrderNotificationEventProcessCommand request, CancellationToken cancellationToken)
        {
            try
            {
                ServiceResponseProcessStatus responseStatus = Json.DeserializeObject<OrderNotificationStatusResponse>((JToken)request.Data).Status != OrderCreateStatus.Cancelled.ToString() ? ServiceResponseProcessStatus.Completed : ServiceResponseProcessStatus.Error;

                await _mediator.Send
                    (
                          request: new ServiceResponseInsertCommand
                          {
                              Id = request.Id,
                              JsonResponseData = Json.Serialize(request.Data),
                              Status = responseStatus,
                              Type = Constants.Application.OrderNotificationEventSubject
                          },
                           cancellationToken: CancellationToken.None
                    );

                return Unit.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in OrderNotificationEventSubscribeCommandHandler | Error ID: {request.Id} ");
                await _mediator.Send
                    (
                        request: new ServiceResponseInsertCommand
                        {
                            Id = request.Id,
                            JsonResponseData = Json.Serialize(request),
                            Status = ServiceResponseProcessStatus.Error,
                            Type = Constants.Application.OrderNotificationEventSubject
                        },
                        cancellationToken: CancellationToken.None
                    );
                throw;
            }
        }
    }
}
