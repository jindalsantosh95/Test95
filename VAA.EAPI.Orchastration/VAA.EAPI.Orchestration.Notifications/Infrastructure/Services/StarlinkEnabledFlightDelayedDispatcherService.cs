using System;
using VAA.EAPI.Infrastructure.Services.Dispatcher;
using VAA.EAPI.Orchestration.Notifications.Infrastructure.Interfaces;
using VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.FlightMs;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Services
{
    public sealed class StarlinkEnabledFlightDelayedDispatcherService(IDelayedDispatcherService<StarlinkFlightDetail> delayedDispatcherService) : IStarlinkEnabledFlightDelayedDispatcherService
    {
        private readonly IDelayedDispatcherService<StarlinkFlightDetail> _delayedDispatcherService = delayedDispatcherService ?? throw new ArgumentNullException(nameof(delayedDispatcherService));

        public int Count => _delayedDispatcherService.Count;

        public bool Schedule(StarlinkFlightDetail item, TimeSpan delay) => _delayedDispatcherService.Schedule(item, delay);
    }
}
