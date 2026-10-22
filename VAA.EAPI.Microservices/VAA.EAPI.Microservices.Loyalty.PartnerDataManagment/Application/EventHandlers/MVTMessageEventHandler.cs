using Microsoft.Extensions.Logging;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Interfaces;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.MVT;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Mapper.MVT;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.EventHandlers
{
    public class MVTMessageEventHandler : IMVTMessageEventHandler
    {
        private readonly ServiceBusTopicMessageEventCommandHandler _serviceBusTopicMessageEventCommandHandler;
        private readonly EventStoreRequestInsertCommandHandler _eventStoreRequestInsertCommandHandler;
        private readonly EventStoreRequestDeleteCommandHandler _eventStoreRequestDeleteCommandHandler;
        private readonly ILogger<MVTMessageEventHandler> _logger;

        public MVTMessageEventHandler(ServiceBusTopicMessageEventCommandHandler serviceBusTopicMessageEventCommandHandler, EventStoreRequestInsertCommandHandler eventStoreRequestInsertCommandHandler, EventStoreRequestDeleteCommandHandler eventStoreRequestDeleteCommandHandler, ILogger<MVTMessageEventHandler> logger)
        {
            _serviceBusTopicMessageEventCommandHandler = serviceBusTopicMessageEventCommandHandler ?? throw new ArgumentNullException(nameof(serviceBusTopicMessageEventCommandHandler));
            _eventStoreRequestInsertCommandHandler = eventStoreRequestInsertCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestInsertCommandHandler));
            _eventStoreRequestDeleteCommandHandler = eventStoreRequestDeleteCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestDeleteCommandHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Handle(MVTMessageRequest request)
        {
            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request));
                }

                var correlationIdForEvents = Guid.NewGuid().ToString();

                await PublishEvent(MVTEventDataMapper.MVTMessageEventDataMapper(request), correlationIdForEvents, Constants.Application.FlightMessageEventType, CancellationToken.None);

                return true;
            }

            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error processing MVT message. FlightCarrier: {FlightCarrier}, FlightNumber: {FlightNumber}",
                    request?.FlightCarrier,
                    request?.FlightNumber);

                return false;
            }
        }

        private async Task PublishEvent(CloudEvent mvtMessageEvent, string correlationIdForEvents, string type, CancellationToken cancellationToken)
        {

            await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand { Id = correlationIdForEvents, Type = type, Data = mvtMessageEvent.Data, Subject = mvtMessageEvent.Subject, Source = mvtMessageEvent.Source }, cancellationToken);

            try
            {
                await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand { Message = mvtMessageEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.OrderMSServiceBusConnectionString), ServiceBusTopicName = mvtMessageEvent.Source }, cancellationToken);
            }

            catch
            {
                await _eventStoreRequestDeleteCommandHandler.Handle(new EventStoreRequestDeleteCommand { Id = correlationIdForEvents, Type = type, Data = mvtMessageEvent.Data, Subject = mvtMessageEvent.Subject, Source = mvtMessageEvent.Source }, cancellationToken);
                throw;
            }
        }
    }
}
