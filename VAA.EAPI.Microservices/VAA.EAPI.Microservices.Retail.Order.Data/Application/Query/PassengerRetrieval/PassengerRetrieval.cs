using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.FlightOrderNotification.Service;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories.FlightOrderNotification.Interface;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.PassengerRetrieval;
using VAA.EAPI.Microservices.Retail.Order.Data.Mapper.PassengerRetrieval;
using System.Linq;
using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.PassengerRetrieval
{
    public class PassengerRetrieval(IFlightOrderAssociationService flightOrderAssociationService, IRequestHandler<OrdersRepoQuery, IEnumerable<OrderDoc>> ordersRepoQueryHandler, ILogger<FlightOrderNotificationDetails> logger, IRequestHandler<ServiceBusTopicMessageEventCommand, Unit> serviceBusHandler) : IPassengerRetrieval
    {
        private readonly IFlightOrderAssociationService _iFlightOrderAssociationService = flightOrderAssociationService ?? throw new ArgumentNullException(nameof(flightOrderAssociationService));
        private readonly IRequestHandler<OrdersRepoQuery, IEnumerable<OrderDoc>> _ordersRepoQueryHandler = ordersRepoQueryHandler ?? throw new ArgumentNullException(nameof(ordersRepoQueryHandler));
        private readonly ILogger<FlightOrderNotificationDetails> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IRequestHandler<ServiceBusTopicMessageEventCommand, Unit> _serviceBusHandler = serviceBusHandler ?? throw new ArgumentNullException(nameof(serviceBusHandler));

        public async Task GetPassengerFlightDetailAsync(CloudEvent cloudEvent, CancellationToken cancellationToken)
        {
            var request = Json.Deserialize<PassengerRetrievalQuery>(Convert.ToString(cloudEvent.Data));

            var orderIds = (await _iFlightOrderAssociationService.GetOrderIdsAsync(request.DepartureDate, request.FlightNumber, cancellationToken))
                .Where(orderId => !string.IsNullOrWhiteSpace(orderId))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            if (!orderIds.Any())
            {
                _logger.LogInformation("No orders found for flight {FlightNum} on {DepartureDate} for event {EventId}",
                    request.FlightNumber, request.DepartureDate, cloudEvent.Id);
                return;
            }

            var batchTasks = orderIds.Chunk(ConfigReader.Get<int>(Constants.Environment.NotificationOrderReadBatchSize))
                                     .Select(batch => _ordersRepoQueryHandler.Handle(new OrdersRepoQuery(batch), cancellationToken));

            var batchResults = await Task.WhenAll(batchTasks);

            var passengerDetails = batchResults.SelectMany(batch => batch)
                                               .Where(orderDoc => orderDoc != null)
                                               .SelectMany(orderDoc => PassengerFlightResponseMapper.CreateResponses(orderDoc, request))
                                               .GroupBy(PassengerFlightResponseMapper.CreatePassengerFlightKey, StringComparer.OrdinalIgnoreCase)
                                               .Select(group => group.First())
                                               .ToArray();

            await PublishPassengerFlightAsync(passengerDetails, cancellationToken);
        }

        private async Task PublishPassengerFlightAsync(IEnumerable<PassengerRetrievalResponse> passengerDetails, CancellationToken cancellationToken)
        {
            var passengerFlightEvent = new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Source = Constants.Application.PDMEventMessages,
                Type = Constants.Application.FlownPassengerMessage,
                Data = Json.Serialize(passengerDetails).Compress(),
                Subject = Constants.Application.FlownPassengerMessage
            };

            try
            {
                var command = new ServiceBusTopicMessageEventCommand
                {
                    Message = passengerFlightEvent,
                    ServiceBusConnectionString = ConfigReader.Get<string>(Constants.Environment.PassengerFlightServiceBusConnectionString),
                    ServiceBusTopicName = Constants.Application.PDMEventMessages
                };
                await _serviceBusHandler.Handle(command, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Passenger flight event publish to Service Bus failed for event {EventId} with exception type {ExceptionType}",
                    passengerFlightEvent.Id, ex.GetType().Name);
                throw;
            }
        }

    }
}
