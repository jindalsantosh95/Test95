using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Orchestration.Notifications.Domain.Entities;
using VAA.EAPI.Orchestration.Notifications.Mapper;

namespace VAA.EAPI.Orchestration.Notifications.Application.Events
{
    public sealed class CustomerProfileDataEventCommandHandler(EventStoreRequestInsertCommandHandler eventStoreRequestInsertCommandHandler, EventStoreRequestDeleteCommandHandler eventStoreRequestDeleteCommandHandler, ServiceBusTopicMessageEventCommandHandler serviceBusTopicMessageEventCommandHandler, ILogger<FlightDataRequestEventCommandHandler> logger) : ICustomerProfileDataEventCommand
    {
        private readonly ILogger<FlightDataRequestEventCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly EventStoreRequestInsertCommandHandler _eventStoreRequestInsertCommandHandler = eventStoreRequestInsertCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestInsertCommandHandler));
        private readonly EventStoreRequestDeleteCommandHandler _eventStoreRequestDeleteCommandHandler = eventStoreRequestDeleteCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestDeleteCommandHandler));
        private readonly ServiceBusTopicMessageEventCommandHandler _serviceBusTopicMessageEventCommandHandler = serviceBusTopicMessageEventCommandHandler ?? throw new ArgumentNullException(nameof(serviceBusTopicMessageEventCommandHandler));

        public async Task Handle(CloudEvent cloudEventReq, CancellationToken cancellationToken)
        {
            var eventId = Guid.NewGuid().ToString();

            try
            {
                var requestData = Json.Deserialize<List<FlightOrderData>>(Convert.ToString(cloudEventReq.Data));

                var cloudEvent = NotifCloudEventMapper.FlightOrderNotifEventMapper(requestData, eventId);

                await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);

                await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand { Message = cloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.NotificationServiceBusConnectionString), ServiceBusTopicName = ConfigReader.Get(Constants.Environment.NotificationTopicName) }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Rollback event from Event store due to error in CustomerProfileDataEventCommandHandler: {ex.Message}");

                await _eventStoreRequestDeleteCommandHandler.Handle(new EventStoreRequestDeleteCommand { Id = eventId, Type = Constants.Constants.FlightOrderNotifReqEventSubject }, cancellationToken);
            }
        }
    }

}

