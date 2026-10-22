using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Interfaces;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Enums;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.EventHandlers
{
    public sealed class AutoAccrualFeedbackGenerationPublisher : IAutoAccrualFeedbackGenerationPublisher
    {
        private readonly EventStoreRequestInsertCommandHandler _eventStoreInsertHandler;
        private readonly EventStoreRequestDeleteCommandHandler _eventStoreDeleteHandler;
        private readonly ServiceBusTopicMessageEventCommandHandler _serviceBusPublisher;

        public AutoAccrualFeedbackGenerationPublisher(
            EventStoreRequestInsertCommandHandler eventStoreInsertHandler,
            EventStoreRequestDeleteCommandHandler eventStoreDeleteHandler,
            ServiceBusTopicMessageEventCommandHandler serviceBusPublisher)
        {
            _eventStoreInsertHandler = eventStoreInsertHandler ?? throw new ArgumentNullException(nameof(eventStoreInsertHandler));
            _eventStoreDeleteHandler = eventStoreDeleteHandler ?? throw new ArgumentNullException(nameof(eventStoreDeleteHandler));
            _serviceBusPublisher = serviceBusPublisher ?? throw new ArgumentNullException(nameof(serviceBusPublisher));
        }

        public async Task PublishAsync(string correlationId, string messageId, CancellationToken cancellationToken)
        {
            var eventType = EnumOperations.GetEnumDescription(SkyTeamNlaEventType.InboundAutoAccrualFeedbackGeneration);
            var generationEvent = new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                CorrelationId = Guid.TryParse(correlationId, out var parsedCorrelationId) ? parsedCorrelationId : Guid.Empty,
                Type = eventType,
                Source = Constants.Application.PdmEventTopicName,
                Subject = eventType,
                Time = DateTime.UtcNow.ToString(Constants.Application.EventTimeFormat),
                Data = new AutoAccrualFeedbackGenerationPayload { CorrelationId = correlationId, MessageId = messageId }
            };

            try
            {
                await _eventStoreInsertHandler.Handle(new EventRequestInsertCommand
                {
                    Id = generationEvent.Id,
                    Type = generationEvent.Type,
                    Data = generationEvent.Data,
                    Subject = generationEvent.Subject
                }, cancellationToken);

                await _serviceBusPublisher.Handle(new ServiceBusTopicMessageEventCommand
                {
                    Message = generationEvent,
                    ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString),
                    ServiceBusTopicName = Constants.Application.PdmEventTopicName
                }, cancellationToken);
            }
            catch
            {
                await _eventStoreDeleteHandler.Handle(new EventStoreRequestDeleteCommand
                {
                    Id = generationEvent.Id,
                    Type = generationEvent.Type,
                    Data = generationEvent.Data,
                    Subject = generationEvent.Subject
                }, cancellationToken);

                throw;
            }
        }
    }
}
