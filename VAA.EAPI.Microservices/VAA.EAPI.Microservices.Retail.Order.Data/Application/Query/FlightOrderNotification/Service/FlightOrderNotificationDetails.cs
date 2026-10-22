using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.FlightOrderNotification.Interface;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightOrderNotification;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories.FlightOrderNotification.Interface;
using CloudEvent = VAA.EAPI.Domain.Events.CloudEvent;


namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.FlightOrderNotification.Service
{
    public class FlightOrderNotificationDetails(IFlightOrderAssociationService iFlightOrderAssociationService, IRequestHandler<OrdersRepoQuery, IEnumerable<VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity.OrderDoc>> ordersRepoQueryHandler, ILogger<FlightOrderNotificationDetails> logger, IRequestHandler<ServiceBusTopicMessageEventCommand, Unit> serviceBusHandler, EventStoreRequestInsertCommandHandler eventRequestInsertHandler, EventStoreRequestDeleteCommandHandler eventStoreRequestDeleteCommandHandler) : IFlightOrderNotificationDetails
    {
        private readonly IFlightOrderAssociationService _iFlightOrderAssociationService = iFlightOrderAssociationService ?? throw new ArgumentNullException(nameof(iFlightOrderAssociationService));
        private readonly IRequestHandler<OrdersRepoQuery, IEnumerable<OrderDoc>> _ordersRepoQueryHandler = ordersRepoQueryHandler ?? throw new ArgumentNullException(nameof(ordersRepoQueryHandler));
        private readonly ILogger<FlightOrderNotificationDetails> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IRequestHandler<ServiceBusTopicMessageEventCommand, Unit> _serviceBusHandler = serviceBusHandler ?? throw new ArgumentNullException(nameof(serviceBusHandler));
        private readonly EventStoreRequestInsertCommandHandler _eventRequestInsertHandler = eventRequestInsertHandler ?? throw new ArgumentNullException(nameof(eventRequestInsertHandler));
        private readonly EventStoreRequestDeleteCommandHandler _eventStoreRequestDeleteCommandHandler = eventStoreRequestDeleteCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestDeleteCommandHandler));

        public async Task GetFlightOrderDetailAsync(CloudEvent cloudEvent, CancellationToken cancellationToken)
        {
            var request = Json.Deserialize<FlightBookingQuery>(Convert.ToString(cloudEvent.Data));

            var orderIds = await _iFlightOrderAssociationService.GetOrderIdsAsync(request.ScheduledDepartureLocalDateTime, request.OperatingCarrier.FlightNum, cancellationToken);

            if (!orderIds.Any())
            {
                _logger.LogInformation("No orders found for flight {FlightNum} on {DepartureDate} for event {EventId}",
                    request.OperatingCarrier.FlightNum, request.ScheduledDepartureLocalDateTime, cloudEvent.Id);

                return;
            }

            try
            {
                var batchTasks = orderIds.Chunk(ConfigReader.Get<int>(Constants.Environment.NotificationOrderReadBatchSize))
                                         .Select(batch => _ordersRepoQueryHandler.Handle(new OrdersRepoQuery(batch), cancellationToken));

                var batchResults = await Task.WhenAll(batchTasks);

                var flightNotifications = batchResults.SelectMany(batch => batch)
                                                      .Where(orderDoc => orderDoc != null)
                                                      .Select(orderDoc =>
                                                      {
                                                          var response = (FlightOrderNotificationResponse)orderDoc;

                                                          response.EventFlight = (Flight)request;
                                                          response.Seats = orderDoc?.Fulfillment?.Seat?.Where(seat => SeatMatchesRequest(seat, request))?.ToArray();

                                                          return response;
                                                      });

                await PublishFlightNotificationsAsync(flightNotifications, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Flight order notification processing failed for flight {FlightNum} on {DepartureDate} for event {EventId} with exception type {ExceptionType}",
                    request.OperatingCarrier.FlightNum, request.ScheduledDepartureLocalDateTime, cloudEvent.Id, ex.GetType().Name);

                throw;
            }

        }

        private async Task PublishFlightNotificationsAsync(IEnumerable<FlightOrderNotificationResponse> flightNotifications, CancellationToken cancellationToken)
        {
            var notificationEvent = new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Source = Constants.Application.FlightOrderDataEventSubject,
                Type = Constants.Application.FlightOrderDataEventType,
                Data = Json.Serialize(flightNotifications).Compress(),
                Subject = Constants.Application.FlightOrderDataEventSubject
            };

            try
            {
                var eventRequestInsertCommand = new EventRequestInsertCommand
                {
                    Id = notificationEvent.Id,
                    Type = notificationEvent.Type,
                    Data = notificationEvent.Data,
                    Subject = notificationEvent.Subject
                };

                await _eventRequestInsertHandler.Handle(eventRequestInsertCommand, cancellationToken);

                var command = new ServiceBusTopicMessageEventCommand { Message = notificationEvent, ServiceBusConnectionString = ConfigReader.Get<string>(Constants.Environment.NotificationServiceBusConnectionString), ServiceBusTopicName = Constants.Application.NotificationTopicName };

                await _serviceBusHandler.Handle(command, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Flight order notification persistence or publish failed; attempting event cleanup for event {EventId} with exception type {ExceptionType}",
                    notificationEvent.Id, ex.GetType().Name);

                await _eventStoreRequestDeleteCommandHandler.Handle(new EventStoreRequestDeleteCommand { Id = notificationEvent.Id, Type = Constants.Application.FlightOrderDataEventType }, cancellationToken);
            }
        }

        private static bool SeatMatchesRequest(Seat seat, FlightBookingQuery request)
        {
            if (seat == null || request?.OperatingCarrier == null) return false;

            var requestFlightNumber = request.OperatingCarrier.FlightNum?.TrimStart('0');
            var seatFlightNumber = seat.FlightNumber?.TrimStart('0');

            if (!requestFlightNumber.Is(seatFlightNumber)) return false;

            var departureMatches = seat.DeparturePoint.Is(request.Departure) || seat.SeatDeparturePoint.Is(request.Departure);
            var arrivalMatches = seat.ArrivalPoint.Is(request.Arrival);

            if (!departureMatches || !arrivalMatches) return false;

            // Avoid matching seats from the same route/flight number on a different date (when date components are available).
            if (!seat.DepartureDate.IsNullOrWhiteSpace() && !seat.DepartureYear.IsNullOrWhiteSpace())
            {
                var requestDate = request.ScheduledDepartureLocalDateTime;
                var expectedDepartureDate = requestDate.ToString("ddMMM", System.Globalization.CultureInfo.InvariantCulture).ToUpperInvariant();
                var expectedDepartureYear = seat.DepartureYear.Length == 4
                    ? requestDate.ToString("yyyy", System.Globalization.CultureInfo.InvariantCulture)
                    : requestDate.ToString("yy", System.Globalization.CultureInfo.InvariantCulture);

                if (!expectedDepartureDate.Is(seat.DepartureDate) || !expectedDepartureYear.Is(seat.DepartureYear)) return false;
            }

            return true;
        }
    }

}
