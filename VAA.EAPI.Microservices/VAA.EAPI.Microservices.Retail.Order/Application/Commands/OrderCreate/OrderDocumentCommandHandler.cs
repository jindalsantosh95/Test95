using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Retail.Order.Domain.Order;
using VAA.EAPI.Microservices.Retail.Order.Mapper.OrderCreate;

namespace VAA.EAPI.Microservices.Retail.Order.Application.Commands.OrderCreate
{
    public class OrderDocumentCommandHandler : IRequestHandler<OrderDocumentCommand, bool>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderDocumentCommandHandler> _logger;

        public OrderDocumentCommandHandler(IMediator mediator, ILogger<OrderDocumentCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Handle(OrderDocumentCommand response, CancellationToken cancellationToken)
        {
            string eventId = Guid.NewGuid().ToString();

            try
            {
                var cloudEvent = OrderCreateCommandToEventMapper.EventMapper(response.OrderDocument, eventId);

                await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);

                try
                {
                    await _mediator.Send(new ServiceBusTopicMessageEventCommand { Message = cloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), ServiceBusTopicName = ConfigReader.Get(Constants.Environment.ServiceBusTopicName) }, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Exception - {ex}");

                    await _mediator.Send(new EventStoreRequestDeleteCommand { Id = eventId, Type = OrderEventType.OrderCreate.ToString(), Data = response.OrderDocument, Subject = Constants.Application.OrderStoreEventSubject }, cancellationToken);

                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} - Exception - {ex}");
                return false;
            }
        }
    }
}
