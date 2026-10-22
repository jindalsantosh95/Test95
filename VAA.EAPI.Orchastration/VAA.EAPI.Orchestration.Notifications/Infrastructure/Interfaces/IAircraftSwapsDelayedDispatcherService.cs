using System;
using VAA.EAPI.Orchestration.Notifications.Domain.Entities;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Interfaces
{    
    public interface IAircraftSwapsDelayedDispatcherService
    {
        int Count { get; }
        bool Schedule(AircraftMessage item, TimeSpan delay);
    }
}