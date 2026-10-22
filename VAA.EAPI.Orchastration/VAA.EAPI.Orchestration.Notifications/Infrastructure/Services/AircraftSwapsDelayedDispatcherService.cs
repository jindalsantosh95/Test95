using System;
using VAA.EAPI.Infrastructure.Services.Dispatcher;
using VAA.EAPI.Orchestration.Notifications.Domain.Entities;
using VAA.EAPI.Orchestration.Notifications.Infrastructure.Interfaces;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Services
{
    public sealed class AircraftSwapsDelayedDispatcherService(IDelayedDispatcherService<AircraftMessage> delayedDispatcherService) : IAircraftSwapsDelayedDispatcherService
    {
        private readonly IDelayedDispatcherService<AircraftMessage> _delayedDispatcherService = delayedDispatcherService ?? throw new ArgumentNullException(nameof(delayedDispatcherService));

        public int Count => _delayedDispatcherService.Count;

        public bool Schedule(AircraftMessage item, TimeSpan delay) => _delayedDispatcherService.Schedule(item, delay);
    }
}
