using System;
using VAA.EAPI.Infrastructure.Services.Dispatcher;
using VAA.EAPI.Orchestration.Notifications.Infrastructure.Interfaces;
using VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.FlightMs;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Services
{
    public sealed class SeatSelectionReminderDelayedDispatcherService(IDelayedDispatcherService<SeatSelectionReminderFlightDetail> delayedDispatcherService) : ISeatSelectionReminderDelayedDispatcherService
    {
        private readonly IDelayedDispatcherService<SeatSelectionReminderFlightDetail> _delayedDispatcherService = delayedDispatcherService ?? throw new ArgumentNullException(nameof(delayedDispatcherService));

        public int Count => _delayedDispatcherService.Count;

        public bool Schedule(FlightDetail item, TimeSpan delay) => _delayedDispatcherService.Schedule((SeatSelectionReminderFlightDetail)item, delay);
    }
}
