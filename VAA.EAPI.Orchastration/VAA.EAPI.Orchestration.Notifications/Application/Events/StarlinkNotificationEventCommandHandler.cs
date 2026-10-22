using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Orchestration.Notifications.Domain.Entities;
using VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.FlightMs;
using VAA.EAPI.Orchestration.Notifications.Mapper;

namespace VAA.EAPI.Orchestration.Notifications.Application.Events
{
    public sealed class StarlinkNotificationEventCommandHandler(ILogger<StarlinkNotificationEventCommandHandler> logger, IRequestHandler<EventRequestInsertCommand, bool> eventStoreRequestInsertCommandHandler, IRequestHandler<EventStoreRequestDeleteCommand, bool> eventStoreRequestDeleteCommandHandler, IRequestHandler<ServiceBusTopicMessageEventCommand, Unit> serviceBusTopicMessageEventCommandHandler, IHttpConnector httpConnector) : IStarlinkNotificationEventCommand
    {
        private readonly ILogger<StarlinkNotificationEventCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IRequestHandler<EventRequestInsertCommand, bool> _eventStoreRequestInsertCommandHandler = eventStoreRequestInsertCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestInsertCommandHandler));
        private readonly IRequestHandler<EventStoreRequestDeleteCommand, bool> _eventStoreRequestDeleteCommandHandler = eventStoreRequestDeleteCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestDeleteCommandHandler));
        private readonly IRequestHandler<ServiceBusTopicMessageEventCommand, Unit> _serviceBusTopicMessageEventCommandHandler = serviceBusTopicMessageEventCommandHandler ?? throw new ArgumentNullException(nameof(serviceBusTopicMessageEventCommandHandler));
        private readonly IHttpConnector _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));

        public async Task Handle(CloudEvent cloudEventReq, CancellationToken cancellationToken)
        {
            var eventId = Guid.NewGuid().ToString();

            try
            {
                var flight = DeserializeFlight(cloudEventReq);

                if (flight is null || flight.TailNumber.IsNullOrWhiteSpace())
                {
                    _logger.LogInformation("Skipping Starlink notification because the delayed flight event has no tail number.");
                    return;
                }

                var isEligible = await IsStarlinkEligibleAsync(flight);

                if (!isEligible)
                {
                    _logger.LogInformation("Skipping Starlink notification for ineligible tail {TailNumber}.", flight.TailNumber);
                    return;
                }

                var request = (FlightOrderDataRequest)flight;
                await SetScheduledDepartureLocalDateTimeAsync(request);

                var cloudEvent = NotifCloudEventMapper.FlightOrderDataEventMapper(request, eventId);

                await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);

                await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand { Message = cloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.NotificationServiceBusConnectionString), ServiceBusTopicName = ConfigReader.Get(Constants.Environment.NotificationTopicName) }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Rollback event from Event store due to error in StarlinkNotificationEventCommandHandler: {ex.Message}");

                await _eventStoreRequestDeleteCommandHandler.Handle(new EventStoreRequestDeleteCommand { Id = eventId, Type = Constants.Constants.FlightOrderDataReqEventSubject }, cancellationToken);

                throw;
            }
        }

        private static StarlinkFlightDetail DeserializeFlight(CloudEvent cloudEventReq)
        {
            if (cloudEventReq.Data is StarlinkFlightDetail flight)
            {
                return flight;
            }

            return Json.Deserialize<StarlinkFlightDetail>(Convert.ToString(cloudEventReq.Data));
        }

        private async Task<bool> IsStarlinkEligibleAsync(StarlinkFlightDetail flight)
        {
            var categoryIndex = ConfigReader.Try("Notifications:StarlinkNotification:CategoryIndex", 0);
            var request = new CheckAircraftCategoryRequest
            {
                TailNumbers = [flight.TailNumber],
                CategoryIndex = categoryIndex
            };

            Dictionary<string, string> headers = new()
            {
                { Constants.Constants.ApimSubscriptionKey, ConfigReader.Get(Constants.Environment.FlightOpsKey) }
            };

            var result = await _httpConnector.PostAsync<CheckAircraftCategoryResult>(
                ConfigReader.Get("Notifications:FlightMs:CheckAircraftCategoryUri"),
                new StringContent(Json.Serialize(request), Encoding.UTF8, "application/json"),
                headers);

            return result?.AircraftCategoryChecks?.Any(check => check.DoesCategoryExist && check.TailNumber.Is(flight.TailNumber)) ?? false;
        }

        private async Task SetScheduledDepartureLocalDateTimeAsync(FlightOrderDataRequest request)
        {
            var referenceUri = ConfigReader.Get<Uri>("Notifications:ReferenceMs:Uri");
            var airports = await _httpConnector.GetAsync<JArray>($"{referenceUri}/airportdetail?airportCodes={request.Departure},{request.Arrival}");
            var departureAirport = airports.First(x => x["airportCode"].ToString().Is(request.Departure));

            request.ScheduledDepartureLocalDateTime = TimeZoneInfo.ConvertTime(request.ScheduledDepartureUtcDateTime, TimeZoneInfoHelper.FindSystemTimeZoneById(departureAirport["timeZone"].ToString()));
        }
    }
}
