using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Orchestration.Notifications.Infrastructure.Interfaces;
using VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.FlightMs;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Services
{
    public sealed class CheckInOpeningService(ILogger<CheckInOpeningService> logger, IHttpConnector httpConnector, ICheckInOpenDelayedDispatcherService checkInOpenDelayedDispatcherService, IStarlinkEnabledFlightDelayedDispatcherService starlinkEnabledFlightDelayedDispatcherService) : ICheckInOpeningService
    {
        private bool isWorking = false;
        private readonly ILogger<CheckInOpeningService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IHttpConnector _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        private readonly ICheckInOpenDelayedDispatcherService _checkInOpenDelayedDispatcherService = checkInOpenDelayedDispatcherService ?? throw new ArgumentNullException(nameof(checkInOpenDelayedDispatcherService));
        private readonly IStarlinkEnabledFlightDelayedDispatcherService _starlinkEnabledFlightDelayedDispatcherService = starlinkEnabledFlightDelayedDispatcherService ?? throw new ArgumentNullException(nameof(starlinkEnabledFlightDelayedDispatcherService));

        public async Task SynchronizeAsync()
        {
            if (isWorking) return;

            isWorking = true;

            try
            {
                var fromOffset = ConfigReader.Try("Notifications:CheckInOpeningService:FromOffset", TimeSpan.FromDays(1));
                var toOffset = ConfigReader.Try("Notifications:CheckInOpeningService:ToOffset", new TimeSpan(1, 0, 59, 0));
                var starlinkBeforeDeparture = ConfigReader.Try("Notifications:StarlinkNotification:BeforeDeparture", TimeSpan.FromHours(3));

                var date = DateTime.UtcNow;

                var flightScheduleRequest = new FlightScheduleRequest()
                {
                    From = date.Add(fromOffset).ToString("yyyy-MM-ddTHH:mm:00.000").Parse<DateTime>(),
                    To = date.Add(toOffset).ToString("yyyy-MM-ddTHH:mm:00.000").Parse<DateTime>()
                };

                Dictionary<string, string> headers = new()
                {
                    { Constants.Constants.ApimSubscriptionKey, ConfigReader.Get(Constants.Environment.FlightOpsKey) }
                };

                var from = flightScheduleRequest.From - fromOffset;
                var to = flightScheduleRequest.To - fromOffset;

                // aims return all active flights in the period [flightScheduleRequest.From, flightScheduleRequest.To],
                // then we filter only the flights which will be departing within the specified window.
                var response = (await _httpConnector.PostAsync<IEnumerable<FlightDetail>>(ConfigReader.Get("Notifications:FlightMs:ScheduleUri"), new StringContent(Common.Serialisation.Json.Serialize(flightScheduleRequest)), headers))
                                                    .Where(x =>
                                                    {
                                                        DateTime scheduledDateTimeOfDeparture = new(x.Date, x.ScheduledTimeOfDeparture);
                                                        DateTime opening = scheduledDateTimeOfDeparture.AddDays(-1);

                                                        return opening >= from && opening <= to;
                                                    })
                                                    .ToArray();

                // we delay the service bus message until the check-in opening time, which is 24 hours before the
                // scheduled departure. so we calculate the delay for each flight and schedule the message dispatch
                // accordingly.
                foreach (FlightDetail flight in response)
                {
                    DateTime scheduledDateTimeOfDeparture = flight.ScheduledDateTimeOfDeparture;
                    DateTime opening = scheduledDateTimeOfDeparture.AddDays(-1);
                    TimeSpan delay = TimeSpan.FromSeconds(Math.Max(3, (opening - from).TotalSeconds));

                    _checkInOpenDelayedDispatcherService.Schedule(flight, delay);


                    // for Starlink enabled flights, we also want to send a notification before departure,
                    // so we schedule another message dispatch for the Starlink notification. The notification
                    // time is configurable, but it should be at least 3 seconds before departure to avoid
                    // scheduling messages in the past.
                    if (!flight.Registration.IsNullOrWhiteSpace())
                    {
                        DateTime starlinkNotificationTime = scheduledDateTimeOfDeparture.Subtract(starlinkBeforeDeparture);
                        TimeSpan starlinkDelay = TimeSpan.FromSeconds(Math.Max(3, (starlinkNotificationTime - from).TotalSeconds));

                        _starlinkEnabledFlightDelayedDispatcherService.Schedule((StarlinkFlightDetail)flight, starlinkDelay);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error has occurred processing check-in opening reminders, {ex.Message}");

                // we throw the exception back as we are using the timer function retry capability
                throw;
            }
            finally
            {
                isWorking = false;
            }
        }
    }
}
