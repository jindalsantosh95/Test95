using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.NotificationMs;
using VAA.EAPI.Orchestration.Notifications.Mapper;
using FlightOrderData = VAA.EAPI.Orchestration.Notifications.Domain.Entities.FlightOrderData;
using Notification = VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.NotificationMs;

namespace VAA.EAPI.Orchestration.Notifications.Application.Events
{
    public sealed class FlightOrderDataEventCommandHandler(IRequestHandler<EventRequestInsertCommand, bool> eventStoreRequestInsertCommandHandler, IRequestHandler<EventStoreRequestDeleteCommand, bool> eventStoreRequestDeleteCommandHandler, IRequestHandler<ServiceBusTopicMessageEventCommand, Unit> serviceBusTopicMessageEventCommandHandler, ILogger<FlightDataRequestEventCommandHandler> logger, IHttpConnector httpConnector) : IFlightOrderDataEventCommand
    {
        private readonly IRequestHandler<EventRequestInsertCommand, bool> _eventStoreRequestInsertCommandHandler = eventStoreRequestInsertCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestInsertCommandHandler));
        private readonly IRequestHandler<EventStoreRequestDeleteCommand, bool> _eventStoreRequestDeleteCommandHandler = eventStoreRequestDeleteCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestDeleteCommandHandler));
        private readonly IRequestHandler<ServiceBusTopicMessageEventCommand, Unit> _serviceBusTopicMessageEventCommandHandler = serviceBusTopicMessageEventCommandHandler ?? throw new ArgumentNullException(nameof(serviceBusTopicMessageEventCommandHandler));
        private readonly ILogger<FlightDataRequestEventCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IHttpConnector _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));

        public async Task Handle(CloudEvent cloudEventReq, CancellationToken cancellationToken)
        {
            var eventId = Guid.NewGuid().ToString();

            try
            {
                var requestData = Json.Deserialize<List<FlightOrderData>>(Convert.FromBase64String(cloudEventReq.Data as string).DecompressToString());
                var eventFlight = requestData.First().EventFlight;

                if (eventFlight.NotificationType == Constants.Constants.NotificationTypeSeatSelectionReminder)
                {
                    requestData = requestData.Where(IsSeatSelectionReminderEligible).ToList();

                    if (!requestData.Any())
                    {
                        _logger.LogInformation("Seat selection reminder: no eligible customers found for flight {FlightNum} departing {DepartureTime}.", eventFlight?.OperatingCarrier?.FlightNum, eventFlight?.ScheduledDepartureLocalDateTime);
                        return;
                    }
                }
               
                if (eventFlight.NotificationType == Constants.Constants.NotificationTypeAircraftSwap)
                {
                    var cloudEvent = NotifCloudEventMapper.AircraftChangeNotifEventMapper(requestData, eventId);

                    await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);

                    await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand { Message = cloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.NotificationServiceBusConnectionString), ServiceBusTopicName = ConfigReader.Get(Constants.Environment.NotificationTopicName) }, cancellationToken);
                }
                else
                {
                    var notificationCopy = GetNotificationCopy(eventFlight.NotificationType);
                    var message = notificationCopy.Message;

                    if (HasAirportPlaceholders(message))
                    {
                        var referenceUri = ConfigReader.Get<Uri>("Notifications:ReferenceMs:Uri");
                        var airports = await _httpConnector.GetAsync<JArray>($"{referenceUri}/airportdetail?airportCodes={eventFlight.Departure},{eventFlight.Arrival}");
                        var departureAirport = airports.First(x => x["airportCode"].ToString().Is(eventFlight.Departure));
                        var arrivalAirport = airports.First(x => x["airportCode"].ToString().Is(eventFlight.Arrival));

                        message = message.Replace("{departure}", departureAirport["cityName"].ToString())
                                         .Replace("{arrival}", arrivalAirport["cityName"].ToString());
                    }

                    var notifRequest = new Notification.NotificationRequest
                    {
                        DeepLink = notificationCopy.DeepLink,
                        CustomEventName = notificationCopy.CustomEventName,
                        Campaigns = notificationCopy.campaigns,
                        Message = message,

                        FlightOrderData = requestData.Select(d => (Notification.FlightOrderData)d)
                    };

                    var cloudEvent = NotifCloudEventMapper.NotifEventMapper(notifRequest, eventId);

                    await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);

                    await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand { Message = cloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.NotificationServiceBusConnectionString), ServiceBusTopicName = ConfigReader.Get(Constants.Environment.NotificationTopicName) }, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Rollback event from Event store due to error in FlightOrderDataEventCommandHandler: {ex.Message}");
                await _eventStoreRequestDeleteCommandHandler.Handle(new EventStoreRequestDeleteCommand { Id = eventId, Type = Constants.Constants.FlightOrderNotifReqEventSubject }, cancellationToken);
            }
        }

        private static bool HasAirportPlaceholders(string message)
        {
            return message.Contains("{departure}") || message.Contains("{arrival}");
        }

        private static (string Message, string DeepLink, string CustomEventName, Campaigns campaigns) GetNotificationCopy(string notificationType)
        {
            if (notificationType.Is(Constants.Constants.NotificationTypeStarlink))
            {
                return
                (
                    ConfigReader.Try(Constants.Environment.StarlinkEnabledAlertMessage, "Starlink WiFi is onboard your flight. Tap to find out more."),
                    ConfigReader.Try(Constants.Environment.StarlinkEnabledContent, "https://virginatlantic.com/trips"),
                    ConfigReader.Try(Constants.Environment.StarlinkCustomEventName, Constants.Constants.DefaultStarlinkCustomEventName),
                    new Campaigns
                    {
                        Categories = new List<string>
                        {
                            ConfigReader.Try(Constants.Environment.StarlinkCampaign, Constants.Constants.DefaultStarlinkCampaign),
                            ConfigReader.Try(Constants.Environment.StarlinkWifiCampaign, Constants.Constants.DefaultStarlinkWifiCampaign)
                        }
                    }
                );
            }

            if (notificationType.Is(Constants.Constants.NotificationTypeSeatSelectionReminder))
            {
                return
                (
                    ConfigReader.Get(Constants.Environment.SeatSelectionReminderAlertMessage),
                    ConfigReader.Get(Constants.Environment.SeatSelectionReminderContent),
                    null,
                    null
                );
            }

            return
            (
                ConfigReader.Get(Constants.Environment.CheckInOpenedAlertMessage),
                ConfigReader.Get(Constants.Environment.CheckInOpenedContent),
                null,
                null
            );
        }

        private static bool IsSeatSelectionReminderEligible(FlightOrderData flightOrderData)
        {
            if (flightOrderData?.Seats == null) return true;

            return !flightOrderData.Seats.Any(seat => seat != null && !seat.CurrentSeatAssignment.IsNullOrWhiteSpace());
        }
    }

}

