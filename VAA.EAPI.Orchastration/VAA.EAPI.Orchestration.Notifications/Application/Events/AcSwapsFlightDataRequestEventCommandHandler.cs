using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Orchestration.Notifications.Domain.Entities;
using VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.FlightMs;
using VAA.EAPI.Orchestration.Notifications.Mapper;


namespace VAA.EAPI.Orchestration.Notifications.Application.Events
{
    public sealed class AcSwapsFlightDataRequestEventCommandHandler(ILogger<AcSwapsFlightDataRequestEventCommandHandler> logger, IRequestHandler<EventRequestInsertCommand, bool> eventStoreRequestInsertCommandHandler, IRequestHandler<EventStoreRequestDeleteCommand, bool> eventStoreRequestDeleteCommandHandler, IRequestHandler<ServiceBusTopicMessageEventCommand, Unit> serviceBusTopicMessageEventCommandHandler, IHttpConnector httpConnector) : IAcSwapsFlightDataRequestEventCommand
    {

        private readonly IRequestHandler<EventRequestInsertCommand, bool> _eventStoreRequestInsertCommandHandler = eventStoreRequestInsertCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestInsertCommandHandler));
        private readonly IRequestHandler<EventStoreRequestDeleteCommand, bool> _eventStoreRequestDeleteCommandHandler = eventStoreRequestDeleteCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestDeleteCommandHandler));
        private readonly IRequestHandler<ServiceBusTopicMessageEventCommand, Unit> _serviceBusTopicMessageEventCommandHandler = serviceBusTopicMessageEventCommandHandler ?? throw new ArgumentNullException(nameof(serviceBusTopicMessageEventCommandHandler));
        private readonly ILogger<AcSwapsFlightDataRequestEventCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IHttpConnector _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));

        public async Task Handle(VAA.EAPI.Domain.Events.CloudEvent cloudEventReq, CancellationToken cancellationToken)
        {
            var eventId = Guid.NewGuid().ToString();
            try
            {
                //To call Flight MS API
                var aircraftMessage = Json.Deserialize<AircraftMessage>(Convert.ToString(cloudEventReq.Data));
                var request = (OperationalScheduleRequest)aircraftMessage;

                Dictionary<string, string> headers = new()
                {
                    { Constants.Constants.ApimSubscriptionKey, ConfigReader.Get(Constants.Environment.FlightOpsKey) }
                };

                var response = await _httpConnector.PostAsync<OperationalScheduleResponse>(
                    ConfigReader.Get("AircraftSwap:FlightMs:OperationalScheduleUri"), new StringContent(Json.Serialize(request), System.Text.Encoding.UTF8, "application/json"), headers);

                var flightInfo = response?.Flights?.FirstOrDefault();
                var legInfo = flightInfo?.Legs?.FirstOrDefault();
                var scheduledDepartureTime = legInfo?.ScheduledDepartureTime;

                if (scheduledDepartureTime is null || scheduledDepartureTime.LocalDateTime == default)
                {
                    _logger.LogInformation($"Aircraft Swaps - No flights found for the flight number {request.FlightNumber} with date {request.LocalDateFrom}");
                    return;
                }

                var aircraftCode = flightInfo?.Aircraft?.ModelCode;
                if (aircraftCode != null && aircraftCode == aircraftMessage.AircraftCode)
                {
                    return;
                }
                else
                {
                    if (!ValidateDepartureDatetoSendNotification(legInfo.ScheduledDepartureTime.UTCDateTime))
                    {
                        return;
                    }

                    flightInfo.Carrier = request.AirlineCode;
                    flightInfo.FlightNumber = request.FlightNumber.ToString();

                    //To publish msg to Retail Order Data
                    var flightOrderDataRequest = (FlightOrderDataRequest)flightInfo;

                    var cloudEvent = NotifCloudEventMapper.FlightOrderDataEventMapper(flightOrderDataRequest, eventId);

                    await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);

                    await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand { Message = cloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.NotificationServiceBusConnectionString), ServiceBusTopicName = ConfigReader.Get(Constants.Environment.NotificationTopicName) }, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Rollback event from Event store due to error in AcSwapsFlightDataRequestCommandHandler: {ex.Message}");
                await _eventStoreRequestDeleteCommandHandler.Handle(new EventStoreRequestDeleteCommand { Id = eventId, Type = Constants.Constants.FlightOrderDataReqEventSubject }, cancellationToken);
            }
        }

        public static bool ValidateDepartureDatetoSendNotification(DateTime flightDateUtc)
        {
            return (flightDateUtc - DateTime.UtcNow).TotalDays > 7;
        }

    }

}

