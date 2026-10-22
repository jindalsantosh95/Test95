using System;
using VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.FlightMs;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Interfaces
{
    public interface IStarlinkEnabledFlightDelayedDispatcherService
    {
        int Count { get; }
        bool Schedule(StarlinkFlightDetail item, TimeSpan delay);
    }
}
