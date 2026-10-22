using System;
using VAA.EAPI.Infrastructure.Services.Dispatcher;
using VAA.EAPI.Orchestration.Notifications.Infrastructure.Interfaces;
using VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.FlightMs;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Services
{
    public sealed class CheckInOpenDelayedDispatcherService(IDelayedDispatcherService<FlightDetail> delayedDispatcherService) : ICheckInOpenDelayedDispatcherService
    {
        private readonly IDelayedDispatcherService<FlightDetail> _delayedDispatcherService = delayedDispatcherService ?? throw new ArgumentNullException(nameof(delayedDispatcherService));

        public int Count => _delayedDispatcherService.Count;

        public bool Schedule(FlightDetail item, TimeSpan delay) => _delayedDispatcherService.Schedule(item, delay);
    }
}
