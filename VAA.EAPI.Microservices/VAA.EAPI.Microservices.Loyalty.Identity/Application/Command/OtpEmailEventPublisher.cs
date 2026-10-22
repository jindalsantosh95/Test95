using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.Interfaces;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command
{
    public class OtpEmailEventPublisher : IOtpEmailEventPublisher
    {
        private readonly EventStoreRequestInsertCommandHandler _eventStoreRequestInsertCommandHandler;
        private readonly EventStoreRequestDeleteCommandHandler _eventStoreRequestDeleteCommandHandler;
        private readonly ServiceBusTopicMessageEventCommandHandler _serviceBusTopicMessageEventCommandHandler;

        public OtpEmailEventPublisher(
            EventStoreRequestInsertCommandHandler eventStoreRequestInsertCommandHandler,
            EventStoreRequestDeleteCommandHandler eventStoreRequestDeleteCommandHandler,
            ServiceBusTopicMessageEventCommandHandler serviceBusTopicMessageEventCommandHandler)
        {
            _eventStoreRequestInsertCommandHandler = eventStoreRequestInsertCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestInsertCommandHandler));
            _eventStoreRequestDeleteCommandHandler = eventStoreRequestDeleteCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestDeleteCommandHandler));
            _serviceBusTopicMessageEventCommandHandler = serviceBusTopicMessageEventCommandHandler ?? throw new ArgumentNullException(nameof(serviceBusTopicMessageEventCommandHandler));
        }

        public async Task Publish(CloudEvent emailEvent, string correlationIdForEvents, CancellationToken cancellationToken)
        {
            if (emailEvent == null) throw new ArgumentNullException(nameof(emailEvent));
            if (string.IsNullOrWhiteSpace(correlationIdForEvents)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(correlationIdForEvents));
            if (string.IsNullOrWhiteSpace(emailEvent.Source)) throw new InvalidOperationException("Email event source is missing.");

            await _eventStoreRequestInsertCommandHandler.Handle(
                new EventRequestInsertCommand { Id = correlationIdForEvents, Type = Constants.Application.OTPEmailEventSubject, Data = emailEvent.Data, Subject = emailEvent.Subject },
                cancellationToken);

            try
            {
                await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand
                {
                    Message = emailEvent,
                    ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString),
                    ServiceBusTopicName = emailEvent.Source
                }, cancellationToken);
            }
            catch
            {
                await _eventStoreRequestDeleteCommandHandler.Handle(
                    new EventStoreRequestDeleteCommand { Id = correlationIdForEvents, Type = Constants.Application.OTPEmailEventSubject, Data = emailEvent.Data, Subject = emailEvent.Subject },
                    cancellationToken);
                throw;
            }
        }
    }
}
