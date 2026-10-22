using MediatR;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium.BiDi.Network;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.Irop;
using VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.NotificationMs;
using VAA.EAPI.Orchestration.Notifications.Mapper;
using BookingReference = VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.NotificationMs.BookingReference;

namespace VAA.EAPI.Orchestration.Notifications.Application.Events
{
    public sealed class IropEventCommandHandler(EventStoreRequestInsertCommandHandler eventRequestInsertHandler, EventStoreRequestDeleteCommandHandler eventStoreRequestDeleteCommandHandler, ServiceBusTopicMessageEventCommandHandler serviceBusTopicMessageEventCommandHandler, ITrackAnalyticsEventCommandHandler trackAnalyticsEventCommandHandler, ILogger<FlightDataRequestEventCommandHandler> logger) : IIropEventCommand
    {
        private readonly ILogger<FlightDataRequestEventCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly ITrackAnalyticsEventCommandHandler _trackAnalyticsEventCommandHandler = trackAnalyticsEventCommandHandler ?? throw new ArgumentNullException(nameof(trackAnalyticsEventCommandHandler));
        private readonly EventStoreRequestInsertCommandHandler _eventRequestInsertHandler = eventRequestInsertHandler ?? throw new ArgumentNullException(nameof(eventRequestInsertHandler));
        private readonly EventStoreRequestDeleteCommandHandler _eventStoreRequestDeleteCommandHandler = eventStoreRequestDeleteCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestDeleteCommandHandler));
        private readonly ServiceBusTopicMessageEventCommandHandler _serviceBusTopicMessageEventCommandHandler = serviceBusTopicMessageEventCommandHandler ?? throw new ArgumentNullException(nameof(serviceBusTopicMessageEventCommandHandler));

        public async Task Handle(Ctx bizTalkmessage, CancellationToken cancellationToken)
        {
            var eventId = Guid.NewGuid().ToString();
            var isIropNotification = false;
            var trackAnalyticsEventCommand = new TrackAnalyticsEventCommand
            {
                Type = nameof(TrackingAnalyticsType.Microservice),
                Subject = Constants.Constants.IropNotificationReceivedTelemetryEvent
            };

            try
            {
                var irop = bizTalkmessage?.SendMessage?.MessageData?.ENENotification;
                if (irop is null)
                {
                    _logger.LogWarning("Ignoring message without an IROP ENENotification payload.");
                    return;
                }

                isIropNotification = true;

                trackAnalyticsEventCommand.Track("PnrLocator", irop.RecordLocator);
                trackAnalyticsEventCommand.Track("ReasonType", irop.ReasonType);

                var (deepLink, message) = irop.ReasonType switch
                {
                    "CANCL" => ($"{ConfigReader.Get(Constants.Environment.FlightCancelledContent)}/{irop.RecordLocator}", ConfigReader.Get(Constants.Environment.FlightCancelledAlertMessage)),
                    "TMCHG" => ($"{ConfigReader.Get(Constants.Environment.FlightTimeChangedContent)}/{irop.RecordLocator}", ConfigReader.Get(Constants.Environment.FlightTimeChangedAlertMessage))
                };

                var notifRequest = new NotificationRequest
                {
                    DeepLink = deepLink,
                    Message = message,
                    FlightOrderData = new List<FlightOrderData>
                    {
                        new FlightOrderData
                        {
                            BookingReference = new BookingReference
                            {
                                PnrLocator = irop.RecordLocator,
                                CreatedDate = irop.BookingCreationGMTDate ,

                            } ,
                            EventFlight =  new Flight
                        {
                            OperatingCarrier = new ()
                            {
                                FlightNum = Convert.ToString(irop.Flights.ENEFlight.MarketedFlightNumber),
                            },
                        },

                        }
                    }

                };

                var cloudEvent = NotifCloudEventMapper.NotifEventMapper(notifRequest, eventId);

                await _eventRequestInsertHandler.Handle(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);

                await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand { Message = cloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.NotificationServiceBusConnectionString), ServiceBusTopicName = ConfigReader.Get(Constants.Environment.NotificationTopicName) }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Rollback event from Event store due to error in IRopDataEventCommandHandler: {ex.Message}");
                await _eventStoreRequestDeleteCommandHandler.Handle(new EventStoreRequestDeleteCommand { Id = eventId, Type = Constants.Constants.FlightOrderNotifReqEventSubject }, cancellationToken);
            }
            finally
            {
                if (isIropNotification)
                    await _trackAnalyticsEventCommandHandler.Handle(trackAnalyticsEventCommand, cancellationToken);
            }
        }
    }

}

