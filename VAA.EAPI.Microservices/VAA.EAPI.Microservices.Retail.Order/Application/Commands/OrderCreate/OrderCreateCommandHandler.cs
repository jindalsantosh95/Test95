using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Retail.Order.Domain.Order;
using VAA.EAPI.Microservices.Retail.Order.Mapper.OrderCreate;

namespace VAA.EAPI.Microservices.Retail.Order.Application.Commands.OrderCreate
{
    public class OrderCreateCommandHandler : IRequestHandler<OrderCreateCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderCreateCommandHandler> _logger;

        public OrderCreateCommandHandler(IMediator mediator, ILogger<OrderCreateCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<string>>> Handle(OrderCreateCommand request, CancellationToken cancellationToken)
        {
            var cloudEventId = Guid.NewGuid().ToString();

            try
            {
                var cloudEvent = OrderCreateCommandToEventMapper.EventMapper(request.Request, cloudEventId);

                await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);

                await _mediator.Send(new ServiceBusTopicMessageEventCommand { Message = cloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), ServiceBusTopicName = ConfigReader.Get(Constants.Environment.ServiceBusTopicName) }, cancellationToken);

                var orderResponse = new Response<string>
                {
                    Status = Status.Accepted,
                    HeaderParameters = new List<ResponseHeader> { new ResponseHeader { Key = Constants.Application.HeaaderParameterlocation, Value = ConfigReader.Get(Constants.Environment.OrderCreateResponseUrl).Replace("{correlationid}", cloudEvent.Id) } },
                };

                return Result.Success(orderResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Exception - {ex}");
                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEventId, Type = OrderEventType.OrderCreate.ToString(), Data = request, Subject = Constants.Application.OrderCreateEventSubject }, cancellationToken);
                return Result.Failure<Response<string>>((ex.InnerException ?? ex).ToString());
            }
        }
    }
}
