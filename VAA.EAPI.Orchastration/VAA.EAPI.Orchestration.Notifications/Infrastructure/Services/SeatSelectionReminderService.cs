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
    public sealed class SeatSelectionReminderService(ILogger<SeatSelectionReminderService> logger, IHttpConnector httpConnector, ISeatSelectionReminderDelayedDispatcherService delayedDispatcherService) : ISeatSelectionReminderService
    {
        private bool _isWorking;
        private readonly ILogger<SeatSelectionReminderService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IHttpConnector _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        private readonly ISeatSelectionReminderDelayedDispatcherService _delayedDispatcherService = delayedDispatcherService ?? throw new ArgumentNullException(nameof(delayedDispatcherService));

        public async Task SynchronizeAsync()
        {
            if (_isWorking) return;

            _isWorking = true;

            try
            {
                var fromOffset = ConfigReader.Try("Notifications:SeatSelectionReminderService:FromOffset", TimeSpan.FromDays(7));
                var toOffset = ConfigReader.Try("Notifications:SeatSelectionReminderService:ToOffset", new TimeSpan(7, 0, 59, 0));
                var reminderDaysBeforeDeparture = ConfigReader.Try("Notifications:SeatSelectionReminderService:DaysBeforeDeparture", 7);

                var date = DateTime.UtcNow;

                var flightScheduleRequest = new FlightScheduleRequest
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

                var response = (await _httpConnector.PostAsync<IEnumerable<FlightDetail>>(ConfigReader.Get("Notifications:FlightMs:ScheduleUri"), new StringContent(Common.Serialisation.Json.Serialize(flightScheduleRequest)), headers))
                                .Where(flight =>
                                {
                                    var reminderTime = flight.ScheduledDateTimeOfDeparture.AddDays(-reminderDaysBeforeDeparture);
                                    return reminderTime >= from && reminderTime <= to;
                                })
                                .ToArray();

                foreach (var flight in response)
                {
                    var reminderTime = flight.ScheduledDateTimeOfDeparture.AddDays(-reminderDaysBeforeDeparture);
                    var delay = TimeSpan.FromSeconds(Math.Max(3, (reminderTime - from).TotalSeconds));
                    _delayedDispatcherService.Schedule(flight, delay);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error has occurred processing seat selection reminder scheduling, {Message}", ex.Message);
                throw;
            }
            finally
            {
                _isWorking = false;
            }
        }
    }
}
