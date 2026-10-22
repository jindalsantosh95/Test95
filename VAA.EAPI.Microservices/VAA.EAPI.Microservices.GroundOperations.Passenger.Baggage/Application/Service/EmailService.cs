using MediatR;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Application.Interface;
using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Domain.OrderEmail;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Application.Service
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IRequestHandler<EventRequestInsertCommand, bool> _eventStoreRequestInsertCommandHandler;
        private readonly IRequestHandler<EventStoreRequestDeleteCommand, bool> _eventStoreRequestDeleteCommandHandler;
        private readonly IRequestHandler<ServiceBusTopicMessageEventCommand, Unit> _serviceBusTopicMessageEventCommandHandler;

        public EmailService(ILogger<EmailService> logger, IRequestHandler<EventRequestInsertCommand, bool> eventStoreRequestInsertCommandHandler, IRequestHandler<EventStoreRequestDeleteCommand, bool> eventStoreRequestDeleteCommandHandler, IRequestHandler<ServiceBusTopicMessageEventCommand, Unit> serviceBusTopicMessageEventCommandHandler)
        {
            _logger = logger ?? throw new ArgumentException(nameof(logger));
            _eventStoreRequestInsertCommandHandler = eventStoreRequestInsertCommandHandler ?? throw new ArgumentException(nameof(eventStoreRequestInsertCommandHandler));
            _eventStoreRequestDeleteCommandHandler = eventStoreRequestDeleteCommandHandler ?? throw new ArgumentException(nameof(eventStoreRequestDeleteCommandHandler));
            _serviceBusTopicMessageEventCommandHandler = serviceBusTopicMessageEventCommandHandler ?? throw new ArgumentException(nameof(serviceBusTopicMessageEventCommandHandler));
        }      

        public async Task AddEventForEmailReceiptAsync(BagsOrderEmailRequest request)
        {

            var cloudEvent = (CloudEvent)request;

            try
            {
                await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, CancellationToken.None);

                await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand
                {
                    Message = cloudEvent,
                    ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.EmailServiceBusConnectionString),
                    ServiceBusTopicName = Constants.Application.EmailBagsPaymentEventTopicName
                }, CancellationToken.None);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Baggage confirmation email event failed for {EventId}.", cloudEvent.Id);

                await _eventStoreRequestDeleteCommandHandler.Handle(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = Constants.Application.EmailBagsPaymentEventType }, CancellationToken.None);
            }
        }
    }
}
